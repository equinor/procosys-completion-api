using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Authorization;
using Equinor.ProCoSys.Auth.Permission;
using Equinor.ProCoSys.Auth.Person;
using Equinor.ProCoSys.BlobStorage;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.WebApi.Middleware;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

public sealed class TestFactory : WebApplicationFactory<Startup>
{
    private const string WriterOid = "00000000-0000-0000-0000-000000000001";
    private const string ReaderOid = "00000000-0000-0000-0000-000000000003";
    private const string NoPermissionUserOid = "00000000-0000-0000-0000-000000000666";
    private readonly string _connectionString;
    private readonly string _configPath;
    private readonly Dictionary<UserType, ITestUser> _testUsers = new();
    private readonly List<Action> _teardownList = new();
    private readonly List<IDisposable> _disposables = new();

    public readonly IAzureBlobService BlobStorageMock = Substitute.For<IAzureBlobService>();
    private readonly IPersonApiService _personApiServiceMock = Substitute.For<IPersonApiService>();
    private readonly IPermissionApiService _permissionApiServiceMock = Substitute.For<IPermissionApiService>();
    private readonly ICheckListApiService _checkListApiServiceMock = Substitute.For<ICheckListApiService>();
    private readonly IPublishEndpoint _publishEndpointMock = Substitute.For<IPublishEndpoint>();

    public static string PlantWithAccess => KnownPlantData.PlantA;
    public static string PlantWithoutAccess => KnownPlantData.PlantB;
    public static string Unknown => "UNKNOWN";
    public static Guid ProjectGuidWithAccess => KnownPlantData.ProjectGuidA[KnownPlantData.PlantA];
    public static Guid ProjectGuidWithoutAccess => KnownPlantData.ProjectGuidB[KnownPlantData.PlantA];
    public static Guid CheckListGuid => KnownPlantData.CheckListGuid[KnownPlantData.PlantA];
    public static Guid RaisedByOrgGuid => KnownPlantData.RaisedByOrgGuid[KnownPlantData.PlantA];
    public static Guid ClearingByOrgGuid => KnownPlantData.ClearingByOrgGuid[KnownPlantData.PlantA];
    public static Guid PriorityGuid => KnownPlantData.PriorityGuid[KnownPlantData.PlantA];
    public static Guid SortingGuid => KnownPlantData.SortingGuid[KnownPlantData.PlantA];
    public static Guid TypeGuid => KnownPlantData.TypeGuid[KnownPlantData.PlantA];
    public static string AValidRowVersion => "AAAAAAAAAAA=";
    public static string WrongButValidRowVersion => "AAAAAAAAAAA=";

    public Dictionary<string, KnownTestData> SeededData { get; }

    #region singleton implementation
    private static TestFactory s_instance;
    private static readonly object s_padlock = new();

    public static TestFactory Instance
    {
        get
        {
            if (s_instance is null)
            {
                lock (s_padlock)
                {
                    if (s_instance is null)
                    {
                        s_instance = new TestFactory();
                    }
                }
            }

            return s_instance;
        }
    }

    private TestFactory()
    {
        SeededData = new Dictionary<string, KnownTestData>();

        var projectDir = Directory.GetCurrentDirectory();
        _connectionString = GetTestDbConnectionString(projectDir);
        _configPath = Path.Combine(projectDir, "appsettings.json");

        SetupTestUsers();
    }

    #endregion

    public new void Dispose()
    {
        // Run teardown
        foreach (var action in _teardownList)
        {
            action();
        }

        foreach (var testUser in _testUsers)
        {
            testUser.Value.HttpClient.Dispose();
        }
            
        foreach (var disposable in _disposables)
        {
            try { disposable.Dispose(); } catch { /* Ignore */ }
        }
            
        lock (s_padlock)
        {
            s_instance = null;
        }

        base.Dispose();
    }

    public HttpClient GetHttpClient(UserType userType, string plant)
    {
        var testUser = _testUsers[userType];
            
        SetupPermissionMock(plant, testUser);
            
        UpdatePlantInHeader(testUser.HttpClient, plant);
            
        return testUser.HttpClient;
    }

    public TestProfile GetTestProfile(UserType userType)
        => _testUsers[userType].Profile;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication()
                .AddScheme<IntegrationTestAuthOptions, IntegrationTestAuthHandler>(
                    IntegrationTestAuthHandler.TestAuthenticationScheme, _ => { });

            services.PostConfigureAll<JwtBearerOptions>(jwtBearerOptions =>
                jwtBearerOptions.ForwardAuthenticate = IntegrationTestAuthHandler.TestAuthenticationScheme);

            services.AddScoped(_ => _personApiServiceMock);
            services.AddScoped(_ => _permissionApiServiceMock);
            services.AddScoped(_ => _checkListApiServiceMock);
            services.AddScoped(_ => BlobStorageMock);
            services.AddScoped(_ => _publishEndpointMock);
        });

        builder.ConfigureServices(services =>
        {
            ReplaceRealDbContextWithTestDbContext(services);
                
            CreateSeededTestDatabase(services);
                
            EnsureTestDatabaseDeletedAtTeardown(services);
        });
    }

    private void ReplaceRealDbContextWithTestDbContext(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault
            (d => d.ServiceType == typeof(DbContextOptions<CompletionContext>));

        if (descriptor is not null)
        {
            services.Remove(descriptor);
        }

        services.AddDbContext<CompletionContext>(options 
            => options.UseSqlServer(_connectionString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
    }

    private void CreateSeededTestDatabase(IServiceCollection services)
    {
        using var serviceProvider = services.BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();
            
        var scopeServiceProvider = scope.ServiceProvider;
        var dbContext = scopeServiceProvider.GetRequiredService<CompletionContext>();

        dbContext.Database.EnsureDeleted();

        dbContext.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));

        dbContext.CreateNewDatabaseWithCorrectSchema();

        SeedDataForPlant(dbContext, scopeServiceProvider, KnownPlantData.PlantA);
        SeedDataForPlant(dbContext, scopeServiceProvider, KnownPlantData.PlantB);
    }

    private void SeedDataForPlant(CompletionContext dbContext, IServiceProvider scopeServiceProvider, string plant)
    {
        var knownData = new KnownTestData(plant);
        SeededData.Add(plant, knownData);
        dbContext.Seed(scopeServiceProvider, knownData);
    }

    private void EnsureTestDatabaseDeletedAtTeardown(IServiceCollection services)
        => _teardownList.Add(() =>
        {
            using var dbContext = DatabaseContext(services);
                
            dbContext.Database.EnsureDeleted();
        });

    private CompletionContext DatabaseContext(IServiceCollection services)
    {
        services.AddDbContext<CompletionContext>(options 
            => options.UseSqlServer(_connectionString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

        var sp = services.BuildServiceProvider();
        _disposables.Add(sp);

        var spScope = sp.CreateScope();
        _disposables.Add(spScope);

        return spScope.ServiceProvider.GetRequiredService<CompletionContext>();
    }

    private string GetTestDbConnectionString(string projectDir)
    {
        var dbName = "IntegrationTestsDB";
        var dbPath = Path.Combine(projectDir, $"{dbName}.mdf");
            
        // Set Initial Catalog to be able to delete database!
        return $"Server=(LocalDB)\\MSSQLLocalDB;Initial Catalog={dbName};Integrated Security=true;AttachDbFileName={dbPath}";
    }
        
    private void SetupPermissionMock(string plant, ITestUser testUser)
    {
        _permissionApiServiceMock.GetPermissionsForCurrentUserAsync(plant)
            .Returns(Task.FromResult(testUser.Permissions));
                        
        _permissionApiServiceMock.GetAllOpenProjectsForCurrentUserAsync(plant)
            .Returns(Task.FromResult(testUser.AccessableProjects));

        _permissionApiServiceMock.GetRestrictionRolesForCurrentUserAsync(plant)
            .Returns(Task.FromResult(testUser.Restrictions));
    }

    private void SetupTestUsers()
    {
        var accessablePlants = new List<AccessablePlant>
        {
            new() {Id = KnownPlantData.PlantA, Title = KnownPlantData.PlantATitle, HasAccess = true},
            new() {Id = KnownPlantData.PlantB, Title = KnownPlantData.PlantBTitle}
        };

        var accessableProjects = new List<AccessableProject>
        {
            new()
            {
                ProCoSysGuid = ProjectGuidWithAccess,
                HasAccess = true
            },
            new()
            {
                ProCoSysGuid = ProjectGuidWithoutAccess
            }
        };

        var restrictions = new List<string>
        {
            ClaimsTransformation.NoRestrictions
        };

        SetupAnonymousUser();

        SetupWriterUser(accessablePlants, accessableProjects, restrictions);

        SetupReaderUser(accessablePlants, accessableProjects, restrictions);
    
        SetupNoPermissionUser();
            
        var webHostBuilder = WithWebHostBuilder(builder =>
        {
            // Important to set Test environment so Program.cs don't try to get 
            // config from Azure
            builder.UseEnvironment(EnvironmentExtensions.IntegrationTestEnvironmentName);
            builder.ConfigureAppConfiguration((_, conf) => conf.AddJsonFile(_configPath));
        });

        SetupProCoSysServiceMocks();

        CreateAuthenticatedHttpClients(webHostBuilder);
    }

    private void CreateAuthenticatedHttpClients(WebApplicationFactory<Startup> webHostBuilder)
    {
        foreach (var testUser in _testUsers.Values)
        {
            testUser.HttpClient = webHostBuilder.CreateClient();

            if (testUser.Profile is not null)
            {
                AuthenticateUser(testUser);
            }
        }
    }

    private void SetupProCoSysServiceMocks()
    {
        foreach (var testUser in _testUsers.Values.Where(t => t.Profile is not null))
        {
            if (testUser.AuthProCoSysPerson is not null)
            {
                _personApiServiceMock.TryGetPersonByOidAsync(new Guid(testUser.Profile.Oid))
                    .Returns(Task.FromResult(testUser.AuthProCoSysPerson));
            }
            else
            {
                _personApiServiceMock.TryGetPersonByOidAsync(new Guid(testUser.Profile.Oid))
                    .Returns(Task.FromResult((ProCoSysPerson)null));
            }
            _permissionApiServiceMock.GetAllPlantsForUserAsync(new Guid(testUser.Profile.Oid))
                .Returns(Task.FromResult(testUser.AccessablePlants));
        }

        // Need to mock getting info for current application from Main. This to satisfy VerifyIpoApiClientExists middleware
        var config = new ConfigurationBuilder().AddJsonFile(_configPath).Build();
        var apiObjectId = config["Authenticator:CompletionApiObjectId"];
        if (apiObjectId is null)
        {
            throw new Exception("Config missing: Authenticator:CompletionApiObjectId");
        }
        _personApiServiceMock.TryGetPersonByOidAsync(new Guid(apiObjectId))
            .Returns(Task.FromResult(new ProCoSysPerson
            {
                AzureOid = apiObjectId,
                FirstName = "PCS",
                LastName = "API",
                UserName = "PA",
                Email = "noreply@pcs.com",
                ServicePrincipal = true
            }));
        _checkListApiServiceMock.GetCheckListAsync(PlantWithAccess, CheckListGuid)
            .Returns(new ProCoSys4CheckList("RC", false, ProjectGuidWithAccess));
    }

    // Authenticated client without any roles
    private void SetupNoPermissionUser()
        => _testUsers.Add(UserType.NoPermissionUser,
            new TestUser
            {
                Profile =
                    new TestProfile
                    {
                        FirstName = "No",
                        LastName = "Access",
                        UserName = "NO",
                        Email = "no@pcs.com",
                        Oid = NoPermissionUserOid
                    },
                AccessablePlants = new List<AccessablePlant>
                {
                    new() {Id = KnownPlantData.PlantA, Title = KnownPlantData.PlantATitle},
                    new() {Id = KnownPlantData.PlantB, Title = KnownPlantData.PlantBTitle}
                },
                Permissions = new List<string>(),
                AccessableProjects = new List<AccessableProject>(),
                Restrictions = new List<string>()
            });

    // Authenticated client with necessary roles to read PunchItems
    private void SetupReaderUser(
        List<AccessablePlant> commonAccessablePlants,
        List<AccessableProject> accessableProjects,
        List<string> restrictions)
        => _testUsers.Add(UserType.Reader,
            new TestUser
            {
                Profile =
                    new TestProfile
                    {
                        FirstName = "Ralf",
                        LastName = "Read",
                        UserName = "RR",
                        Email = "rr@pcs.com",
                        Oid = ReaderOid
                    },
                AccessablePlants = commonAccessablePlants,
                Permissions = new List<string>
                {
                    Permissions.PUNCHITEM_READ
                },
                AccessableProjects = accessableProjects,
                Restrictions = restrictions

            });

    // Authenticated client with necessary roles to Create and Update a PunchItem
    private void SetupWriterUser(
        List<AccessablePlant> accessablePlants,
        List<AccessableProject> accessableProjects,
        List<string> restrictions)
        => _testUsers.Add(UserType.Writer,
            new TestUser
            {
                Profile =
                    new TestProfile
                    {
                        FirstName = "Werner",
                        LastName = "Write",
                        UserName = "WW",
                        Email = "ww@pcs.com",
                        Oid = WriterOid
                    },
                AccessablePlants = accessablePlants,
                Permissions = new List<string>
                {
                    Permissions.PUNCHITEM_CREATE,
                    Permissions.PUNCHITEM_CLEAR,
                    Permissions.PUNCHITEM_VERIFY,
                    Permissions.PUNCHITEM_WRITE,
                    Permissions.PUNCHITEM_ATTACH,
                    Permissions.PUNCHITEM_DETACH,
                    Permissions.PUNCHITEM_DELETE,
                    Permissions.PUNCHITEM_READ
                },
                AccessableProjects = accessableProjects,
                Restrictions = restrictions
            });

    private void SetupAnonymousUser() => _testUsers.Add(UserType.Anonymous, new TestUser());

    private static void AuthenticateUser(ITestUser user)
        => user.HttpClient.DefaultRequestHeaders.Add("Authorization", user.Profile.CreateBearerToken());

    private static void UpdatePlantInHeader(HttpClient client, string plant)
    {
        if (client.DefaultRequestHeaders.Contains(CurrentPlantMiddleware.PlantHeader))
        {
            client.DefaultRequestHeaders.Remove(CurrentPlantMiddleware.PlantHeader);
        }

        if (!string.IsNullOrEmpty(plant))
        {
            client.DefaultRequestHeaders.Add(CurrentPlantMiddleware.PlantHeader, plant);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Equinor.ProCoSys.Auth.Authorization;
using Equinor.ProCoSys.Auth.Permission;
using Equinor.ProCoSys.Auth.Person;
using Equinor.ProCoSys.BlobStorage;
using Equinor.ProCoSys.Common.Email;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.FormularTypes;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.Responsibles;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.TagFunctions;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.WebApi.Middleware;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Testing.Platform.Services;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

public class TestFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;
    private readonly Dictionary<UserType, ITestUser> _testUsers = new();
    private readonly List<Action> _teardownList = [];

    public readonly IAzureBlobService BlobStorageMock = Substitute.For<IAzureBlobService>();
    private readonly IPersonApiService _personApiServiceMock = Substitute.For<IPersonApiService>();
    private readonly IPermissionApiService _permissionApiServiceMock = Substitute.For<IPermissionApiService>();
    public readonly ICheckListApiService _checkListApiServiceMock = Substitute.For<ICheckListApiService>();
    private readonly IEmailService _emailServiceMock = Substitute.For<IEmailService>();
    private readonly TokenCredential _tokenCredentialsMock = Substitute.For<TokenCredential>();
    private readonly IFormularTypeApiService _formularTypeApiService = Substitute.For<IFormularTypeApiService>();
    private readonly IResponsibleApiService _responsibleApiService = Substitute.For<IResponsibleApiService>();
    private readonly ITagFunctionApiService _tagFunctionApiService = Substitute.For<ITagFunctionApiService>();

    public static readonly string ResponsibleCodeWithAccess = "RespA";
    public static readonly string ResponsibleCodeWithoutAccess = "RespB";
    public static string PlantWithAccess => KnownData.PlantA;
    public static string PlantWithoutAccess => KnownData.PlantB;
    public static string Unknown => "UNKNOWN";
    public static Guid ProjectGuidWithAccess => KnownData.ProjectGuidA[KnownData.PlantA];
    public static Guid ProjectGuidWithoutAccess => KnownData.ProjectGuidB[KnownData.PlantA];
    public static Guid CheckListGuidNotRestricted => KnownData.CheckListGuidA[KnownData.PlantA];
    public static Guid CheckListGuidRestricted => KnownData.CheckListGuidB[KnownData.PlantA];
    public static Guid CheckListGuidInProjectWithoutAccess => KnownData.CheckListGuidB[KnownData.PlantB];
    public static Guid RaisedByOrgGuid => KnownData.RaisedByOrgGuid[KnownData.PlantA];
    public static Guid ClearingByOrgGuid => KnownData.ClearingByOrgGuid[KnownData.PlantA];
    public static Guid PriorityGuid => KnownData.PriorityGuid[KnownData.PlantA];
    public static Guid SortingGuid => KnownData.SortingGuid[KnownData.PlantA];
    public static Guid TypeGuid => KnownData.TypeGuid[KnownData.PlantA];
    public static Guid OriginalWorkOrderGuid => KnownData.OriginalWorkOrderGuid[KnownData.PlantA];
    public static Guid WorkOrderGuid => KnownData.WorkOrderGuid[KnownData.PlantA];
    public static Guid SWCRGuid => KnownData.SWCRGuid[KnownData.PlantA];
    public static Guid DocumentGuid => KnownData.DocumentGuid[KnownData.PlantA];
    public static string AValidRowVersion => "AAAAAAAAAAA=";
    public static string WrongButValidRowVersion => "AAAAAAAAAAA=";

    public static ProCoSysPerson Person1 = new()
    {
        AzureOid = "asdf-fghj-qwer-tyui",
        Email = "test@email.com",
        FirstName = "Ola",
        LastName = "Hansen",
        UserName = "oha@mail.com"
    };
    public static ProCoSysPerson Person2 = new () {
        AzureOid = "1234-4567-6789-5432",
        Email = "test2@email.com",
        FirstName = "Hans",
        LastName = "Olsen",
        UserName = "hans@mail.com"
    };
    
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
        builder.UseEnvironment(EnvironmentExtensions.IntegrationTestEnvironmentName);
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
            services.AddScoped(_ => _emailServiceMock);
            services.AddScoped(_ => _formularTypeApiService);
            services.AddScoped(_ => _responsibleApiService);
            services.AddScoped(_ => _tagFunctionApiService);
        });

        builder.ConfigureServices(services =>
        {
            ReplaceRealDbContextWithTestDbContext(services);
        
            //replace Azure ServiceBus with in memory MassTransit
            services.AddMassTransitTestHarness();

            ReplaceRealTokenCredentialsWithTestCredentials(services);
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

    private void ReplaceRealTokenCredentialsWithTestCredentials(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault
            (d => d.ServiceType == typeof(TokenCredential));

        if (descriptor is not null)
        {
            services.Remove(descriptor);
        }
        services.AddSingleton(_tokenCredentialsMock);
    }

    private void CreateSeededTestDatabase(IServiceCollection services)
    {
        using var serviceProvider = services.BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();
            
        var scopeServiceProvider = scope.ServiceProvider;
        var dbContext = ServiceProviderServiceExtensions.GetRequiredService<CompletionContext>(scopeServiceProvider);

        dbContext.Database.EnsureDeleted();
        
        dbContext.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));

        dbContext.CreateNewDatabaseWithCorrectSchema();

        dbContext.SeedCurrentUser();

        SeedDataForPlant(dbContext, scopeServiceProvider, KnownData.PlantA);
        SeedDataForPlant(dbContext, scopeServiceProvider, KnownData.PlantB);

        dbContext.SeedPersonData(_testUsers[UserType.Writer].Profile);
        dbContext.SeedPersonData(_testUsers[UserType.RestrictedWriter].Profile);
        dbContext.SeedPersonData(_testUsers[UserType.Reader].Profile);
        dbContext.SeedPerson(Guid.NewGuid().ToString(), "Ola", "Hansen", ImportUserOptions.UserName, "import_user@abc",true);

        dbContext.SeedLabels();
        dbContext.SeedMailTemplates();
    }

    private void SeedDataForPlant(CompletionContext dbContext, IServiceProvider serviceProvider, string plant)
    {
        var knownData = new KnownTestData(plant);
        SeededData.Add(plant, knownData);
        dbContext.SeedPlantData(serviceProvider, knownData);
    }

    private void EnsureTestDatabaseDeletedAtTeardown(IServiceCollection services)
        => _teardownList.Add(() =>
        {
            using var sp = services.BuildServiceProvider();
            using var dbContext = ServiceProviderServiceExtensions.GetRequiredService<CompletionContext>(sp);
                
            dbContext.Database.EnsureDeleted();
        });

    private static string GetTestDbConnectionString(string projectDir)
    {
        var dbName = "IntegrationTestsDB";
        var dbPath = Path.Combine(projectDir, $"{dbName}.mdf");
            
        // Set Initial Catalog to be able to delete database!
        return $"Server=(LocalDB)\\MSSQLLocalDB;Initial Catalog={dbName};Integrated Security=true;AttachDbFileName={dbPath}";
    }
        
    private void SetupPermissionMock(string plant, ITestUser testUser)
    {
        _permissionApiServiceMock.GetPermissionsForCurrentUserAsync(plant, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(testUser.Permissions));
                        
        _permissionApiServiceMock.GetAllOpenProjectsForCurrentUserAsync(plant, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(testUser.AccessableProjects));

        _permissionApiServiceMock.GetRestrictionRolesForCurrentUserAsync(plant, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(testUser.Restrictions));
    }

    public void SetupBlobStorageMock(Uri uri)
        => BlobStorageMock.GetDownloadSasUri(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<string>(),
                Arg.Any<string>())
            .Returns(uri);

    private void SetupTestUsers()
    {
        var accessablePlants = new List<AccessablePlant>
        {
            new() {Id = KnownData.PlantA, Title = KnownData.PlantATitle, HasAccess = true},
            new() {Id = KnownData.PlantB, Title = KnownData.PlantBTitle}
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
        SetupAnonymousUser();
        SetupWriterUser(accessablePlants, accessableProjects);
        SetupRestrictedWriterUser(accessablePlants, accessableProjects);
        SetupReaderUser(accessablePlants, accessableProjects);
        SetupNoPermissionUser();
        SetupProCoSysServiceMocks();
        CreateAuthenticatedHttpClients();
    }

    private void CreateAuthenticatedHttpClients()
    {
        foreach (var testUser in _testUsers.Values)
        {
            testUser.HttpClient = CreateClient();

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
                _personApiServiceMock.TryGetPersonByOidAsync(
                        new Guid(testUser.Profile.Oid),
                        Arg.Any<bool>(),
                        Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult(testUser.AuthProCoSysPerson));
            }
            else
            {
                _personApiServiceMock.TryGetPersonByOidAsync(
                        new Guid(testUser.Profile.Oid), 
                        Arg.Any<bool>(),
                        Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult((ProCoSysPerson)null));
            }
            _permissionApiServiceMock.GetAllPlantsForUserAsync(new Guid(testUser.Profile.Oid), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(testUser.AccessablePlants));
        }

        
        var apiObjectId = "00000000-0000-0000-0000-000000099999"; //needs to match value in appsettings.integrationTests.json
        _personApiServiceMock.TryGetPersonByOidAsync(
                new Guid(apiObjectId),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ProCoSysPerson
            {
                AzureOid = apiObjectId,
                FirstName = "PCS",
                LastName = "API",
                UserName = "PA",
                Email = "noreply@pcs.com",
                ServicePrincipal = true
            }));
        _personApiServiceMock.GetAllPersonsAsync(PlantWithAccess, Arg.Any<CancellationToken>())
            .Returns([
                    Person1,
                    Person2
                ]);
        var checkListNotRestricted = new ProCoSys4CheckList(
            CheckListGuid: CheckListGuidNotRestricted,
            FormularType: "FT",
            FormularGroup: "FG",
            ResponsibleCode: ResponsibleCodeWithAccess,
            TagFunctionCode: "TFC",
            TagFunctionDescription: "TFD",
            TagRegisterCode: "TRC",
            TagRegisterDescription: "TRD",
            IsVoided: false, 
            ProjectGuid: ProjectGuidWithAccess);
        var checkListRestricted = new ProCoSys4CheckList(
            CheckListGuid: CheckListGuidRestricted,
            FormularType: "FT",
            FormularGroup: "FG",
            ResponsibleCode: ResponsibleCodeWithoutAccess,
            TagFunctionCode: "TFC",
            TagFunctionDescription: "TFD",
            TagRegisterCode: "TRC",
            TagRegisterDescription: "TRD",
            IsVoided: false,
            ProjectGuid: ProjectGuidWithAccess);
        var checkListInProjectWithoutAccess = new ProCoSys4CheckList(
            CheckListGuid: CheckListGuidInProjectWithoutAccess,
            FormularType: "FT",
            FormularGroup: "FG",
            ResponsibleCode: ResponsibleCodeWithoutAccess,
            TagFunctionCode: "TFC",
            TagFunctionDescription: "TFD",
            TagRegisterCode: "TRC",
            TagRegisterDescription: "TRD",
            IsVoided: false,
            ProjectGuid: ProjectGuidWithoutAccess);

        _checkListApiServiceMock.GetCheckListAsync(CheckListGuidNotRestricted, Arg.Any<CancellationToken>())
            .Returns(checkListNotRestricted);
        _checkListApiServiceMock.GetCheckListAsync(CheckListGuidRestricted, Arg.Any<CancellationToken>())
            .Returns(checkListRestricted);
        _checkListApiServiceMock.GetCheckListAsync(CheckListGuidInProjectWithoutAccess, Arg.Any<CancellationToken>())
            .Returns(checkListInProjectWithoutAccess);

        _checkListApiServiceMock.GetManyCheckListsAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([]);
        _checkListApiServiceMock.GetManyCheckListsAsync(
                Arg.Is<List<Guid>>(guids => guids.Contains(CheckListGuidNotRestricted)), Arg.Any<CancellationToken>())
            .Returns([checkListNotRestricted]);
        _checkListApiServiceMock.GetManyCheckListsAsync(
                Arg.Is<List<Guid>>(guids => guids.Contains(CheckListGuidRestricted)), Arg.Any<CancellationToken>())
            .Returns([checkListRestricted]);
        _checkListApiServiceMock.GetManyCheckListsAsync(
                Arg.Is<List<Guid>>(guids => guids.Contains(CheckListGuidInProjectWithoutAccess)), Arg.Any<CancellationToken>())
            .Returns([checkListInProjectWithoutAccess]);

        var searchResult = new ProCoSys4CheckListSearchResult(
        [
            new ProCoSys4CheckListSearchDto(
                Guid.NewGuid(), "T", "C", "M", "FT", "FG", "OK", "RC", "TRC", "TRD", "TFC", "TFD", 1, 2, 3)
        ], 10);

        _checkListApiServiceMock.SearchCheckListsAsync(
            ProjectGuidWithAccess,
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>()).Returns(searchResult);

        List<ProCoSys4FormularType> formularTypes = [new ProCoSys4FormularType("T", "Rem", "FG")];
        _formularTypeApiService.GetAllAsync(PlantWithAccess, Arg.Any<CancellationToken>()).Returns(formularTypes);

        List<ProCoSys4Responsible> responsibles = [new ProCoSys4Responsible("R1C", "R1D")];
        _responsibleApiService.GetAllAsync(PlantWithAccess, Arg.Any<CancellationToken>()).Returns(responsibles);

        List<ProCoSys4TagFunction> tagFunctions = [new ProCoSys4TagFunction("TF1C", "TF1R", "R1C", "R1D")];
        _tagFunctionApiService.GetAllAsync(PlantWithAccess, Arg.Any<CancellationToken>()).Returns(tagFunctions);
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
                        Oid = "00000000-0000-0000-0000-000000000666"
                    },
                AccessablePlants = new List<AccessablePlant>
                {
                    new() {Id = KnownData.PlantA, Title = KnownData.PlantATitle},
                    new() {Id = KnownData.PlantB, Title = KnownData.PlantBTitle}
                },
                Permissions = new List<string>(),
                AccessableProjects = new List<AccessableProject>(),
                Restrictions = new List<string>()
            });

    // Authenticated client with necessary roles to read PunchItems
    private void SetupReaderUser(
        List<AccessablePlant> commonAccessablePlants,
        List<AccessableProject> accessableProjects)
        => _testUsers.Add(UserType.Reader,
            new TestUser
            {
                Profile =
                    new TestProfile
                    {
                        FirstName = "Ralf",
                        LastName = "Read",
                        UserName = "RR",
                        Email = "ralf@pcs.com",
                        Oid = "00000000-0000-0000-0000-000000000003"
                    },
                AccessablePlants = commonAccessablePlants,
                Permissions =
                [
                    Permissions.PUNCHITEM_READ,
                    Permissions.MCCR_READ,
                    Permissions.LIBRARY_READ,
                    Permissions.USER_READ,
                    Permissions.WO_READ,
                    Permissions.SWCR_READ,
                    Permissions.DOCUMENT_READ
                ],
                AccessableProjects = accessableProjects,
                Restrictions = [ClaimsTransformation.NoRestrictions]
            });

    // Authenticated client with necessary roles to Create and Update a PunchItem
    // Is also Superuser
    // Not restricted to content
    private void SetupWriterUser(
        List<AccessablePlant> accessablePlants,
        List<AccessableProject> accessableProjects)
        => _testUsers.Add(UserType.Writer,
            new TestUser
            {
                Profile =
                    new TestProfile
                    {
                        FirstName = "Werner",
                        LastName = "Write",
                        UserName = "WW",
                        Email = "werner@pcs.com",
                        Oid = "00000000-0000-0000-0000-000000000001",
                        Superuser = true
                    },
                AccessablePlants = accessablePlants,
                Permissions =
                [
                    Permissions.PUNCHITEM_CREATE,
                    Permissions.PUNCHITEM_CLEAR,
                    Permissions.PUNCHITEM_VERIFY,
                    Permissions.PUNCHITEM_WRITE,
                    Permissions.PUNCHITEM_DELETE,
                    Permissions.PUNCHITEM_READ,
                    Permissions.MCCR_READ,
                    Permissions.LIBRARY_READ,
                    Permissions.USER_READ
                ],
                AccessableProjects = accessableProjects,
                Restrictions = [ClaimsTransformation.NoRestrictions]
            });

    // Authenticated client with necessary roles to Create and Update a PunchItem
    // Restricted to content with responsible code = ResponsibleCodeAvailableForRestrictedWriter
    private void SetupRestrictedWriterUser(
        List<AccessablePlant> accessablePlants,
        List<AccessableProject> accessableProjects)
        => _testUsers.Add(UserType.RestrictedWriter,
            new TestUser
            {
                Profile =
                    new TestProfile
                    {
                        FirstName = "Reidar",
                        LastName = "Resttricted",
                        UserName = "RR",
                        Email = "reidar@pcs.com",
                        Oid = "00000000-0000-0000-0000-000000000009"
                    },
                AccessablePlants = accessablePlants,
                Permissions =
                [
                    Permissions.PUNCHITEM_CREATE,
                    Permissions.PUNCHITEM_CLEAR,
                    Permissions.PUNCHITEM_VERIFY,
                    Permissions.PUNCHITEM_WRITE,
                    Permissions.PUNCHITEM_DELETE,
                    Permissions.PUNCHITEM_READ,
                    Permissions.MCCR_READ,
                    Permissions.LIBRARY_READ
                ],
                AccessableProjects = accessableProjects,
                Restrictions = [ResponsibleCodeWithAccess]
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

using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.Imports;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.TieImport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.Import;

[TestClass]
public class PunchItemImportHandlerTest(IServiceProvider serviceProvider)
{
    private IPunchItemImportHandler _dut;

    [TestInitialize]
    public void Setup()
    {
         serviceProvider = TestFactory.Instance.Services;
        _dut = serviceProvider.GetRequiredService<IPunchItemImportHandler>();
    }
    
    [TestMethod]
    public async Task Handle_ShouldReturnFailedResult_WhenTryingToCreatePunchForExistingExternalItemNo()   
    {
        //Arrange 
        var knownPunch = TestFactory.Instance.SeededData[TestFactory.PlantWithAccess].PunchItemA;
        const string TagNo = "testTagNo";
        const string Responsible = "AnyoneReally";
        const string FormType = "SuperDuperFormType";
        TestFactory.Instance._checkListApiServiceMock.GetCheckListGuidByMetaInfoAsync(knownPunch.Plant,TagNo,Responsible,FormType,default)
            .Returns(knownPunch.CheckListGuid); 
        
        var tiObject = new TIObject {
            Classification = "PROCOSYS",
            Method = "CREATE",
            ObjectName = "PUNCHITEM",
            ObjectClass = "PUNCHITEM", 
            Site = knownPunch.Plant ,
            Project = knownPunch.Project.Name
        };
        tiObject.Attributes.Add(new TIAttribute { Name = PunchObjectAttributes.ExternalPunchItemNo, Value = knownPunch.ExternalItemNo });
        tiObject.Attributes.Add(new TIAttribute { Name = PunchObjectAttributes.TagNo, Value = TagNo });
        tiObject.Attributes.Add(new TIAttribute { Name = PunchObjectAttributes.Responsible, Value = Responsible });
        tiObject.Attributes.Add(new TIAttribute { Name = PunchObjectAttributes.FormType, Value = FormType });
        tiObject.Attributes.Add(new TIAttribute { Name = PunchObjectAttributes.Description, Value = "Interesting fact about tomatoes" });
        tiObject.Attributes.Add(new TIAttribute { Name = PunchObjectAttributes.Status, Value = "PA" });
        tiObject.Attributes.Add(new TIAttribute { Name = PunchObjectAttributes.RaisedByOrganization, Value = "COM" });
        tiObject.Attributes.Add(new TIAttribute { Name = PunchObjectAttributes.ClearedByOrganization, Value = "ENG" });
        
        //Act
        var result = await _dut.ImportMessage(tiObject);
       
        //Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result == MessageResults.Failed);
        Assert.IsTrue(result.Logs.Count == 1);
        Assert.IsTrue(result.ErrorMessage.Contains("ExternalItemNo already exists in project!"));
    }
    
      [TestMethod]
    public async Task Handle_ShouldReturnSuccess_WhenImportingAValidMessage()   
    {
        //Arrange 
        var knownPunch = TestFactory.Instance.SeededData[TestFactory.PlantWithAccess].PunchItemA;
        const string TagNo = "testTagNo";
        const string Responsible = "AnyoneReally";
        const string FormType = "SuperDuperFormType";
        TestFactory.Instance._checkListApiServiceMock.GetCheckListGuidByMetaInfoAsync(knownPunch.Plant,TagNo,Responsible,FormType,default)
            .Returns(knownPunch.CheckListGuid); 
        
        var tiObject = new TIObject {
            Classification = "PROCOSYS",
            Method = "CREATE",
            ObjectName = "PUNCHITEM",
            ObjectClass = "PUNCHITEM", 
            Site = knownPunch.Plant ,
            Project = knownPunch.Project.Name
        };
        const string NotUsedExternalItemNo = "NotUsedNumber";
        tiObject.Attributes.Add(new TIAttribute { Name = PunchObjectAttributes.ExternalPunchItemNo, Value = NotUsedExternalItemNo });
        tiObject.Attributes.Add(new TIAttribute { Name = PunchObjectAttributes.TagNo, Value = TagNo });
        tiObject.Attributes.Add(new TIAttribute { Name = PunchObjectAttributes.Responsible, Value = Responsible });
        tiObject.Attributes.Add(new TIAttribute { Name = PunchObjectAttributes.FormType, Value = FormType });
        tiObject.Attributes.Add(new TIAttribute { Name = PunchObjectAttributes.Description, Value = "Interesting fact about tomatoes" });
        tiObject.Attributes.Add(new TIAttribute { Name = PunchObjectAttributes.Status, Value = "PA" });
        tiObject.Attributes.Add(new TIAttribute { Name = PunchObjectAttributes.RaisedByOrganization, Value = "COM" });
        tiObject.Attributes.Add(new TIAttribute { Name = PunchObjectAttributes.ClearedByOrganization, Value = "ENG" });
        
        var context = serviceProvider.GetRequiredService<CompletionContext>();
        Assert.IsTrue(context.PunchItems.Count(p => p.ExternalItemNo == NotUsedExternalItemNo)==0);
        //Act
        var result = await _dut.ImportMessage(tiObject);
       
        //Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result == MessageResults.Successful);
        
        
        Assert.IsTrue(context.PunchItems.Count(p => p.ExternalItemNo == NotUsedExternalItemNo)==1);
    }

    
}

﻿using Equinor.ProCoSys.Completion.TieImport.CommonLib;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Tests;

[TestClass]
public class ImportHandlerTests
{
    IImportSchemaMapper _importSchemaMapperMock;
    ImportHandler _dut;

    [TestInitialize]
    public void Setup()
    {
        _importSchemaMapperMock = Substitute.For<IImportSchemaMapper>();
        _dut = new ImportHandler(_importSchemaMapperMock, Substitute.For<ILogger<ImportHandler>>());
    }

    [TestMethod]
    public void Handle_ShouldReturnErrorResultIfMapperFails()
    {
        // Arrange
        var mappingResultNotSuccess = new MappingResult(
            new TIMessageResult
            {
                ErrorMessage = "Mapping failed", 
                Result = MessageResults.Failed
            }
        );

        _importSchemaMapperMock.Map(Arg.Any<TIInterfaceMessage>()).Returns(mappingResultNotSuccess);

        //Act
        var result = _dut.Handle(new TIInterfaceMessage());

        //Assert
        Assert.AreEqual(MessageResults.Failed, result.Results[0].Result);
        Assert.AreEqual("Mapping failed", result.Results[0].ErrorMessage);
    }

    [TestMethod]
    public void Handle_ShouldReturnSuccessResultIfMapperSucceeds()
    {
        // Arrange
        var mappingResultIsSuccess = new MappingResult(
            new TIMessageResult
            {
                ErrorMessage = null,
                Result = MessageResults.Successful
            }
        );

        _importSchemaMapperMock.Map(Arg.Any<TIInterfaceMessage>()).Returns(mappingResultIsSuccess);

        //Act
        var result = _dut.Handle(new TIInterfaceMessage());

        //Assert
        Assert.AreEqual(MessageResults.Successful, result.Results[0].Result);
        Assert.IsNull(result.Results[0].ErrorMessage);
    }
}
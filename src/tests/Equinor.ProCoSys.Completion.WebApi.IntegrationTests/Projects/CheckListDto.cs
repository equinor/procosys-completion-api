using System;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.Projects;

public record CheckListDto(
    Guid CheckListGuid,
    string TagNo,
    string CommPkgNo,
    string McPkgNo,
    string FormularType,
    string Status,
    string ResponsibleCode,
    string TagRegisterCode,
    string TagRegisterDescription,
    string TagFunctionCode,
    string TagFunctionDescription,
    short? SheetNo,
    short? SubSheetNo,
    short? RevisionNo);

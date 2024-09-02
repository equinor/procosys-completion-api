using System;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.CheckLists;

public record CheckListDetailsDto(
    Guid CheckListGuid,
    string FormularType,
    string ResponsibleCode,
    string TagRegisterCode,
    string TagRegisterDescription,
    string TagFunctionCode,
    string TagFunctionDescription,
    Guid ProjectGuid);

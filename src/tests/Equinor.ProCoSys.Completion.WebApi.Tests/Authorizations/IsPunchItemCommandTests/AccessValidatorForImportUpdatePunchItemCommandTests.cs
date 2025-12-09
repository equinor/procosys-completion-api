// using System;
// using Equinor.ProCoSys.Completion.Command.PunchItemCommands.ImportUpdatePunchItemCommand;
// using Equinor.ProCoSys.Completion.Domain;
// using Equinor.ProCoSys.Completion.Domain.Imports;
// using Microsoft.VisualStudio.TestTools.UnitTesting;
//
// namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;
//
// [TestClass]
// public class
//     AccessValidatorForImportUpdatePunchItemCommandTests : AccessValidatorForCommandNeedAccessTests<
//     UpdatePunchItemCommandForImport>
// {
//     protected override UpdatePunchItemCommandForImport GetPunchItemCommandWithAccessToBothProjectAndContent()
//         => new(Guid.Empty, Guid.Empty, string.Empty, PunchItemGuidWithAccessToProjectAndContent, null!, null!,
//             new Optional<ActionByPerson?>(), new Optional<ActionByPerson?>(), new Optional<ActionByPerson?>(), null!);
//
//     protected override UpdatePunchItemCommandForImport GetPunchItemCommandWithAccessToProjectButNotContent()
//         => new(Guid.Empty, Guid.Empty, string.Empty, PunchItemGuidWithAccessToProjectButNotContent, null!, null!,
//             new Optional<ActionByPerson?>(), new Optional<ActionByPerson?>(), new Optional<ActionByPerson?>(), null!);
//
//     protected override UpdatePunchItemCommandForImport GetPunchItemCommandWithoutAccessToProject()
//         => new(Guid.Empty, Guid.Empty, string.Empty, PunchItemGuidWithoutAccessToProject, null!, null!,
//             new Optional<ActionByPerson?>(), new Optional<ActionByPerson?>(), new Optional<ActionByPerson?>(), null!);
// }

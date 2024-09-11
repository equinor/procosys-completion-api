// using System;
// using Equinor.ProCoSys.Completion.Command.PunchItemCommands.ImportUpdatePunchItem;
// using Equinor.ProCoSys.Completion.Domain;
// using Equinor.ProCoSys.Completion.Domain.Imports;
// using Microsoft.VisualStudio.TestTools.UnitTesting;
//
// namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;
//
// [TestClass]
// public class
//     AccessValidatorForImportUpdatePunchItemCommandTests : AccessValidatorForCommandNeedAccessTests<
//     ImportUpdatePunchItemCommand>
// {
//     protected override ImportUpdatePunchItemCommand GetPunchItemCommandWithAccessToBothProjectAndContent()
//         => new(Guid.Empty, Guid.Empty, string.Empty, PunchItemGuidWithAccessToProjectAndContent, null!, null!,
//             new Optional<ActionByPerson?>(), new Optional<ActionByPerson?>(), new Optional<ActionByPerson?>(), null!);
//
//     protected override ImportUpdatePunchItemCommand GetPunchItemCommandWithAccessToProjectButNotContent()
//         => new(Guid.Empty, Guid.Empty, string.Empty, PunchItemGuidWithAccessToProjectButNotContent, null!, null!,
//             new Optional<ActionByPerson?>(), new Optional<ActionByPerson?>(), new Optional<ActionByPerson?>(), null!);
//
//     protected override ImportUpdatePunchItemCommand GetPunchItemCommandWithoutAccessToProject()
//         => new(Guid.Empty, Guid.Empty, string.Empty, PunchItemGuidWithoutAccessToProject, null!, null!,
//             new Optional<ActionByPerson?>(), new Optional<ActionByPerson?>(), new Optional<ActionByPerson?>(), null!);
// }

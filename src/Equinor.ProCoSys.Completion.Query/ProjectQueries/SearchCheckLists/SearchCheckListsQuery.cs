﻿using System;
using Equinor.ProCoSys.Completion.Domain;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.ProjectQueries.SearchCheckLists;

public class SearchCheckListsQuery(
    Guid projectGuid,
    string? tagNoContains,
    string? responsibleCode,
    string? registerAndTagFunctionCode,
    string? formularType,
    int? currentPage,
    int? itemsPerPage)
    : INeedProjectAccess, IRequest<Result<SearchResultDto>>
{
    public Guid ProjectGuid { get; } = projectGuid;
    public string? TagNoContains { get; } = tagNoContains;
    public string? ResponsibleCode { get; } = responsibleCode;
    public string? RegisterAndTagFunctionCode { get; } = registerAndTagFunctionCode;
    public string? FormularType { get; } = formularType;
    public int? CurrentPage { get; } = currentPage;
    public int? ItemsPerPage { get; } = itemsPerPage;
    public Guid GetProjectGuidForAccessCheck() => ProjectGuid;
}
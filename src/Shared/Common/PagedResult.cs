namespace Shared.Common;

/// <summary>
/// Standard pagination envelope for list endpoints (see agents.md).
/// </summary>
public sealed class PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }

    public required int Page { get; init; }

    public required int PageSize { get; init; }

    public required int TotalCount { get; init; }
}

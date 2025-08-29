namespace Nextech.Hn.Api.Models;

/// <summary>
/// Represents a paginated result containing a collection of items and total count
/// </summary>
/// <typeparam name="T">The type of items in the collection</typeparam>
/// <param name="Total">The total number of items across all pages</param>
/// <param name="Items">The read-only collection of items for the current page</param>
public record PagedResult<T>(
    int Total,
    IReadOnlyList<T> Items
);

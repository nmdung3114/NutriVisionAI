namespace NutriVision.Application.Common;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public static class Paging
{
    public static (int page, int pageSize) Normalize(int page, int pageSize, int defaultSize = 20, int maxSize = 100)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = defaultSize;
        if (pageSize > maxSize) pageSize = maxSize;
        return (page, pageSize);
    }
}

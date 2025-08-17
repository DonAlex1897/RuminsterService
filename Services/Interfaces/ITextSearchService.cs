using System.Linq.Expressions;

namespace RuminsterBackend.Services.Interfaces
{
    public interface ITextSearchService
    {
        string Normalize(string query);
        IQueryable<T> ApplyContainsFilter<T>(IQueryable<T> source, string query, params Expression<Func<T, string?>>[] fields);
        IQueryable<T> ApplyPagination<T>(IQueryable<T> source, int? offset, int? limit, int maxLimit = 50);
    }
}

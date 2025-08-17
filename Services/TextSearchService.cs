using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RuminsterBackend.Services.Interfaces;

namespace RuminsterBackend.Services
{
    public class TextSearchService : ITextSearchService
    {
        public string Normalize(string query)
        {
            return (query ?? string.Empty).Trim();
        }

        public IQueryable<T> ApplyContainsFilter<T>(IQueryable<T> source, string query, params Expression<Func<T, string?>>[] fields)
        {
            var q = Normalize(query).ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(q) || fields == null || fields.Length == 0)
            {
                return source;
            }

            Expression? orExpression = null;
            var param = Expression.Parameter(typeof(T), "e");

            foreach (var field in fields)
            {
                var body = new ReplaceParameterVisitor(field.Parameters[0], param).Visit(field.Body);

                var coalesce = Expression.Coalesce(body!, Expression.Constant(string.Empty));
                var toLower = Expression.Call(coalesce, typeof(string).GetMethod("ToLower", Type.EmptyTypes)!);
                var contains = Expression.Call(toLower, typeof(string).GetMethod("Contains", new[] { typeof(string) })!, Expression.Constant(q));

                orExpression = orExpression == null ? contains : Expression.OrElse(orExpression, contains);
            }

            if (orExpression == null)
            {
                return source;
            }

            var lambda = Expression.Lambda<Func<T, bool>>(orExpression, param);
            return source.Where(lambda);
        }

        public IQueryable<T> ApplyPagination<T>(IQueryable<T> source, int? offset, int? limit, int maxLimit = 50)
        {
            if (offset.HasValue && offset.Value > 0)
            {
                source = source.Skip(offset.Value);
            }

            if (limit.HasValue)
            {
                var eff = Math.Min(Math.Max(limit.Value, 0), maxLimit);
                source = source.Take(eff);
            }

            return source;
        }

        private class ReplaceParameterVisitor(ParameterExpression from, ParameterExpression to) : ExpressionVisitor
        {
            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == from ? to : base.VisitParameter(node);
            }
        }
    }
}

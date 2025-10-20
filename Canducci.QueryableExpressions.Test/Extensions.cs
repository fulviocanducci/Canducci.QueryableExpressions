using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
namespace Canducci.QueryableExpressions.Test
{
    public static class Extensions
    {
        //public static string GetSqlWithParameters<T>(IQueryable<T> query)
        //{
        //    var sql = query.ToQueryString();
        //    var context = query.GetService<ICurrentDbContext>().Context;
        //    var command = context.Database.GetDbConnection().CreateCommand();

        //    var compiledQuery = query.Provider.Execute<IEnumerable<T>>(query.Expression);
        //    var relationalCommandCache = query.GetType()
        //        .GetField("_relationalCommandCache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
        //        ?.GetValue(query);

        //    if (relationalCommandCache == null)
        //        return sql; // fallback sem parâmetros

        //    var relationalCommand = relationalCommandCache
        //        .GetType()
        //        .GetProperty("RelationalCommand")
        //        ?.GetValue(relationalCommandCache);

        //    var parameterValues = relationalCommandCache
        //        .GetType()
        //        .GetProperty("ParameterValues")
        //        ?.GetValue(relationalCommandCache) as IReadOnlyDictionary<string, object>;

        //    if (parameterValues != null && parameterValues.Any())
        //    {
        //        sql += "\n\n-- Parameters:";
        //        foreach (var param in parameterValues)
        //            sql += $"\n-- {param.Key} = {param.Value}";
        //    }

        //    return sql;
        //}

    }
}

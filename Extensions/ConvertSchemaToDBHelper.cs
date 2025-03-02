using Microsoft.EntityFrameworkCore.Storage;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure.Internal;
using Pomelo.EntityFrameworkCore.MySql.Storage.Internal;

namespace RuminsterBackend.Extensions
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "Temporary fix while migrating Warehouse")]
    public class ConvertSchemaToDBHelper : MySqlSqlGenerationHelper
    {
        // Based on: https://github.com/dotnet/efcore/issues/22971#issuecomment-707768344
        public ConvertSchemaToDBHelper(
            RelationalSqlGenerationHelperDependencies dependencies,
            IMySqlOptions options)
            : base(dependencies, options)
        {
        }

        public override string GetObjectName(string name, string schema)
        {
            return name; // Removes schema here
        }

        public override string GetSchemaName(string name, string schema)
        {
            return schema; // <-- this is the first part that is needed to map schemas to databases
        }
    }
}

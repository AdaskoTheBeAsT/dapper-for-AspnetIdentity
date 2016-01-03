namespace Lu.Dapper.Extensions.Adapters
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    using global::Dapper;

    public partial class SqlServerAdapter : ISqlAdapter
    {
        public int Insert(
            IDbConnection connection,
            IDbTransaction transaction,
            int? commandTimeout,
            string tableName,
            string columnList,
            string parameterList,
            IEnumerable<PropertyInfo> keyProperties,
            object entityToInsert,
            bool autoIncrement)
        {
            var cmd = string.Format("insert into {0} ({1}) values ({2})", tableName, columnList, parameterList);

            connection.Execute(cmd, entityToInsert, transaction: transaction, commandTimeout: commandTimeout);

            if (autoIncrement)
            {
                // NOTE: would prefer to use IDENT_CURRENT('tablename') or IDENT_SCOPE but these are not available on SQLCE
                var r = connection.Query("select scope_identity() id", transaction: transaction, commandTimeout: commandTimeout);
                var id = (int)r.First().id;
                var keyProperty = keyProperties.FirstOrDefault();
                if (keyProperty != null)
                {
                    keyProperty.SetValue(entityToInsert, id, null);
                }

                return id;
            }

            return 0;
        }

        public async Task<int> InsertAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            int? commandTimeout,
            string tableName,
            string columnList,
            string parameterList,
            IEnumerable<PropertyInfo> keyProperties,
            object entityToInsert,
            bool autoIncrement)
        {
            var cmd = string.Format("insert into {0} ({1}) values ({2})", tableName, columnList, parameterList);

            await
                connection.ExecuteAsync(cmd, entityToInsert, transaction: transaction, commandTimeout: commandTimeout)
                    .ConfigureAwait(false);
            if (autoIncrement)
            {
                var r =
                    await
                    connection.QueryAsync<dynamic>(
                        "select scope_identity() id",
                        transaction: transaction,
                        commandTimeout: commandTimeout).ConfigureAwait(false);
                var id = (int)r.First().id;
                var keyProperty = keyProperties.FirstOrDefault();
                if (keyProperty != null)
                {
                    keyProperty.SetValue(entityToInsert, id, null);
                }

                return id;
            }

            // NOTE: would prefer to use IDENT_CURRENT('tablename') or IDENT_SCOPE but these are not available on SQLCE
            return 0;
        }
    }
}

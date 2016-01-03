namespace Lu.Dapper.Extensions.Adapters
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    using global::Dapper;

    public partial class MySqlAdapter : ISqlAdapter
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
                // http://stackoverflow.com/questions/8517841/mysql-last-insert-id-connector-net
                var id = (int)(long)connection.Query<ulong>(
                        "SELECT CAST(LAST_INSERT_ID() AS UNSIGNED INTEGER)",
                        transaction: transaction,
                        commandTimeout: commandTimeout).FirstOrDefault();

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
                // http://stackoverflow.com/questions/8517841/mysql-last-insert-id-connector-net
                var r =
                    await
                    connection.QueryAsync<ulong>(
                        "SELECT CAST(LAST_INSERT_ID() AS UNSIGNED INTEGER)",
                        transaction: transaction,
                        commandTimeout: commandTimeout).ConfigureAwait(false);
                var id = (int)(long)r.FirstOrDefault();
                var keyProperty = keyProperties.FirstOrDefault();
                if (keyProperty != null)
                {
                    keyProperty.SetValue(entityToInsert, id, null);
                }

                return id;
            }

            return 0;
        }
    }
}

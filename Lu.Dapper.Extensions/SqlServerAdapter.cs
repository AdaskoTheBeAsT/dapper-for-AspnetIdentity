﻿namespace Lu.Dapper.Extensions
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
            string cmd = string.Format("insert into {0} ({1}) values ({2})", tableName, columnList, parameterList);

            connection.Execute(cmd, entityToInsert, transaction: transaction, commandTimeout: commandTimeout);

            if (autoIncrement)
            {
                // NOTE: would prefer to use IDENT_CURRENT('tablename') or IDENT_SCOPE but these are not available on SQLCE
                var r = connection.Query("select @@IDENTITY id", transaction: transaction, commandTimeout: commandTimeout);
                int id = (int)r.First().id;
                if (keyProperties.Any())
                {
                    keyProperties.First().SetValue(entityToInsert, id, null);
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
            string cmd = string.Format("insert into {0} ({1}) values ({2})", tableName, columnList, parameterList);

            await
                connection.ExecuteAsync(cmd, entityToInsert, transaction: transaction, commandTimeout: commandTimeout)
                    .ConfigureAwait(false);
            if (autoIncrement)
            {
                var r =
                    await
                    connection.QueryAsync<dynamic>(
                        "select @@IDENTITY id",
                        transaction: transaction,
                        commandTimeout: commandTimeout).ConfigureAwait(false);
                int id = (int)r.First().id;
                if (keyProperties.Any())
                {
                    keyProperties.First().SetValue(entityToInsert, id, null);
                }

                return id;
            }

            // NOTE: would prefer to use IDENT_CURRENT('tablename') or IDENT_SCOPE but these are not available on SQLCE
            return 0;
        }
    }
}
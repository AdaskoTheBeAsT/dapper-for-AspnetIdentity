namespace Lu.Dapper.Extensions.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using System.Threading.Tasks;

    using global::Dapper;

    public static partial class SqlMapperExtensions
    {
        public static IEnumerable<T> Find<T>(
            this IDbConnection connection,
            Expression<Func<T, bool>> expression,
            IDbTransaction transaction = null,
            int? commandTimeout = null)
        {
            var result = GetDynamicQuery(expression);
            return connection.Query<T>(result.Sql, (object)result.Param, transaction, commandTimeout: commandTimeout);
        }

        public static Task<IEnumerable<T>> FindAsync<T>(
            this IDbConnection connection,
            Expression<Func<T, bool>> expression,
            IDbTransaction transaction = null,
            int? commandTimeout = null)
        {
            var result = GetDynamicQuery(expression);
            return connection.QueryAsync<T>(result.Sql, (object)result.Param, transaction, commandTimeout);
        }

        public static int Remove<T>(
            this IDbConnection connection,
            Expression<Func<T, bool>> expression,
            IDbTransaction transaction = null,
            int? commandTimeout = null)
        {
            var result = GetDynamicRemove(expression);
            return connection.Execute(result.Sql, (object)result.Param, transaction, commandTimeout);
        }

        public static Task<int> RemoveAsync<T>(
            this IDbConnection connection,
            Expression<Func<T, bool>> expression,
            IDbTransaction transaction = null,
            int? commandTimeout = null)
        {
            var result = GetDynamicRemove(expression);
            return connection.ExecuteAsync(result.Sql, (object)result.Param, transaction, commandTimeout);
        }

        public static string GetTableName<T>(this IDbConnection connection)
        {
            var type = typeof(T);
            return GetTableName(type);
        }

        private static LinqToSqlResult GetDynamicQuery<T>(Expression<Func<T, bool>> expression)
        {
            var headSql = "SELECT * FROM";
            return GetDynamicSql(expression, headSql);
        }

        private static LinqToSqlResult GetDynamicRemove<T>(Expression<Func<T, bool>> expression)
        {
            var headSql = "DELETE FROM";
            return GetDynamicSql(expression, headSql);
        }

        private static LinqToSqlResult GetDynamicSql<T>(Expression<Func<T, bool>> expression, string headSql)
        {
            var tableName = GetTableName(typeof(T));
            var queryProperties = new List<QueryParameter>();
            var body = (BinaryExpression)expression.Body;
            IDictionary<string, object> expando = new ExpandoObject();
            var builder = new StringBuilder();

            // walk the tree and build up a list of query parameter objects
            // from the left and right branches of the expression tree
            WalkTree(body, ExpressionType.Default, ref queryProperties);

            // convert the query parms into a SQL string and dynamic property object
            builder.Append(headSql + " ");
            builder.Append(tableName);
            builder.Append(" WHERE ");

            for (int i = 0; i < queryProperties.Count(); i++)
            {
                QueryParameter item = queryProperties[i];

                builder.Append(
                    !string.IsNullOrEmpty(item.LinkingOperator) && i > 0
                        ? string.Format(
                            "{0} {1} {2} @{1} ",
                            item.LinkingOperator,
                            item.PropertyName,
                            item.QueryOperator)
                        : string.Format("{0} {1} @{0} ", item.PropertyName, item.QueryOperator));

                expando[item.PropertyName] = item.PropertyValue;
            }

            return new LinqToSqlResult(builder.ToString().TrimEnd(), expando);
        }

        /// <summary>
        /// Walks the tree.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="linkingType">Type of the linking.</param>
        /// <param name="queryProperties">The query properties.</param>
        private static void WalkTree(
            BinaryExpression body,
            ExpressionType linkingType,
            ref List<QueryParameter> queryProperties)
        {
            if (body.NodeType != ExpressionType.AndAlso && body.NodeType != ExpressionType.OrElse)
            {
                string propertyName = GetPropertyName(body);
                dynamic propertyValue = body.Right;
                string opr = GetOperator(body.NodeType);
                string link = GetOperator(linkingType);

                queryProperties.Add(new QueryParameter(link, propertyName, GetValue(propertyValue), opr));
            }
            else
            {
                WalkTree((BinaryExpression)body.Left, body.NodeType, ref queryProperties);
                WalkTree((BinaryExpression)body.Right, body.NodeType, ref queryProperties);
            }
        }

        private static object GetValue(dynamic propertyValue)
        {
            if (propertyValue.NodeType != System.Linq.Expressions.ExpressionType.Constant)
            {
                MemberExpression member = (MemberExpression)propertyValue;
                var objectMember = Expression.Convert(member, typeof(object));

                var getterLambda = Expression.Lambda<Func<object>>(objectMember);

                var getter = getterLambda.Compile();

                return getter();
            }

            return propertyValue.Value;
        }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <returns>The property name for the property expression.</returns>
        private static string GetPropertyName(BinaryExpression body)
        {
            string propertyName = body.Left.ToString().Split(new char[] { '.' })[1];

            if (body.Left.NodeType == ExpressionType.Convert)
            {
                // hack to remove the trailing ) when convering.
                propertyName = propertyName.Replace(")", string.Empty);
            }

            return propertyName;
        }

        /// <summary>
        /// Gets the operator.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// The expression types SQL server equivalent operator.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        private static string GetOperator(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.NotEqual:
                    return "!=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.AndAlso:
                case ExpressionType.And:
                    return "AND";
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return "OR";
                case ExpressionType.Default:
                    return string.Empty;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}

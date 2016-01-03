namespace Lu.Dapper.Extensions
{
    using System;

    public class LinqToSqlResult
    {
        /// <summary>
        /// The _result
        /// </summary>
        private readonly Tuple<string, dynamic> _result;

        /// <summary>
        /// Initializes a new instance of the <see cref="LinqToSqlResult" /> class.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="param">The param.</param>
        public LinqToSqlResult(string sql, dynamic param)
        {
            _result = new Tuple<string, dynamic>(sql, param);
        }

        /// <summary>
        /// Gets the SQL.
        /// </summary>
        /// <value>
        /// The SQL.
        /// </value>
        public string Sql
        {
            get
            {
                return _result.Item1;
            }
        }

        /// <summary>
        /// Gets the param.
        /// </summary>
        /// <value>
        /// The param.
        /// </value>
        public dynamic Param
        {
            get
            {
                return _result.Item2;
            }
        }
    }
}

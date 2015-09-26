namespace Lu.Dapper.Extensions
{
    using System.Data;

    public class DapperRepository<T> : DapperRepository<T, object>, IRepository<T> where T : class
    {
        public DapperRepository(IDbConnection connection)
            : this(connection, null)
        {
        }

        public DapperRepository(IDbConnection connection, IDbTransaction transaction = null)
            : base(connection, transaction)
        {
        }
    }
}

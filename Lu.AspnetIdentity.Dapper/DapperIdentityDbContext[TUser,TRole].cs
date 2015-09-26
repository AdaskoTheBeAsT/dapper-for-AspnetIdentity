namespace Lu.AspnetIdentity.Dapper
{
    using System.Data;

    public class DapperIdentityDbContext<TUser, TRole> : IdentityDbContext<TUser, TRole, string, string>
        where TUser : IdentityUser<string> where TRole : IdentityRole<string>
    {
        public DapperIdentityDbContext(IDbConnection connection)
            : base(connection, null)
        {
        }

        public DapperIdentityDbContext(IDbConnection connection, IDbTransaction transaction)
            : base(connection, transaction)
        {
        }
    }
}

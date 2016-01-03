namespace Lu.AspnetIdentity.Dapper
{
    using System.Data;

    public class DapperIdentityDbContext<TUser, TRole, TUserRole, TUserLogin, TUserClaim> :
        IdentityDbContext<TUser, TRole, TUserRole, TUserLogin, TUserClaim, string, string, string, string, string>
        where TUser : IdentityUser<string>
        where TRole : IdentityRole<string>
        where TUserRole : IdentityUserRole<string, string, string>
        where TUserLogin : IdentityUserLogin<string, string>
        where TUserClaim : IdentityUserClaim<string, string>
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

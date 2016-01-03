namespace Lu.AspnetIdentity.Dapper
{
    public class RoleStore<TUser, TRole, TUserRole, TUserLogin, TUserClaim> : RoleStore<TUser, TRole, TUserRole, TUserLogin, TUserClaim, string, string, string, string, string>
        where TUser : IdentityUser 
        where TRole : IdentityRole
        where TUserRole : IdentityUserRole<string, string, string>
        where TUserLogin : IdentityUserLogin<string, string>
        where TUserClaim : IdentityUserClaim<string, string>
    {
        public RoleStore(DapperIdentityDbContext<TUser, TRole, TUserRole, TUserLogin, TUserClaim> dbContext)
            : base(dbContext)
        {
        }
    }
}

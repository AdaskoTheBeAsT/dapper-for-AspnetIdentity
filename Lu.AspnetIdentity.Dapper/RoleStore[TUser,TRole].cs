namespace Lu.AspnetIdentity.Dapper
{
    public class RoleStore<TUser, TRole> : RoleStore<TUser, TRole, string, string>
        where TUser : IdentityUser where TRole : IdentityRole
    {
        public RoleStore(DapperIdentityDbContext<TUser, TRole> dbContext)
            : base(dbContext)
        {
        }
    }
}

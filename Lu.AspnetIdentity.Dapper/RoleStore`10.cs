namespace Lu.AspnetIdentity.Dapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNet.Identity;

    public class RoleStore<TUser, TRole, TUserRole, TUserLogin, TUserClaim, TUserKey, TRoleKey, TUserRoleKey, TUserLoginKey, TUserClaimKey> : IQueryableRoleStore<TRole, TRoleKey>,
                                                           IRoleStore<TRole, TRoleKey>,
                                                           IDisposable
        where TUser : IdentityUser<TUserKey> 
        where TRole : IdentityRole<TRoleKey>
        where TUserRole : IdentityUserRole<TUserRoleKey, TUserKey, TRoleKey>
        where TUserLogin : IdentityUserLogin<TUserLoginKey, TUserKey>
        where TUserClaim : IdentityUserClaim<TUserClaimKey, TUserKey>
    {
        private readonly IdentityDbContext<TUser, TRole, TUserRole, TUserLogin, TUserClaim, TUserKey, TRoleKey, TUserRoleKey, TUserLoginKey, TUserClaimKey> _dbContent;

        public RoleStore(IdentityDbContext<TUser, TRole, TUserRole, TUserLogin, TUserClaim, TUserKey, TRoleKey, TUserRoleKey, TUserLoginKey, TUserClaimKey> dbContent)
        {
            if (dbContent == null)
            {
                throw new ArgumentNullException("dbContent is null");
            }

            _dbContent = dbContent;
        }

        public IdentityDbContext<TUser, TRole, TUserRole, TUserLogin, TUserClaim, TUserKey, TRoleKey, TUserRoleKey, TUserLoginKey, TUserClaimKey> DbContent
        {
            get
            {
                return _dbContent;
            }
        }

        public virtual IQueryable<TRole> Roles
        {
            get
            {
                var roles = GetAllRoles();

                // task.Start();
                // task.Wait();
                // var roles = task.Result;
                return roles.AsQueryable<TRole>();
            }
        }

        public virtual async Task CreateAsync(TRole role)
        {
            if (role == null)
            {
                throw new ArgumentNullException("role");
            }

            await DbContent.RoleRepository.InsertAsync(role);

            // return Task.FromResult<object>(null);
        }

        public virtual async Task DeleteAsync(TRole role)
        {
            if (role == null)
            {
                throw new ArgumentNullException("role");
            }

            await DbContent.RoleRepository.RemoveAsync(role);

            // return Task.FromResult<object>(null);
        }

        public virtual async Task<TRole> FindByIdAsync(TRoleKey roleId)
        {
            return await DbContent.RoleRepository.GetAsync(roleId);
        }

        public virtual async Task<TRole> FindByNameAsync(string roleName)
        {
            var roles = await DbContent.RoleRepository.FindAsync(c => c.Name == roleName);
            return roles.FirstOrDefault();
        }

        public virtual async Task UpdateAsync(TRole role)
        {
            if (role == null)
            {
                throw new ArgumentNullException("role");
            }

            await DbContent.RoleRepository.UpdateAsync(role);
        }

        public virtual void Dispose()
        {
            // if (DbContent.RoleRepository != null && DbContent.RoleRepository is DapperRepository<TRole>)
            // {
            //    var rr = DbContent.RoleRepository as DapperRepository<TRole>;
            //    rr.Dispose();
            // }
        }

        private IEnumerable<TRole> GetAllRoles()
        {
            try
            {
                var tableName = DbContent.RoleRepository.GetTableName<TRole>();
                var sql = string.Format("SELECT * FROM {0}", tableName);
                var roles = DbContent.RoleRepository.Query<TRole>(sql, null);
                return roles;
            }
            catch
            {
                throw;
            }
        }

        // private async Task<IEnumerable<TRole>> GetAllRoles()
        // {
        //    try
        //    {
        //        var tableName = DbContent.RoleRepository.GetTableName<TRole>();
        //        var sql = string.Format("select * from {0}", tableName);
        //        var roles = await DbContent.RoleRepository.QueryAsync<TRole>(sql, null);
        //        return roles;
        //    }
        //    catch(Exception ex)
        //    {
        //        throw ex;
        //    }
        // }
    }
}

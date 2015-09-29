namespace Lu.AspnetIdentity.Dapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNet.Identity;

    public class UserStore<TUser, TRole> : UserStore<TUser, TRole, string, string>, IUserStore<TUser>
        where TUser : IdentityUser where TRole : IdentityRole
    {
        public UserStore(DapperIdentityDbContext<TUser, TRole> dbContext)
            : base(dbContext)
        {
        }

        public override async Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            var result = await DbContent.UserLoginRepository.FindAsync(c => c.UserId == user.Id);
            IList<UserLoginInfo> rs = new List<UserLoginInfo>();
            if (result != null && result.Any())
            {
                foreach (var login in result)
                {
                    rs.Add(new UserLoginInfo(login.LoginProvider, login.ProviderKey));
                }
            }

            return rs;
        }

        public override async Task<IList<System.Security.Claims.Claim>> GetClaimsAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            var result = await DbContent.UserClaimRepository.FindAsync(c => c.UserId == user.Id);
            IList<System.Security.Claims.Claim> rs = new List<System.Security.Claims.Claim>();
            if (result != null && result.Any())
            {
                foreach (var login in result)
                {
                    rs.Add(new System.Security.Claims.Claim(login.ClaimType, login.ClaimValue));
                }
            }

            return rs;
        }

        public override async Task RemoveClaimAsync(TUser user, System.Security.Claims.Claim claim)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (claim == null)
            {
                throw new ArgumentNullException("claim");
            }

            await
                DbContent.UserClaimRepository.RemoveAsync(
                    c => c.UserId == user.Id && c.ClaimValue == claim.Value && c.ClaimType == claim.ValueType);
        }

        public override async Task RemoveFromRoleAsync(TUser user, string roleName)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (string.IsNullOrEmpty(roleName))
            {
                throw new ArgumentNullException("roleName");
            }

            var roles = await DbContent.RoleRepository.FindAsync(c => c.Name == roleName);
            if (roles != null && roles.Any())
            {
                var role = roles.First();
                await DbContent.UserRoleRepository.RemoveAsync(c => c.RoleId == role.Id && c.UserId == user.Id);
            }
        }

        public override async Task RemoveLoginAsync(TUser user, UserLoginInfo login)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (login == null)
            {
                throw new ArgumentNullException("login");
            }

            await
                DbContent.UserLoginRepository.RemoveAsync(
                    c =>
                    c.UserId == user.Id && c.ProviderKey == login.ProviderKey && c.LoginProvider == login.LoginProvider);
        }
    }
}

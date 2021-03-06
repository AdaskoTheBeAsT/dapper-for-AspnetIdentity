﻿namespace Lu.AspnetIdentity.Dapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using global::Dapper;

    using Microsoft.AspNet.Identity;

    public class UserStore<TUser, TRole, TUserRole, TUserLogin, TUserClaim, TUserKey, TRoleKey, TUserRoleKey, TUserLoginKey, TUserClaimKey> : IUserLoginStore<TUser, TUserKey>,
                                                       IUserClaimStore<TUser, TUserKey>,
                                                       IUserRoleStore<TUser, TUserKey>,
                                                       IUserPasswordStore<TUser, TUserKey>,
                                                       IUserSecurityStampStore<TUser, TUserKey>,
                                                       IQueryableUserStore<TUser, TUserKey>,
                                                       IUserEmailStore<TUser, TUserKey>,
                                                       IUserPhoneNumberStore<TUser, TUserKey>,
                                                       IUserTwoFactorStore<TUser, TUserKey>,
                                                       IUserLockoutStore<TUser, TUserKey>,
                                                       IUserStore<TUser, TUserKey>
        where TUser : IdentityUser<TUserKey>
        where TRole : IdentityRole<TRoleKey>
        where TUserRole : IdentityUserRole<TUserRoleKey, TUserKey, TRoleKey>
        where TUserLogin : IdentityUserLogin<TUserLoginKey, TUserKey>
        where TUserClaim : IdentityUserClaim<TUserClaimKey, TUserKey>
    {
        private IdentityDbContext<TUser, TRole, TUserRole, TUserLogin, TUserClaim, TUserKey, TRoleKey, TUserRoleKey, TUserLoginKey, TUserClaimKey> _dbContent;

        public UserStore(IdentityDbContext<TUser, TRole, TUserRole, TUserLogin, TUserClaim, TUserKey, TRoleKey, TUserRoleKey, TUserLoginKey, TUserClaimKey> dbContent)
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

        public virtual IQueryable<TUser> Users
        {
            get
            {
                var users = GetAllUsers();

                //// task.Start();
                //// task.Wait();
                //// var users=task.Result;
                return users.AsQueryable<TUser>();
            }
        }

        public virtual async Task AddLoginAsync(TUser user, UserLoginInfo login)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (login == null)
            {
                throw new ArgumentNullException("login");
            }

            var userLogin = (TUserLogin)new IdentityUserLogin<TUserLoginKey, TUserKey>()
            {
                UserId = user.Id,
                ProviderKey = login.ProviderKey,
                LoginProvider = login.LoginProvider
            };
            await DbContent.UserLoginRepository.InsertAsync(userLogin);
        }

        public virtual async Task<TUser> FindAsync(UserLoginInfo login)
        {
            var task = await
                DbContent.UserLoginRepository.FindAsync(
                    c => c.LoginProvider == login.LoginProvider && c.ProviderKey == login.ProviderKey);

            var logines = task;
            if (logines != null && logines.Any())
            {
                var userId = logines.First().UserId;
                return await DbContent.UserRepository.GetAsync(userId);
            }

            return null;
        }

        public virtual async Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            var result = await DbContent.UserLoginRepository.FindAsync(c => (object)c.UserId == (object)user.Id);
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

        public virtual async Task RemoveLoginAsync(TUser user, UserLoginInfo login)
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
                    (object)c.UserId == (object)user.Id && c.ProviderKey == login.ProviderKey
                    && c.LoginProvider == login.LoginProvider);
        }

        public virtual async Task CreateAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            await DbContent.UserRepository.InsertAsync(user);
        }

        public virtual async Task DeleteAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            await DbContent.UserRepository.RemoveAsync(user);
        }

        public virtual async Task<TUser> FindByIdAsync(TUserKey userId)
        {
            if (object.Equals(userId, default(TUserKey)))
            {
                throw new ArgumentNullException("userId");
            }

            return await DbContent.UserRepository.GetAsync(userId);
        }

        public virtual async Task<TUser> FindByNameAsync(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentNullException("userName");
            }

            var users = await DbContent.UserRepository.FindAsync(c => c.UserName == userName);
            return users.FirstOrDefault();
        }

        public virtual async Task UpdateAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            await DbContent.UserRepository.UpdateAsync(user);
        }

        public virtual void Dispose()
        {
            if (_dbContent != null)
            {
                _dbContent.Dispose();
                _dbContent = null;
            }

            // if (DbContent.UserRepository != null && DbContent.UserRepository is DapperRepository<TUser>)
            // {
            //    var rr = DbContent.UserRepository as DapperRepository<TUser>;
            //    rr.Dispose();
            // }
        }

        public virtual async Task AddClaimAsync(TUser user, System.Security.Claims.Claim claim)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (claim == null)
            {
                throw new ArgumentNullException("claim");
            }

            var userClaim = (TUserClaim)new IdentityUserClaim<TUserClaimKey, TUserKey>()
            {
                UserId = user.Id,
                ClaimType = claim.ValueType,
                ClaimValue = claim.Value
            };
            await DbContent.UserClaimRepository.InsertAsync(userClaim);
        }

        public virtual async Task<IList<System.Security.Claims.Claim>> GetClaimsAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            var result = await DbContent.UserClaimRepository.FindAsync(c => (object)c.UserId == (object)user.Id);
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

        public virtual async Task RemoveClaimAsync(TUser user, System.Security.Claims.Claim claim)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (claim == null)
            {
                throw new ArgumentNullException("claim");
            }

            await
                DbContent.UserClaimRepository.RemoveAsync(
                    c =>
                    (object)c.UserId == (object)user.Id && c.ClaimValue == claim.Value && c.ClaimType == claim.ValueType);
        }

        public virtual async Task AddToRoleAsync(TUser user, string roleName)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (string.IsNullOrEmpty(roleName))
            {
                throw new ArgumentException("Argument cannot be null or empty: roleName.");
            }

            var roles = await DbContent.RoleRepository.FindAsync(c => c.Name == roleName);
            if (roles != null && roles.Any())
            {
                var roleId = roles.First().Id;
                var userRole = (TUserRole)new IdentityUserRole<TUserRoleKey, TUserKey, TRoleKey>() { RoleId = roleId, UserId = user.Id, };
                await DbContent.UserRoleRepository.InsertAsync(userRole);
            }
        }

        public virtual async Task<IList<string>> GetRolesAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            var sql = GetRoleNamesSqlString();
            var roles = await DbContent.RoleRepository.QueryAsync<string>(sql, user);
            IList<string> rs = new List<string>();
            if (roles != null)
            {
                foreach (var role in roles)
                {
                    rs.Add(role);
                }
            }

            return rs;
        }

        public virtual async Task<bool> IsInRoleAsync(TUser user, string roleName)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (string.IsNullOrEmpty(roleName))
            {
                throw new ArgumentNullException("roleName");
            }

            var sql = GetIsInRoleSqlString();
            var param = new DynamicParameters();
            param.Add("UserId", user.Id);
            param.Add("Name", roleName);
            var roles = await DbContent.RoleRepository.QueryAsync<string>(sql, param);
            if (roles != null && roles.Any())
            {
                return true;
            }

            return false;
        }

        public virtual async Task RemoveFromRoleAsync(TUser user, string roleName)
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

                await
                    DbContent.UserRoleRepository.RemoveAsync(
                        c => (object)c.RoleId == (object)role.Id && (object)c.UserId == (object)user.Id);
            }
        }

        public virtual async Task<string> GetPasswordHashAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return await Task.FromResult(user.PasswordHash);
            //// var u = await DbContent.UserRepository.GetAsync(user.Id);
            //// if (u != null)
            ////    return u.PasswordHash;
            //// return string.Empty;
        }

        public virtual async Task<bool> HasPasswordAsync(TUser user)
        {
            var passwordHash = await GetPasswordHashAsync(user);
            return string.IsNullOrEmpty(passwordHash);
        }

        public virtual async Task SetPasswordHashAsync(TUser user, string passwordHash)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            user.PasswordHash = passwordHash;
            await DbContent.UserRepository.UpdateAsync(user);
        }

        public virtual async Task<string> GetSecurityStampAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return await Task.FromResult(user.SecurityStamp);
        }

        public virtual async Task SetSecurityStampAsync(TUser user, string stamp)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            user.SecurityStamp = stamp;
            await DbContent.UserRepository.UpdateAsync(user);
        }

        public virtual async Task<TUser> FindByEmailAsync(string email)
        {
            var users = await DbContent.UserRepository.FindAsync(c => c.Email == email);
            if (users != null && users.Any())
            {
                return users.First();
            }

            return null;
        }

        public virtual async Task<string> GetEmailAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return await Task.FromResult(user.Email);
        }

        public virtual async Task<bool> GetEmailConfirmedAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return await Task.FromResult(user.EmailConfirmed);
        }

        public virtual async Task SetEmailAsync(TUser user, string email)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            user.Email = email;
            await DbContent.UserRepository.UpdateAsync(user);
        }

        public virtual async Task SetEmailConfirmedAsync(TUser user, bool confirmed)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            user.EmailConfirmed = confirmed;
            await DbContent.UserRepository.UpdateAsync(user);
        }

        public virtual async Task<string> GetPhoneNumberAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return await Task.FromResult(user.PhoneNumber);
        }

        public virtual async Task<bool> GetPhoneNumberConfirmedAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return await Task.FromResult(user.PhoneNumberConfirmed);
        }

        public virtual async Task SetPhoneNumberAsync(TUser user, string phoneNumber)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            user.PhoneNumber = phoneNumber;
            await DbContent.UserRepository.UpdateAsync(user);
        }

        public virtual async Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            user.PhoneNumberConfirmed = confirmed;
            await DbContent.UserRepository.UpdateAsync(user);
        }

        public virtual async Task<bool> GetTwoFactorEnabledAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return await Task.FromResult(user.TwoFactorEnabled);
        }

        public virtual async Task SetTwoFactorEnabledAsync(TUser user, bool enabled)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            user.TwoFactorEnabled = enabled;
            await DbContent.UserRepository.UpdateAsync(user);
        }

        public virtual async Task<int> GetAccessFailedCountAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return await Task.FromResult(user.AccessFailedCount);
        }

        public virtual async Task<bool> GetLockoutEnabledAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return await Task.FromResult(user.LockoutEnabled);
        }

        public virtual async Task<DateTimeOffset> GetLockoutEndDateAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return
                await
                Task.FromResult(
                    user.LockoutEndDateUtc.HasValue
                        ? new DateTimeOffset(DateTime.SpecifyKind(user.LockoutEndDateUtc.Value, DateTimeKind.Utc))
                        : new DateTimeOffset());
        }

        public virtual async Task<int> IncrementAccessFailedCountAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            user.AccessFailedCount++;
            await DbContent.UserRepository.UpdateAsync(user);
            return user.AccessFailedCount;
        }

        public virtual async Task ResetAccessFailedCountAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            user.AccessFailedCount = 0;
            await DbContent.UserRepository.UpdateAsync(user);
        }

        public virtual async Task SetLockoutEnabledAsync(TUser user, bool enabled)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            user.LockoutEnabled = enabled;
            await DbContent.UserRepository.UpdateAsync(user);
        }

        public virtual async Task SetLockoutEndDateAsync(TUser user, DateTimeOffset lockoutEnd)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            user.LockoutEndDateUtc = lockoutEnd.UtcDateTime;
            await DbContent.UserRepository.UpdateAsync(user);
        }

        private string GetRoleNamesSqlString()
        {
            var roletableName = DbContent.RoleRepository.GetTableName<TRole>();
            var userRoleTableName = DbContent.UserRoleRepository.GetTableName<IdentityUserRole<TUserKey, TRoleKey>>();
            return string.Format(
                "select name from {0} a inner join {1} b on a.Id=b.RoleId where b.UserId=@Id",
                roletableName,
                userRoleTableName);
        }

        private string GetIsInRoleSqlString()
        {
            var roletableName = DbContent.RoleRepository.GetTableName<TRole>();
            var userRoleTableName = DbContent.UserRoleRepository.GetTableName<IdentityUserRole<TUserKey, TRoleKey>>();
            return
                string.Format(
                    "select name from {0} a inner join {1} b on a.Id=b.RoleId where b.UserId=@UserId and a.Name=@Name",
                    roletableName,
                    userRoleTableName);
        }

        private IEnumerable<TUser> GetAllUsers()
        {
            var users = DbContent.UserRepository.Find(c => c.UserName != " ");
            return users;
        }

        // private async Task<IEnumerable<TUser>> GetAllUsers()
        // {
        //    var users=await DbContent.UserRepository.FindAsync(c => c.UserName != " ");
        //    return users;
        // }
    }
}

namespace Lu.AspnetIdentity.Dapper
{
    using System;
    using System.Data;

    using Lu.Dapper.Extensions;

    public class IdentityDbContext<TUser, TRole, TUserRole, TUserLogin, TUserClaim, TUserKey, TRoleKey, TUserRoleKey, TUserLoginKey, TUserClaimKey> : IDisposable
        where TUser : IdentityUser<TUserKey> 
        where TRole : IdentityRole<TRoleKey>
        where TUserRole : IdentityUserRole<TUserRoleKey, TUserKey, TRoleKey>
        where TUserLogin : IdentityUserLogin<TUserLoginKey, TUserKey>
        where TUserClaim : IdentityUserClaim<TUserClaimKey, TUserKey>
    {
        private readonly IDbConnection _conn;
        private readonly IDbTransaction _transaction;
        private IRepository<TUser, TUserKey> _userRepository;
        private IRepository<TUserLogin, TUserLoginKey> _userLoginRepository;
        private IRepository<TUserClaim, TUserClaimKey> _userClaimRepository;
        private IRepository<TRole, TRoleKey> _roleRepository;
        private IRepository<TUserRole, TUserRoleKey> _userRoleRepository;

        public IdentityDbContext(IDbConnection connection)
            : this(connection, null)
        {
        }

        public IdentityDbContext(IDbConnection connection, IDbTransaction transaction)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            _conn = connection;
            _transaction = transaction;
        }

        public IRepository<TUser, TUserKey> UserRepository
        {
            get
            {
                if (_userRepository != null)
                {
                    return _userRepository;
                }

                _userRepository = new DapperRepository<TUser, TUserKey>(DbConnection, DbTransaction);
                return _userRepository;
            }
        }

        public IRepository<TRole, TRoleKey> RoleRepository
        {
            get
            {
                if (_roleRepository != null)
                {
                    return _roleRepository;
                }

                _roleRepository = new DapperRepository<TRole, TRoleKey>(DbConnection, DbTransaction);
                return _roleRepository;
            }
        }

        public IRepository<TUserRole, TUserRoleKey> UserRoleRepository
        {
            get
            {
                if (_userRoleRepository != null)
                {
                    return _userRoleRepository;
                }

                _userRoleRepository = new DapperRepository<TUserRole, TUserRoleKey>(DbConnection, DbTransaction);

                return _userRoleRepository;
            }
        }

        public IRepository<TUserLogin, TUserLoginKey> UserLoginRepository
        {
            get
            {
                if (_userLoginRepository != null)
                {
                    return _userLoginRepository;
                }

                _userLoginRepository = new DapperRepository<TUserLogin, TUserLoginKey>(DbConnection, DbTransaction);

                return _userLoginRepository;
            }
        }

        public IRepository<TUserClaim, TUserClaimKey> UserClaimRepository
        {
            get
            {
                if (_userClaimRepository != null)
                {
                    return _userClaimRepository;
                }

                _userClaimRepository = new DapperRepository<TUserClaim, TUserClaimKey>(DbConnection, DbTransaction);

                return _userClaimRepository;
            }
        }

        public IDbConnection DbConnection
        {
            get
            {
                return _conn;
            }
        }

        public IDbTransaction DbTransaction
        {
            get
            {
                return _transaction;
            }
        }

        public void Dispose()
        {
            if (_conn.State != ConnectionState.Closed)
            {
                _conn.Close();
            }
        }
    }
}

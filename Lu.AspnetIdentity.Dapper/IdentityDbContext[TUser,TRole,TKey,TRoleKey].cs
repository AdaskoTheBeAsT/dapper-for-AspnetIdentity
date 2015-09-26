namespace Lu.AspnetIdentity.Dapper
{
    using System;
    using System.Data;

    using Lu.Dapper.Extensions;

    public class IdentityDbContext<TUser, TRole, TKey, TRoleKey> : IDisposable
        where TUser : IdentityUser<TKey> where TRole : IdentityRole<TRoleKey>
    {
        private readonly IDbConnection _conn;
        private readonly IDbTransaction _transaction;
        private IRepository<TUser, TKey> _userRepository;
        private IRepository<IdentityUserLogin<TKey>> _userLoginRepository;
        private IRepository<IdentityUserClaim<TKey>> _userClaimRepository;
        private IRepository<TRole, TRoleKey> _roleRepository;
        private IRepository<IdentityUserRole<TKey, TRoleKey>> _userRoleRepository;

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

        public IRepository<TUser, TKey> UserRepository
        {
            get
            {
                if (_userRepository != null)
                {
                    return _userRepository;
                }

                _userRepository = new DapperRepository<TUser, TKey>(DbConnection, DbTransaction);
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

        public IRepository<IdentityUserRole<TKey, TRoleKey>> UserRoleRepository
        {
            get
            {
                if (_userRoleRepository != null)
                {
                    return _userRoleRepository;
                }

                _userRoleRepository = new DapperRepository<IdentityUserRole<TKey, TRoleKey>>(DbConnection, DbTransaction);

                return _userRoleRepository;
            }
        }

        public IRepository<IdentityUserLogin<TKey>> UserLoginRepository
        {
            get
            {
                if (_userLoginRepository != null)
                {
                    return _userLoginRepository;
                }

                _userLoginRepository = new DapperRepository<IdentityUserLogin<TKey>>(DbConnection, DbTransaction);

                return _userLoginRepository;
            }
        }

        public IRepository<IdentityUserClaim<TKey>> UserClaimRepository
        {
            get
            {
                if (_userClaimRepository != null)
                {
                    return _userClaimRepository;
                }

                _userClaimRepository = new DapperRepository<IdentityUserClaim<TKey>>(DbConnection, DbTransaction);

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

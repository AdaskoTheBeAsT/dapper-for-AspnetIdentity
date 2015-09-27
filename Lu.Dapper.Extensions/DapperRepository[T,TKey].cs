namespace Lu.Dapper.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using global::Dapper;

    using Utils;

    public class DapperRepository<T, TKey> : IRepository<T, TKey>, IDisposable
        where T : class
    {
        private readonly IDbConnection _connection;

        private readonly IDbTransaction _transaction;

        public DapperRepository(IDbConnection connection)
            : this(connection, null)
        {
        }

        public DapperRepository(IDbConnection connection, IDbTransaction transaction = null)
        {
            _connection = connection;
            _transaction = transaction;
        }

        public long Insert(T item)
        {
            try
            {
                EnsureConnectionOpen();
                return _connection.Insert<T>(item, _transaction);
            }
            finally
            {
                EnsureConnectionClosed();
            }
        }

        public async Task<int> InsertAsync(T item)
        {
            try
            {
                EnsureConnectionOpen();
                return await _connection.InsertAsync<T>(item, _transaction);
            }
            finally
            {
                EnsureConnectionClosed();
            }
        }

        public bool Update(T item)
        {
            try
            {
                EnsureConnectionOpen();
                return _connection.Update<T>(item, _transaction);
            }
            finally
            {
                EnsureConnectionClosed();
            }
        }

        public async Task<bool> UpdateAsync(T item)
        {
            try
            {
                EnsureConnectionOpen();
                return await _connection.UpdateAsync<T>(item, _transaction);
            }
            finally
            {
                EnsureConnectionClosed();
            }
        }

        public bool Remove(T item)
        {
            try
            {
                EnsureConnectionOpen();
                return _connection.Delete<T>(item, _transaction);
            }
            finally
            {
                EnsureConnectionClosed();
            }
        }

        public async Task<bool> RemoveAsync(T item)
        {
            try
            {
                EnsureConnectionOpen();
                return await _connection.DeleteAsync<T>(item, _transaction);
            }
            finally
            {
                EnsureConnectionClosed();
            }
        }

        public bool RemoveAll()
        {
            try
            {
                EnsureConnectionOpen();
                return _connection.DeleteAll<T>(_transaction);
            }
            finally
            {
                EnsureConnectionClosed();
            }
        }

        public async Task<bool> RemoveAllAsync()
        {
            try
            {
                EnsureConnectionOpen();
                return await _connection.DeleteAllAsync<T>(_transaction);
            }
            finally
            {
                EnsureConnectionClosed();
            }
        }

        public T Get(TKey id)
        {
            try
            {
                EnsureConnectionOpen();
                return _connection.Get<T>(id, _transaction);
            }
            finally
            {
                EnsureConnectionClosed();
            }
        }

        public async Task<T> GetAsync(TKey id)
        {
            try
            {
                EnsureConnectionOpen();
                return await _connection.GetAsync<T>(id, _transaction);
            }
            finally
            {
                EnsureConnectionClosed();
            }
        }

        public void EnsureConnectionClosed()
        {
            if (_connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }
        }

        public void Dispose()
        {
            if (_connection != null && _connection.State != ConnectionState.Closed)
            {
                _connection.Close();

                // _connection = null;
                // _connection.Dispose();
            }
        }

        public void Remove(Expression<Func<T, bool>> predicate)
        {
            try
            {
                EnsureConnectionOpen();
                _connection.Remove<T>(predicate, _transaction);
            }
            finally
            {
                EnsureConnectionClosed();
            }
        }

        public async Task RemoveAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                EnsureConnectionOpen();
                await _connection.RemoveAsync<T>(predicate, _transaction);
            }
            finally
            {
                EnsureConnectionClosed();
            }
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
        {
            try
            {
                EnsureConnectionOpen();
                return _connection.Find<T>(predicate, _transaction);
            }
            finally
            {
                EnsureConnectionClosed();
            }
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                EnsureConnectionOpen();
                return await _connection.FindAsync<T>(predicate, _transaction);
            }
            finally
            {
                EnsureConnectionClosed();
            }
        }

        public IEnumerable<TS> Query<TS>(string sql, object param)
        {
            try
            {
                EnsureConnectionOpen();
                return _connection.Query<TS>(sql, param, _transaction);
            }
            finally
            {
                EnsureConnectionClosed();
            }
        }

        public async Task<IEnumerable<TS>> QueryAsync<TS>(string sql, object param)
        {
            try
            {
                EnsureConnectionOpen();

                return await _connection.QueryAsync<TS>(sql, param, _transaction);
            }
            finally
            {
                EnsureConnectionClosed();
            }
        }

#pragma warning disable 693
        public string GetTableName<T>()
        {
            return _connection.GetTableName<T>();
        }
#pragma warning restore 693

        private void EnsureConnectionOpen()
        {
            var retries = 3;
            if (_connection.State == ConnectionState.Open)
            {
                return;
            }
            else
            {
                while (retries >= 0 && _connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                    retries--;

                    // Task.Delay(30);
                }
            }
        }
    }
}

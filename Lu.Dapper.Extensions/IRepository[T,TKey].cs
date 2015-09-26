namespace Lu.Dapper.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public interface IRepository<T, in TKey>
        where T : class
    {
        long Insert(T item);

        Task<int> InsertAsync(T item);

        bool Remove(T item);

        Task<bool> RemoveAsync(T item);

        bool RemoveAll();

        Task<bool> RemoveAllAsync();

        void Remove(Expression<Func<T, bool>> predicate);

        Task RemoveAsync(Expression<Func<T, bool>> predicate);

        bool Update(T item);

        Task<bool> UpdateAsync(T item);

        T Get(TKey id);

        Task<T> GetAsync(TKey id);

        IEnumerable<T> Find(Expression<Func<T, bool>> predicate);

        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        IEnumerable<TS> Query<TS>(string sql, object param);

        Task<IEnumerable<TS>> QueryAsync<TS>(string sql, object param);

#pragma warning disable 693
        string GetTableName<T>();
#pragma warning restore 693
    }
}

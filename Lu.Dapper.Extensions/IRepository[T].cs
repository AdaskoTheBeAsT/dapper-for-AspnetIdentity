namespace Lu.Dapper.Extensions
{
    public interface IRepository<T> : IRepository<T, object>
        where T : class
    {
    }
}

namespace Lu.Dapper.Extensions.DataAnnotations
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        public TableAttribute(string tableName)
        {
            Name = tableName;
        }

        public string Name { get; private set; }
    }
}

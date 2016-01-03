namespace Lu.Dapper.Extensions.DataAnnotations
{
    using System;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class AutoIncrementAttribute : Attribute
    {
        public AutoIncrementAttribute(string name = null)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }
}

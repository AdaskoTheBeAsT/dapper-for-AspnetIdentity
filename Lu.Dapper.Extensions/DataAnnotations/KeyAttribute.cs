namespace Lu.Dapper.Extensions.DataAnnotations
{
    using System;

    // do not want to depend on data annotations that is not in client profile
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class KeyAttribute : Attribute
    {
        public KeyAttribute(string name = null)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }
}

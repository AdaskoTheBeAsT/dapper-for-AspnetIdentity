namespace Lu.Dapper.Extensions.DataAnnotations
{
    using System;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
    public class ComputedAttribute : Attribute
    {
        public ComputedAttribute(string name = null)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }
}

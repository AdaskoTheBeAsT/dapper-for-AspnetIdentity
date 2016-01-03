namespace Lu.Dapper.Extensions.DataAnnotations
{
    using System;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
    public class WriteAttribute : Attribute
    {
        public WriteAttribute(bool write, string name = null)
        {
            Write = write;
            Name = name;
        }

        public bool Write { get; private set; }

        public string Name { get; private set; }
    }
}

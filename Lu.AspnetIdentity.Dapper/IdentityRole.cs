namespace Lu.AspnetIdentity.Dapper
{
    using System;

    using Lu.Dapper.Extensions.DataAnnotations;

    [Table("identityroles")]
    public class IdentityRole : IdentityRole<string>
    {
        public IdentityRole()
        {
            Id = Guid.NewGuid().ToString();
        }

        public IdentityRole(string name)
            : base(Guid.NewGuid().ToString(), name)
        {
        }

        public IdentityRole(string id, string name)
            : base(id, name)
        {
        }
    }
}

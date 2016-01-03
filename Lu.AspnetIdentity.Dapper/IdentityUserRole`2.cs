namespace Lu.AspnetIdentity.Dapper
{
    using System;

    using Lu.Dapper.Extensions.DataAnnotations;

    [Table("identityuserroles")]
    public partial class IdentityUserRole<TUserKey, TRoleKey> : IdentityUserRole<string, TUserKey, TRoleKey>
    {
        public IdentityUserRole()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}

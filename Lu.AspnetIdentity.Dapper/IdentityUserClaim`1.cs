namespace Lu.AspnetIdentity.Dapper
{
    using System;

    using Lu.Dapper.Extensions.DataAnnotations;

    [Table("identityuserclaims")]
    public partial class IdentityUserClaim<TUserKey> : IdentityUserClaim<string, TUserKey>
    {
        public IdentityUserClaim()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}

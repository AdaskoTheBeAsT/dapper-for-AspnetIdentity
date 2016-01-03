namespace Lu.AspnetIdentity.Dapper
{
    using System;
    using Lu.Dapper.Extensions.DataAnnotations;

    [Table("identityuserlogins")]
    public partial class IdentityUserLogin<TUserKey> : IdentityUserLogin<string, TUserKey>
    {
        public IdentityUserLogin()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}

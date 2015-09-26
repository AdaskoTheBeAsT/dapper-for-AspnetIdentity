namespace Lu.AspnetIdentity.Dapper
{
    using System;

    public partial class IdentityUserRole<TKey, TRoleKey>
    {
        public IdentityUserRole()
        {
            Id = Guid.NewGuid().ToString();
        }

        public string Id { get; set; }

        public TKey UserId { get; set; }

        public TRoleKey RoleId { get; set; }
    }
}

namespace Lu.AspnetIdentity.Dapper
{
    public class IdentityUserRole<TKey, TUserKey, TRoleKey>
    {
        public TKey Id { get; set; }

        public TUserKey UserId { get; set; }

        public TRoleKey RoleId { get; set; }
    }
}

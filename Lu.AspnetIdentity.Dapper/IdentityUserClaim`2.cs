namespace Lu.AspnetIdentity.Dapper
{
    public class IdentityUserClaim<TKey, TUserKey>
    {
        public TKey Id { get; set; }

        public TUserKey UserId { get; set; }

        public string ClaimValue { get; set; }

        public string ClaimType { get; set; }
    }
}

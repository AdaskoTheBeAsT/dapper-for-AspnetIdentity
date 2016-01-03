namespace Lu.AspnetIdentity.Dapper
{
    public class IdentityUserLogin<TKey, TUserKey>
    {
        public TKey Id { get; set; }

        public TUserKey UserId { get; set; }

        public string LoginProvider { get; set; }

        public string ProviderKey { get; set; }
    }
}

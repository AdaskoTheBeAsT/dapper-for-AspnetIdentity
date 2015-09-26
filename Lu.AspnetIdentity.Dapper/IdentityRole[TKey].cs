namespace Lu.AspnetIdentity.Dapper
{
    using Microsoft.AspNet.Identity;

    public class IdentityRole<TKey> : IRole<TKey>
    {
        /// <summary>
        /// Default constructor for Role 
        /// </summary>
        public IdentityRole()
        {
        }

        /// <summary>
        /// Constructor that takes names as argument 
        /// </summary>
        /// <param name="name"></param>
        public IdentityRole(string name)
            : this()
        {
            Name = name;
        }

        public IdentityRole(TKey id, string name) : this(name)
        {
            Id = id;
        }

        /// <summary>
        /// Role ID
        /// </summary>
        public TKey Id { get; set; }

        /// <summary>
        /// Role name
        /// </summary>
        public string Name { get; set; }
    }
}

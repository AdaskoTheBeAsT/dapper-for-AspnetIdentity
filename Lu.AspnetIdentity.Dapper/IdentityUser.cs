﻿namespace Lu.AspnetIdentity.Dapper
{
    using System;

    public class IdentityUser : IdentityUser<string>
    {
        public IdentityUser()
        {
            Id = Guid.NewGuid().ToString();
        }

        public IdentityUser(string name)
            : base(name)
        {
            Id = Guid.NewGuid().ToString();
        }

        public IdentityUser(string id, string name)
            : base(id, name)
        {
        }
    }
}

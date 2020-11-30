using System;
using Microsoft.AspNetCore.Identity;

namespace APIWithIdentity.DomainModel.Models.Auth
{
    public class User : IdentityUser<Guid>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
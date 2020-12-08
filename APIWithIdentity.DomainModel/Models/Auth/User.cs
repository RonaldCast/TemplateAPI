using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

namespace APIWithIdentity.DomainModel.Models.Auth
{
    public class User : IdentityUser<Guid>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        
        [JsonIgnore]
        public List<RefreshToken> RefreshTokens { get; set; }
    }
}
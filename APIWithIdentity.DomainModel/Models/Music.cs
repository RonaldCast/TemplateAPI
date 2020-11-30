using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using APIWithIdentity.DomainModel.Models.Contracts;

namespace APIWithIdentity.DomainModel.Models
{
    public class Music : Entity
    {
        [Required]
        public string Name { get; set; }
        public int ArtistId { get; set; }
        public Artist Artist { get; set; }
       
    }
}
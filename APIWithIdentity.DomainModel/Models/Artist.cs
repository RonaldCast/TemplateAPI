
using System.Collections.Generic;
using APIWithIdentity.DomainModel.Models.Contracts;

namespace APIWithIdentity.DomainModel.Models
{
    public class Artist : Entity
    {
        public string Name { get; set; }
        public ICollection<Music> Musics { get; set; }
    }
}
using  System;
using System.ComponentModel.DataAnnotations;

namespace APIWithIdentity.DomainModel.Models.Contracts
{
    public class Entity 
    {
        [Key]
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
    }
}
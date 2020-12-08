using System;
using System.Linq;
using System.Threading.Tasks;
using APIWithIdentity.DomainModel.Models;
using APIWithIdentity.DomainModel.Models.Auth;
using APIWithIdentity.DomainModel.Models.Contracts;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace APIWithIdentity.DomainModel
{
    public class AppDbContext : IdentityDbContext<User, Role, Guid>
    {
        
        public  DbSet<Artist> Artists { get; set; }
        public  DbSet<Music> Musics { get; set; }
        
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        
        
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
        
        public override int SaveChanges()
        {
            AddAuitInfo();
            return base.SaveChanges();
        }

        public async Task<int> SaveChangesAsync()
        {
            AddAuitInfo();
            return await base.SaveChangesAsync();
        }

        
        private void AddAuitInfo()
        {
            var entries = ChangeTracker.Entries().Where(x => x.Entity is Entity && (x.State == EntityState.Added || x.State == EntityState.Modified));
            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    ((Entity)entry.Entity).Created = DateTime.UtcNow;
                }
                ((Entity)entry.Entity).Modified = DateTime.UtcNow;
            }
        }
        
    }
    
    
}
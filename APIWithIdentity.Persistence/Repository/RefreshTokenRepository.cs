using System;
using System.Linq;
using System.Threading.Tasks;
using APIWithIdentity.DomainModel;
using APIWithIdentity.DomainModel.Models.Auth;
using APIWithIdentity.DTOs;
using APIWithIdentity.DTOs.DTOsAuth;
using APIWithIdentity.Persistence.Repository.Contracts;
using Microsoft.EntityFrameworkCore;

namespace APIWithIdentity.Persistence.Repository
{
    public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<User> SaveRefreshTokenAsync(User user, RefreshToken refreshToken)
        {
            var userUpdate = await Context.Users.Where(x => x.Id == user.Id)
                .FirstOrDefaultAsync();
            
            userUpdate.RefreshTokens.Add(refreshToken);

            return userUpdate;
        }

        public async Task<User> GetUserByRefreshTokenAsync(string token)
        {
           var user = await  Context.Users.SingleOrDefaultAsync(u =>
                u.RefreshTokens.Any(t => t.Token == token));

           return user;
        }

       public async Task<User>  UpdateRefreshTokenAsync(string token, 
           RefreshToken newRefreshToken, string ipAddress)
        {
            var user = await  Context.Users.SingleOrDefaultAsync(u =>
                u.RefreshTokens.Any(t => t.Token == token));
            
            if (user == null) return null;
            
            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);
            
            if (!refreshToken.IsActive) return null;
            
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            refreshToken.ReplacedByToken = newRefreshToken.Token;
            user.RefreshTokens.Add(newRefreshToken);
            Context.Update(user);

            return user;
        }

       public async Task<bool> RevokeTokenAsync(string token, string ipAddress)
       {
           var user = await Context.Users.SingleOrDefaultAsync(u => 
               u.RefreshTokens.Any(t => t.Token == token));
           
           if (user == null) return false;

           var refreshToken = user.RefreshTokens.Single(x => x.Token == token);
           
           if (!refreshToken.IsActive) return false;
           
           refreshToken.Revoked = DateTime.UtcNow;
           refreshToken.RevokedByIp = ipAddress;

           Context.Update(user);

           return true;

       }
        

    }
}
using System.Net.Http;
using System.Threading.Tasks;
using APIWithIdentity.DomainModel.Models.Auth;
using APIWithIdentity.DTOs;
using APIWithIdentity.DTOs.DTOsAuth;

namespace APIWithIdentity.Persistence.Repository.Contracts
{
    public interface IRefreshTokenRepository
    {
        Task<User> SaveRefreshTokenAsync(User user, RefreshToken refreshToken);

        Task<User> GetUserByRefreshTokenAsync(string token);

        Task<User> UpdateRefreshTokenAsync(string token, RefreshToken newRefreshToken, string ipAddress);

        Task<bool> RevokeTokenAsync(string token, string ipAddress); 
        
        

    }
}
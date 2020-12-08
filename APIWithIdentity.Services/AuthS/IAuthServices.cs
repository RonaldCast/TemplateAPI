using System.Threading.Tasks;
using APIWithIdentity.DomainModel.Models.Auth;
using APIWithIdentity.DTOs;

namespace APIWithIdentity.Services
{
    public interface IAuthServices
    {

        Task<User> SaveRefreshTokenAsync(User user, RefreshToken refreshToken);
        
        Task<User> GetUserByRefreshTokenAsync(string token);

        Task<User> UpdateRefreshTokenAsync(string token, RefreshToken newRefreshToken, string ipAddress);

        Task<ResponseMessage<bool>> RevokeTokenAsync(string token, string ipAddress);
    }
}
using System.Threading.Tasks;
using APIWithIdentity.DomainModel.Models.Auth;
using APIWithIdentity.DTOs;
using APIWithIdentity.DTOs.DTOsAuth;
using APIWithIdentity.Persistence;

namespace APIWithIdentity.Services
{
    public class AuthServices : IAuthServices
    {
        
        private readonly IUnitOfWork _uow;
  

        public AuthServices(IUnitOfWork uow)
        {
            _uow = uow;
        }
        

        public async Task<User> SaveRefreshTokenAsync(User user, RefreshToken refreshToken)
        {
           var userResp = await _uow.RefreshTokens.SaveRefreshTokenAsync(user, refreshToken);

           var  save = await _uow.SaveAsync();

           return save != 1 ? null : userResp;
        }
        
        public async Task<User> GetUserByRefreshTokenAsync(string token)
        {
            return await _uow.RefreshTokens.GetUserByRefreshTokenAsync(token);
        }

        public async Task<User> UpdateRefreshTokenAsync(string token, RefreshToken newRefreshToken, string ipAddress)
        {
            var user = await _uow.RefreshTokens.UpdateRefreshTokenAsync(token, newRefreshToken, ipAddress);

            await _uow.SaveAsync();

            return user;
        }

        public async Task<ResponseMessage<bool>> RevokeTokenAsync(string token, string ipAddress)
        {
            var saveRevoke = await _uow.RefreshTokens.RevokeTokenAsync(token, ipAddress);
            
            if (!saveRevoke)
            {
                return new  ResponseMessage<bool> 
                    {
                        Message =  "Error al momento de revocar token", 
                        Response = false
                    }
                ;
            }

            await _uow.SaveAsync();
            
            return new ResponseMessage<bool>
            {
                Message = "Err",
                Response = true
            };

        }

    }
}
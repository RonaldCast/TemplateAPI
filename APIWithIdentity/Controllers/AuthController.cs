using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using APIWithIdentity.DomainModel.Models.Auth;
using APIWithIdentity.DTOs;
using APIWithIdentity.DTOs.DTOsAuth;
using APIWithIdentity.Services;
using APIWithIdentity.Settings;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace APIWithIdentity.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly RoleManager<Role> _roleManager;
        private readonly JwtSettings _jwtSettings;
        private readonly IAuthServices _authServices;

        public AuthController(
            IMapper mapper,
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IOptionsSnapshot<JwtSettings> jwtSettings,
            IAuthServices authServices)
        {
            _mapper = mapper;
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtSettings = jwtSettings.Value;
            _authServices = authServices;
        }
        
        //[Authorize(Roles = "Admin")]
        [HttpPost("SignUp")]
        public async Task<ActionResult<ResponseMessage<string>>> SignUp(UserSignUp userSignUp)
        {
            var user = _mapper.Map<UserSignUp, User>(userSignUp);

            var userCreateResult = await _userManager.CreateAsync(user, userSignUp.Password);

            if (userCreateResult.Succeeded)
            {
                return Ok(new ResponseMessage<string> { Message = "User was created"});

            }
            return BadRequest(new ResponseMessage<string> {Message = "Error creating user"});
        }
        
        [HttpPost("SignIn")]
        public async Task<ActionResult<ResponseMessage<ResponseLogin>>> SignIn(UserLogin userLoginResource)
        {
            ResponseMessage<ResponseLogin> resp = new ResponseMessage<ResponseLogin>();
            
            var user = _userManager.Users.SingleOrDefault(u => u.UserName == userLoginResource.Email);
            if (user is null)
            {
                return NotFound(new ResponseMessage<ResponseLogin>{Message = "Email o contraseña son incorrectos"});
            }

            var userSigninResult = await _userManager.CheckPasswordAsync(user, userLoginResource.Password);

            if (!userSigninResult)
            {
                resp.Message = "Email o contraseña son incorrectos";
                
                return Unauthorized(resp);
            }

          
            var roles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwt(user, roles);
            var refreshToken = GenerateRefreshToken(IpAddress());

            await _authServices.SaveRefreshTokenAsync(user, refreshToken);

            var login = new ResponseLogin()
            {
                Token = token,
                RefreshToken = refreshToken.Token,
                User = _mapper.Map<User, UserResponse>(user)

            };
            resp.Message = "Ha iniciado sesion correctamente";
            resp.Response = login;
            
            return Ok(resp);
        }
        
        [Authorize(Roles = "Admin")]
        [HttpPost("role")]
        public async Task<ActionResult<ResponseMessage<string>>> CreateRole([FromBody]  CreateRole model)
        {
            var roleExist = await _roleManager.RoleExistsAsync(model.RoleName);

            if (roleExist) return BadRequest(new ResponseMessage<string> { Message = "Role exist"});
            var newRole = new Role
            {
                Name = model.RoleName
            };

            var result = await _roleManager.CreateAsync(newRole);

            if (result.Succeeded)
            {
                return Ok(new ResponseMessage<string> { Message = "Role was created"});
            }
            return BadRequest(new ResponseMessage<string> { Message = "Error creating role"});
        }
        
        [Authorize(Roles = "Admin")]
        [HttpPost("roleUser")]
        public async Task<ActionResult<ResponseMessage<string>>> AssignRoleToUser([FromBody] AssignRole model)
        {
            
            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
            {
                return NotFound(new ResponseMessage<string> { Message = "User not found"});
            }
            
            var currentRoles = await _userManager.GetRolesAsync(user);

            var rolesNotExist = model.Roles.Except(_roleManager.Roles.Select(x => x.Name)).ToArray();
            
            if(rolesNotExist.Any())
            {
                return BadRequest(new ResponseMessage<string> { Message = "Roles no exists"});
            }
            
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles.ToArray());


            if (!removeResult.Succeeded)
            {
                ModelState.AddModelError("", "Failed to remove user roles");
                return BadRequest(new ResponseMessage<string> { Message = "Error add roles"});
            }
            
            var result = await _userManager.AddToRolesAsync(user, model.Roles);

            if (result.Succeeded)
            {
                return Ok(new ResponseMessage<string> { Message = "Assign role"});
            }
            
            return BadRequest(new ResponseMessage<string> { Message = "Error add roles"});
           
        }

        
        [HttpPost("refresh-token")]
        public async Task<ActionResult<ResponseMessage<ResponseLogin>>>
           RefreshToken([FromBody] TokenRequest refreshToken)
       {
           var user = await _authServices
               .GetUserByRefreshTokenAsync(refreshToken.Token);

           if (user == null)
               return Unauthorized();

           var ip = IpAddress();
           var newRefreshToken = GenerateRefreshToken(ip);

           var updateRefresh = await _authServices
               .UpdateRefreshTokenAsync(refreshToken.Token, newRefreshToken, ip);

           var roles = await _userManager.GetRolesAsync(updateRefresh);
           var resp = new ResponseMessage<ResponseLogin>()
           {
                Response = new ResponseLogin()
                {
                    Token = GenerateJwt(updateRefresh, roles),
                    RefreshToken =  newRefreshToken.Token
                }
           };

           return Ok(resp);

       }
        
        [HttpPost("revoke-token")]
        public async Task<ActionResult<ResponseMessage<bool>>> RevokeToken([FromBody] TokenRequest model)
        {
            var resp = await _authServices.RevokeTokenAsync(model.Token, IpAddress());

            if (!resp.Response)
                return BadRequest(resp);

            return Ok(resp);
        }

        /*private void SetTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }*/

        [Authorize]
        [HttpPost("revoke-token")]
        private string GenerateJwt(User user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            if (roles.Count > 0)
            {
                claims.AddRange(roles
                    .Select(rol => new Claim(ClaimTypes.Role, rol)));
            }

            var roleClaims = roles.Select(r => new Claim(ClaimTypes.Role, r));
            claims.AddRange(roleClaims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_jwtSettings.ExpirationInDays));

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Issuer,
                claims,
                expires : expires,
                signingCredentials : creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        
        private string IpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }
        
        private RefreshToken GenerateRefreshToken(string ipAddress)
        {
            using(var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                var randomBytes = new byte[64];
                rngCryptoServiceProvider.GetBytes(randomBytes);
                return new RefreshToken
                {
                    Token = Convert.ToBase64String(randomBytes),
                    Expires = DateTime.UtcNow.AddDays(365),
                    Created = DateTime.UtcNow,
                    CreatedByIp = ipAddress
                };
            }
        }
        
        
    }


}
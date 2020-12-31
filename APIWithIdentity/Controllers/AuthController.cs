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
using APIWithIdentity.Validators;
using AutoMapper;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        
        [Authorize(Roles = "Admin")]
        [HttpPost("SignUp")]
        public async Task<IActionResult> SignUp(UserSignUp userSignUp)
        {
            var user = _mapper.Map<UserSignUp, User>(userSignUp);

            var userCreateResult = await _userManager.CreateAsync(user, userSignUp.Password);

            if (userCreateResult.Succeeded)
            {
                return Created(string.Empty, string.Empty);
            }

            return Problem(userCreateResult.Errors.First().Description, null, 500);
        }
        
        [HttpPost("SignIn")]
        public async Task<ActionResult<ResponseMessage<ResponseLogin>>> SignIn(UserLogin userLoginResource)
        {
            ResponseMessage<ResponseLogin> resp = new ResponseMessage<ResponseLogin>();
            
            var user = _userManager.Users.SingleOrDefault(u => u.UserName == userLoginResource.Email);
            if (user is null)
            {
                return NotFound("User not found");
            }

            var userSigninResult = await _userManager.CheckPasswordAsync(user, userLoginResource.Password);

            if (!userSigninResult)
            {
                resp.Message = "Email o contraseña son incorrectos";
                
                return BadRequest(resp);
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
        public async Task<IActionResult> CreateRole([FromBody]  CreateRole model)
        {
            var validator = new RoleValidator();
            var validResult = await validator.ValidateAsync(model);

            if (!validResult.IsValid)
            {
                return BadRequest(validResult.Errors);
            }
            
            var roleExist = await _roleManager.RoleExistsAsync(model.RoleName);

            if (roleExist) return BadRequest();
            var newRole = new Role
            {
                Name = model.RoleName
            };

            var result = await _roleManager.CreateAsync(newRole);

            if (result.Succeeded)
            {
                return Ok(new {message = "se a creado correctamente"});
            }
            return BadRequest();
        }
        
        [Authorize(Roles = "Admin")]
        [HttpPost("roleUser")]
        public async Task<ActionResult> AssignRoleToUser([FromBody] AssignRole model)
        {
            var validator = new AssignRoleValidator();
            var validResult = await validator.ValidateAsync(model);

            if (!validResult.IsValid)
                return BadRequest(validResult.Errors);
            
            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
            {
                return NotFound();
            }
            
            var currentRoles = await _userManager.GetRolesAsync(user);

            var rolesNotExist = model.Roles.Except(_roleManager.Roles.Select(x => x.Name)).ToArray();
            
            if(rolesNotExist.Any())
            {
                return this.BadRequest();
            }
            
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles.ToArray());


            if (!removeResult.Succeeded)
            {
                ModelState.AddModelError("", "Failed to remove user roles");
                return BadRequest();
            }
            
            var result = await _userManager.AddToRolesAsync(user, model.Roles);

            if (result.Succeeded)
            {
                return Ok();
            }
            
            return BadRequest();
           
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

        private void SetTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }

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
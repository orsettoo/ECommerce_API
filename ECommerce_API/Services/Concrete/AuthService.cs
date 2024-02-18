using ECommerce_API.DataContext;
using ECommerce_API.Services.Abstract;
using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace ECommerce_API.Services.Concrete
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _contextAccessor;

        public AuthService(ApplicationDbContext context, IConfiguration config, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _config = config;
            _contextAccessor = contextAccessor;
        }

        public async Task<ServiceResponse<bool>> ChangePassword( string oldPassword, string newPassword, string confirmPassword)
        {
            var user = GetUserId();
            var result = await _context.Users.FindAsync(user);
            if (!VerifyPasswordHas(oldPassword, result.PasswordHash, result.PasswordSalt))
            {
                return new ServiceResponse<bool> { Success = false,Messagge="your password is not true" };
            }
            CreatePasswordHash(newPassword,out byte[] passwordHash,out byte[] passwordSalt); 
            result.PasswordHash = passwordHash;
            result.PasswordSalt = passwordSalt;
            await _context.SaveChangesAsync();
            return new ServiceResponse<bool>
            {
                Success = true,
                Messagge = "Your process is success",
            };
        }

        public async Task<ServiceResponse<bool>> DeleteAccount(string password)
        {
            var user = GetUserId();
            var response = await _context.Users.FirstOrDefaultAsync(x => x.Id == user);
            var returner = new ServiceResponse<bool>();
            if(response == null)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                };
            }
            if (!VerifyPasswordHas(password, response.PasswordHash, response.PasswordSalt))
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Messagge = "Your password is not true",
                };
            
              
            }
            _context.Users.Remove(response);
            await _context.SaveChangesAsync();

            return new ServiceResponse<bool>
            {
                Success = true,
                Messagge = "Your account remove success",
            };
        }

        

        public string GetUserEmail()
        {
            return _contextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);
        }

        public int GetUserId()
        {
            return int.Parse(_contextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        public async Task<ServiceResponse<string>> Login(string email, string password)
        {
            var response = new ServiceResponse<string>();
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email.ToLower().Equals(email.ToLower()));
            if (user == null)
            {
                return new ServiceResponse<string>
                {
                    Messagge = "User not found",
                    Success = false,
                    Data =email,
                };
            }
            else if (!VerifyPasswordHas(password, user.PasswordHash, user.PasswordSalt))
            {
                response.Success = false;
                response.Messagge = "Your password is not match";
            }
            else
            {
                response.Data = CreateToken(user);
                response.Messagge = "User login is successfully";
            }
            return response;
        }
        
        public async Task<ServiceResponse<int>> Register(User user, string password)
        {
            if(await UserExist(user.Email))
            {
                return new ServiceResponse<int>
                {
                    Success = false,
                    Messagge = "Already user exist",
                };
            }
            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.CreatedDate = DateTime.UtcNow;
            _context.Users.Add(user);
            _context.SaveChangesAsync();
            return new ServiceResponse<int>
            {
                Data = user.Id,
                Messagge = "User creation successfully",
                Success = true,
            };

        }

       

        public async Task<ServiceResponse<bool>> RoleForAdmin(string email)
        {
            var user = GetUserId();
            var result = await _context.Users.FirstOrDefaultAsync(x => x.Id == user);
            var forRole = await _context.Users.FirstOrDefaultAsync(x=>x.Email.ToLower().Equals(email.ToLower()));
            if (result.Role == "admin")
            {
                forRole.Role = "admin";
                _context.Users.Update(forRole);
                await _context.SaveChangesAsync();

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Messagge = "Users role changed with admin"
                };


            }
            return new ServiceResponse<bool>
            {
                Success = false,
            };
        }

        public async Task<bool> UserExist(string email)
        {
            if(await _context.Users.AnyAsync(x => x.Email.ToLower().Equals(email.ToLower())))
            {
                return true;
            }
            return false;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(ClaimTypes.Name,user.Email),
                new Claim(ClaimTypes.Role,user.Role),
            };
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:SecretKey").Value));
            var creds = new SigningCredentials(key,SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims : claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }

        private bool VerifyPasswordHas(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
    }
}

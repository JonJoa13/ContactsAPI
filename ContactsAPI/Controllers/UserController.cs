using ContactsAPI.Data;
using ContactsAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ContactsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly ContactsApiDbContext _dbContext;

        public UserController(ContactsApiDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        #region Public methods
        /// <summary>
        /// Get existing user and generate token
        /// </summary>
        /// <param name="userLogin"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Login")]
        public IActionResult Login(UserLogin userLogin)
        {
            try
            {
                var user = _dbContext.Users.FirstOrDefault(o => o.Username == userLogin.Username && o.Password == userLogin.Password);
                if (user == null)
                    return BadRequest("Invalid user credentials");

                #region Generate token
                var claims = new[]
                {
                new Claim(ClaimTypes.NameIdentifier, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

                var builder = WebApplication.CreateBuilder();
                var token = new JwtSecurityToken
                (
                    issuer: builder.Configuration["Jwt:Issuer"],
                    audience: builder.Configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddDays(60),
                    notBefore: DateTime.UtcNow,
                    signingCredentials: new SigningCredentials(
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
                        SecurityAlgorithms.HmacSha256)
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                #endregion

                return Ok(tokenString);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Create new user
        /// </summary>
        /// <param name="addUserRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult AddUser(AddUserRequest addUserRequest)
        {
            try
            {
                var existingUser = _dbContext.Users.FirstOrDefault(o => o.Username == addUserRequest.Username && o.Password == addUserRequest.Password);

                // If the user try to add an existing user with same username and password, we can just return the existing one.
                if (existingUser != null)
                    return Ok(existingUser);

                // If the username is already used, return bad request
                if (_dbContext.Users.Any(u => u.Username == addUserRequest.Username))
                    return BadRequest("Username already used");

                // Check if role is one of roleEnum or return error.
                if (!Enum.IsDefined(typeof(RoleEnum), addUserRequest.Role))
                    return BadRequest("Role is invalid. Select \"Admin\" or \"Standard\"");

                var newUser = new User()
                {
                    Username = addUserRequest.Username,
                    Password = addUserRequest.Password,
                    Role = addUserRequest.Role
                };

                _dbContext.Users.Add(newUser);
                _dbContext.SaveChanges();
                return Ok(newUser);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        #endregion
    }
}

using MailServiceAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MailServiceAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : Controller
    {
        private readonly ILogger<UserController> logger;
        private MailDBContext db;
        private AppSettings settings;
        private IDistributedCache redis;
        public UserController(ILogger<UserController> logger, MailDBContext db, IOptions<AppSettings> settings, IDistributedCache redis) 
        {
            this.logger = logger;
            this.db = db;
            this.settings = settings.Value;
            this.redis = redis;
        }
        
        [AllowAnonymous]
        [HttpGet("")]
        public string Index()
        {
            return "User Endpoint working";
        }

        [Authorize]
        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsersAsync()
        {
            var result = db.Users.Include(x => x.SentMessages).Include(x => x.RecievedMessages).Select(x => new UserDTO()
            {
                Id = x.Id,
                Name = x.Name,
                Email = x.Email,
                Role = x.Role,
                SentMessages = x.SentMessages,
                RecievedMessages = x.RecievedMessages
            });

            if (result is null) return BadRequest();

            return Ok(result);
        }

        [Authorize]
        [HttpGet("users/{id}")]
        public async Task<ActionResult<UserDTO>> GetUserByIdAsync([FromRoute] long id)
        {
            string? cache = await redis.GetStringAsync("User/" + id);

            if(cache is not null && !string.IsNullOrEmpty(cache))
            {
                var cachedUser = JsonConvert.DeserializeObject<UserDTO>(cache);

                Console.WriteLine("Used Cache");

                return Ok(cachedUser);
            }

            var result = await db.Users.Include(x => x.SentMessages).Include(x => x.RecievedMessages).FirstOrDefaultAsync(x => x.Id == id);

            if (result is null) return NotFound();

            UserDTO user = new()
            {
                Id = result.Id,
                Name = result.Name,
                Email = result.Email,
                Password = result.Password,
                Role = result.Role,
                SentMessages = result.SentMessages,
                RecievedMessages = result.RecievedMessages
            };

            await redis.SetStringAsync("User/" + id, JsonConvert.SerializeObject(user));

            Console.WriteLine("Created Cache");

            return Ok(user);
        }

        [AllowAnonymous]
        [HttpPost("signin")]
        public async Task<ActionResult<Auth>> SignInAsync([FromBody] UserAuth userAuth)
        {
            var result = await db.Users.FirstOrDefaultAsync(x => x.Email.ToLower().Equals(userAuth.Email.ToLower()) && x.Password.Equals(userAuth.Password));

            if (result is null) return BadRequest();

            string token = GenerateJwtToken(result);

            return Ok(new Auth() { Token = token });
        }

        private string GenerateJwtToken(User user)
        {
            // generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(settings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        [Authorize]
        //[AllowAnonymous]
        [HttpPost("users")]
        public async Task<ActionResult<UserDTO>> PostUserAsync([FromBody] UserCreateUpdate obj)
        {
            User user = new()
            {
                Email = obj.Email,
                Password = obj.Password,
                Name = obj.Name,
                Role = obj.Role
            };

            var result = await db.Users.AddAsync(user);

            if (result is null) return BadRequest();

            await db.SaveChangesAsync();

            return await GetUserByIdAsync(result.Entity.Id);
        }

        [Authorize]
        [HttpPut("users/{id}")]
        public async Task<ActionResult<UserDTO>> PutUserAsync([FromRoute] long id, [FromBody] UserCreateUpdate obj)
        {
            var result = await db.Users.FirstOrDefaultAsync(x => x.Id == id);

            if (result is null) return NotFound();

            if(!string.IsNullOrEmpty(obj.Name) && !obj.Name.Equals(result.Name))
            {
                result.Name = obj.Name;
            }
            if (!string.IsNullOrEmpty(obj.Email) && !obj.Email.Equals(result.Email))
            {
                result.Email = obj.Email;
            }
            if (!string.IsNullOrEmpty(obj.Password) && !obj.Password.Equals(result.Password))
            {
                result.Password = obj.Password;
            }
            if(obj.Role != result.Role)
            {
                result.Role = obj.Role;
            }

            var updated = db.Users.Update(result);

            if (updated is null) return BadRequest();

            await db.SaveChangesAsync();

            await redis.RemoveAsync("User/" + id);

            return await GetUserByIdAsync(id);
        }

        [Authorize]
        [HttpDelete("users/{id}")]
        public async Task<ActionResult> DeleteUserAsync([FromRoute] long id)
        {
            var result = await db.Users.FirstOrDefaultAsync(x => x.Id == id);

            if (result is null) return NotFound();

            db.Users.Remove(result);

            await db.SaveChangesAsync();

            await redis.RemoveAsync("User/" + id);

            return Ok();
        }
    }
}

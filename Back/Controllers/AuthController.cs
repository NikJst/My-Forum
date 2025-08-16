using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MyForum.Models;
using MyForum.Hubs;
namespace MyForum.Controllers;


    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ForumContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHubContext<ForumHub> _hubContext;
       public AuthController(ForumContext context, IConfiguration configuration, IHubContext<ForumHub> hubContext)
        {
        _context = context;
        _configuration = configuration;
        _hubContext = hubContext;
        }

//РЕГИСТРАТУРА
        public class RegisterRequest
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
    {
        // Проверяем, есть ли уже такой пользователь
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            return BadRequest("Пользователь с таким именем уже существует.");

        // Хэшируем пароль
        var passwordHash = HashPassword(request.Password);

        var user = new User
        {
            Username = request.Username,
            PasswordHash = passwordHash
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
         var token = GenerateJwtToken(user); //  метод генерации JWT
        await UpdateUserCounts();
        return Ok(new { success = true });
    }

    // Простейший хэш пароля 
        // этот метод вызывается из Register(RegisterRequest request)
        private string HashPassword(string password)
    {
        byte[] salt = new byte[128 / 8];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));

        // Сохраняем соль вместе с хэшем, чтобы потом проверить (пример)
        return $"{Convert.ToBase64String(salt)}.{hashed}";
    }

//ВСЕ ЧТО НИЖЕ - ДЛЯ АВТОРИЗАЦИИ
        public class LoginRequest
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

    [HttpPost("login")]// В этом моменте контроллер возвращает токен если регистрация успешна
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user == null)
            return Unauthorized("Неверное имя пользователя или пароль.");

        if (!VerifyPassword(request.Password, user.PasswordHash))
            return Unauthorized("Неверное имя пользователя или пароль.");

        var token = GenerateJwtToken(user);
        await UpdateUserCounts();
        return Ok(new { token });
    }

//ЕСЛИ В LOGIN ВСЕ ОК, ТО ДАЛЬШЕ ==========================>
    private string GenerateJwtToken(User user)
    {

// Проверяем, что ключ настроен
        var jwtKey = _configuration["Jwt:Key"];
Console.WriteLine("JWT Key: " + jwtKey);
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("JWT ключ не настроен в конфигурации");


        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: null,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split('.');
        if (parts.Length != 2) return false;

        var salt = Convert.FromBase64String(parts[0]);

        var hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));

        return parts[1] == hashed;
    }

    // ниже данные достаются из Claims, которые я задал GenerateGwtToken
[Authorize]
[HttpGet("me")] 
public IActionResult GetCurrentUser()
{
    var username = User.Identity?.Name;
    return Ok(new
    {
        message = $"Вы авторизованы как: {username}"
    });
}
//==> отображает количество подключённых пользователей к ветке
    private async Task UpdateUserCounts()
    {
        var threadUsers = await _context.ThreadConnections
            .GroupBy(tc => tc.ThreadId)
            .Select(g => new { ThreadId = g.Key, Count = g.Count() })
            .ToListAsync();

        foreach (var item in threadUsers)
        {
            await _hubContext.Clients.All.SendAsync("UpdateUserCount", item.ThreadId, item.Count);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Warehouse.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Warehouse.Application.DTO;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Warehouse.Infrastructure.Data;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly WarehouseDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UsersController> _logger;
    public UsersController(
        WarehouseDbContext context,
        IPasswordHasher<User> passwordHasher,
        IConfiguration configuration,
        ILogger<UsersController> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
        _logger = logger;
    }

    // GET: api/User/5
    [HttpGet("id/{id}")]
    [EndpointSummary("Returns user by id")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<User>> GetUserById(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        return user;
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult GetMe()
    {
        var token = HttpContext.Request.Headers.Authorization.ToString();
        _logger.LogDebug("JWT Token: {token}", token);

        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var username = User.FindFirstValue(ClaimTypes.Name);
        var role = User.FindFirstValue(ClaimTypes.Role);

        return Ok(new
        {
            Id = id,
            Username = username,
            Role = role
        });
    }

    // POST: api/User/login
    [HttpPost("login")]
    [EndpointSummary("Checks user username and password")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<User>> Login(RegisterUserDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == dto.Username);

        if (user == null)
            return Problem(
                title: "Invalid credentials",
                detail: "Wrong username or password",
                statusCode: StatusCodes.Status401Unauthorized
                );

        var result = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            dto.Password);

        if (result == PasswordVerificationResult.Failed)
            return Problem(
                title: "Invalid credentials",
                detail: "Wrong username or password",
                statusCode: StatusCodes.Status401Unauthorized
                );

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("username", user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new
        {
            token = jwt
        });
    }

    // POST: api/User
    [HttpPost("signup")]
    [EndpointSummary("Inserts user into database")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<User>> PostUser(RegisterUserDto dto)
    {
        var existingUsername = await _context.Users.FirstOrDefaultAsync(x => x.Username == dto.Username);
        var existingEmail = await _context.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);

        if (existingEmail != null)
            return Problem(
                title: "User with this email already exists",
                detail: "User with this email already exists.",
                statusCode: StatusCodes.Status409Conflict);

        if (existingUsername != null)
            return Problem(
                title: "Username already exists",
                detail: "Username already exists.",
                statusCode: StatusCodes.Status409Conflict);

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(
            user,
            dto.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetUserById),
            new { id = user.Id },
            new
            {
                user.Id,
                user.Username,
                user.Role,
                user.Email
            });
    }

    // DELETE: api/User/5
    [HttpDelete("{id}")]
    [EndpointSummary("Deletes user from database")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(int? id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
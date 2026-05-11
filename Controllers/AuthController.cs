using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ElectricityPlanner.Application.DTOs;
using ElectricityPlanner.Application.Options;
using ElectricityPlanner.Domain.Entities;
using ElectricityPlanner.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;


namespace ElectricityPlanner.Controllers;


[ApiController]
[Route("/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtOptions _jwt;
    private readonly IPasswordHasher<AppUsers> _passwordHasher;

    public AuthController(AppDbContext db, IOptions<JwtOptions> jwt, IPasswordHasher<AppUsers> passwordHasher)
    {
        _db = db;
        _jwt = jwt.Value;
        _passwordHasher = passwordHasher;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if(string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password)) return BadRequest("Username and password are required");

        var user = await _db.AppUsers.AsNoTracking().FirstOrDefaultAsync(u => u.Username == request.Username.Trim(), cancellationToken);

        if( user is null) return Unauthorized("Invalid credentials.");

        var verifyPasswordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if(verifyPasswordResult == PasswordVerificationResult.Failed) return Unauthorized("Invalid credentials.");

        var tokenExpires = DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes);
        var token = CreateJwtToken(user, tokenExpires);

     return Ok(new LoginResponse 
     {
        Token = token,
        ExpiresAt = tokenExpires,
        Role = user.Role
     });
    }


    private string CreateJwtToken(AppUsers user, DateTime expires)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new (JwtRegisteredClaimNames.UniqueName, user.Username),
            new (ClaimTypes.NameIdentifier, user.Id.ToString()),
            new (ClaimTypes.Name, user.Username),
            new (ClaimTypes.Role, user.Role),
        };

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace Soteo.AuthServer.Controllers;

public sealed class AuthController : Controller
{
    private readonly string _intercomSecret;
    private readonly string _domain = "localhost";
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SigningCredentials _signingCredentials;

    public AuthController(UserManager<IdentityUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _intercomSecret = configuration["Soteo:IntercomSecret"] ??
            throw new Exception("IntercomSecret is not set.");
        var securityKey = new SymmetricSecurityKey(Convert.FromBase64String(_intercomSecret));
        _signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
    }

    [HttpPost("/token")]
    public async Task<IActionResult> GetPlayerToken(string email, string password)
    {
        if (!ModelState.IsValid) return BadRequest();
        IdentityUser? user = await _userManager.FindByEmailAsync(email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, password)) return Unauthorized();
        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new("player", "true")
        ];
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(60),
            SigningCredentials = _signingCredentials,
            Issuer = _domain,
            Audience = _domain
        };
        string accessToken = new JsonWebTokenHandler().CreateToken(tokenDescriptor);
        return Ok(accessToken);
    }
    
    [HttpPost("/token/service")]
    public async Task<IActionResult> GetServiceToken(string id, string role, string intercomSecret)
    {
        if (!ModelState.IsValid) return BadRequest();
        if (intercomSecret != _intercomSecret) return Unauthorized();
        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, id),
            new(role, "true")
        ];
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(60),
            SigningCredentials = _signingCredentials,
            Issuer = _domain,
            Audience = _domain
        };
        string accessToken = new JsonWebTokenHandler().CreateToken(tokenDescriptor);
        return Ok(accessToken);
    }
}
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace blackbox.utils.baerer
{
    public class JwtSignInHandler
    {
        public const string TokenAudience = "Myself";
        public const string TokenIssuer = "MyProject";
        private readonly SymmetricSecurityKey key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("mysupersecret_secretkey!123"));

        public string BuildJwt(ClaimsPrincipal principal)
        {
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: TokenIssuer,
                audience: TokenAudience,
                claims: principal.Claims,
                expires: DateTime.Now.AddMinutes(20),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
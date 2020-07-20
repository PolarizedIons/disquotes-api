using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using QuotesApi.Models.Users;

namespace QuotesApi.Services
{
    public class JwtService : ISingletonDiService
    {
        public const string Issuer = "Disquotes API";
        public const string Audience = "Disquotes Client";
        public const string AccountIdField = "Account-ID";
        public const string DiscordIdField = "Discord-ID";

        private static readonly TimeSpan AccessTokenValidFor = TimeSpan.FromMinutes(15);

        private readonly JwtSecurityTokenHandler _tokenHandler;
        private byte[] _key;

        public JwtService(IConfiguration config)
        {
            _key = Encoding.UTF8.GetBytes(config["Jwt:Secret"]);
            _tokenHandler = new JwtSecurityTokenHandler();
        }

        public string CreateAccessTokenFor(User user)
        {
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] 
                {
                    new Claim(AccountIdField, user.Id.ToString()),
                    new Claim(DiscordIdField, user.DiscordId.ToString()), 
                }),
                Issuer = Issuer,
                Audience = Audience,
                Expires = DateTime.UtcNow.Add(AccessTokenValidFor),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = _tokenHandler.CreateToken(tokenDescriptor);
            return _tokenHandler.WriteToken(token);
        }
    }
}

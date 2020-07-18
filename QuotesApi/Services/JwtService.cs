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
        private const int ValidForMins = 15;
        public const string Issuer = "Disquotes API";
        public const string Audience = "Disquotes Client";
        public const string AccountIdField = "Account-ID";
        public const string DiscordIdField = "Discord-ID";

        private readonly string _secret;
        private readonly JwtSecurityTokenHandler _tokenHandler;
        
        public JwtService(IConfiguration config)
        {
            _secret = config["Jwt:Secret"];
            _tokenHandler = new JwtSecurityTokenHandler();
        }

        public string CreateTokenFor(User user)
        {
            var key = Encoding.UTF8.GetBytes(_secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] 
                {
                    new Claim(AccountIdField, user.Id.ToString()),
                    new Claim(DiscordIdField, user.DiscordId.ToString()), 
                }),
                Issuer = Issuer,
                Audience = Audience,
                Expires = DateTime.UtcNow.AddMinutes(ValidForMins),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = _tokenHandler.CreateToken(tokenDescriptor);
            return _tokenHandler.WriteToken(token);
        }
    }
}

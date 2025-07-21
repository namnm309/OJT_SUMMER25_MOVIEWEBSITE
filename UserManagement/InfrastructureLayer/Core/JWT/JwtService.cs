using DomainLayer.Enum;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureLayer.Core.JWT
{
    public interface IJwtService
    {
        string GenerateToken(Guid userId, UserRole role, Guid sessionId, string email, string username, bool isActive, int exp);
        Payload? ValidateToken(string token);
    }
    public class JwtService : IJwtService
    {
        private readonly string DEFAULT_SECRET = "ea8cf10696dc45a8b7b5f15758ae3ef238b440cfa1f84b449af315d515de6f95";
        private readonly byte[] _key;
        private readonly JwtSecurityTokenHandler _handler;

        public JwtService()
        {
            var SecretKey = Environment.GetEnvironmentVariable("JWT_SECRET") ?? DEFAULT_SECRET;
            _key = Encoding.ASCII.GetBytes(SecretKey);
            _handler = new JwtSecurityTokenHandler();
        }

        public string GenerateToken(Guid userId, UserRole role, Guid sessionId, string email, string username, bool isActive, int exp)
        {
            var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET") ?? DEFAULT_SECRET);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    // Custom claim để JS dễ dàng đọc userId
                    new Claim("userId", userId.ToString()),
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new("sessionId", sessionId.ToString()),
                    new("isActive", isActive.ToString()),
                    new("email", email),
                    new("username", username),
                    new(ClaimTypes.Email, email),
                    new(ClaimTypes.Name, username),
                    new("role", role.ToString()),
                    new(ClaimTypes.Role, role.ToString())
                }),
                Issuer = userId.ToString(),
                Expires = DateTime.UtcNow.AddSeconds(exp),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = _handler.CreateToken(tokenDescriptor);

            return _handler.WriteToken(token);
        }

        public Payload? ValidateToken(string token)
        {
            _handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(_key),
                ValidateIssuer = false,
                ValidateAudience = false,
                // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var result = (JwtSecurityToken)validatedToken;

            var payload = new Payload()
            {
                UserId = Guid.Parse(result.Issuer),
                Email = result.Claims.First(x => x.Type == "email").Value,
                SessionId = Guid.Parse(result.Claims.First(x => x.Type == "sessionId").Value),
                Role = Enum.Parse<UserRole>(result.Claims.First(x => x.Type == "role").Value),
                IsActive = bool.Parse(result.Claims.First(x => x.Type == "isActive").Value)
            };

            return payload;
        }
    }
}

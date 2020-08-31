using Cosmonaut.Extensions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TweetBook.Contracts.V1;
using TweetBook.Contracts.V1.Request;
using TweetBook.Contracts.V1.Responses;
using TweetBook.Data;
using TweetBook.Domain;
using TweetBook.Options;

namespace TweetBook.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly JwtSettings jwtSettings;
        private readonly TokenValidationParameters tokenValidationParameters;
        private readonly DataContext dataContext;

        public IdentityService(UserManager<IdentityUser> userManager, JwtSettings jwtSettings, 
            TokenValidationParameters tokenValidationParameters, DataContext dataContext)
        {
            this.userManager = userManager;
            this.jwtSettings = jwtSettings;
            this.tokenValidationParameters = tokenValidationParameters;
            this.dataContext = dataContext;
        }

        public async Task<AuthenticationResult> RegisterAsync(string Email, string Password)
        {
            var existingUser = await userManager.FindByEmailAsync(Email);
            if (existingUser != null)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    Errors = new[] { "User with this Email is already exist!" }
                };
            }
            IdentityUser newUser = new IdentityUser
            {
                Email = Email,
                UserName = Email,
            };
            var result = await userManager.CreateAsync(newUser, Password);
            if (!result.Succeeded)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    Errors = new[] { "Failed to create a new account!" }
                };
            }

            return await GenerateAuthenticationResultForUserAsync(newUser);
        }

        public async Task<AuthenticationResult> LoginAsync(string Email, string Password)
        {
            var user = await userManager.FindByEmailAsync(Email);
            if(user == null)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    Errors = new[] { "User doesn't exist." }
                };
            }

            var userHasValidPassword = await userManager.CheckPasswordAsync(user, Password);
            if (!userHasValidPassword)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    Errors = new[] { "User/password doesn't match." }
                };
            }

            return await GenerateAuthenticationResultForUserAsync(user);

        }


        private async Task<AuthenticationResult> GenerateAuthenticationResultForUserAsync(IdentityUser user)
        {
            var tokenHanlder = new JwtSecurityTokenHandler();
            //khóa bí mật cho phần signature
            var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                //Subject: Get hoặc là Set những claims(nằm ở phần payload) của token đã phát hành
                //Token: Get hoặc là Set security token
                //SigningCredentials: Get hoặc là Set những thông tin để kí hiệu token (signature)
                Subject = new System.Security.Claims.ClaimsIdentity(new[]
                {
                    // sub: Subject
                    // Jti: JWT ID
                     new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                     new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                     new Claim(JwtRegisteredClaimNames.Email, user.Email),
                     new Claim("id",user.Id)
                }),
                Expires = DateTime.UtcNow.Add(jwtSettings.TokenLifeTime),
                //Tạo ra signature bằng cách Mã hóa khóa bí mật bằng thuật toán HmacSha256Singnature (signature, header)
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHanlder.CreateToken(tokenDescriptor);
            var refreshToken = new RefreshToken()
            {
                JwtId = token.Id,
                UserId = user.Id,
                CreationDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(6),
            };

            await dataContext.RefreshTokens.AddAsync(refreshToken);
            await dataContext.SaveChangesAsync();

            return new AuthenticationResult
            {
                Success = true,
                Token = tokenHanlder.WriteToken(token),
                RefreshToken = refreshToken
            };
        }

        //REFRESH TOKEN

        //The principal object represents the security context under which code is running.
        //Identity: this property returns the identity object that is associated with this principal.
        //Applications that implement role-based security grant rights based on the role associated with a principal object. Similar to identity objects
        private ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                //kiểm tra token có valid không, nếu valid thì trả về claimspricipal ở payload, không thì ném exceptions
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
                if (!IsJwtWithValidSecurityAlgorithm(validatedToken))
                {
                    return null;
                }

                return principal;
            }
            catch 
            {
                return null;
            }
        }

        //Kiểm tra xem token có phải đc dùng cơ chế Jwt để tạo ra và algorithm dùng để mã hóa có phải HmacSha256 không?
        //Vì ở phần installer ta chỉ specify key chứ không specify type of key
        private bool IsJwtWithValidSecurityAlgorithm(SecurityToken validatedToken)
        {
            return (validatedToken is JwtSecurityToken jwtSecurityToken) &&
                jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
        }

        public async Task<AuthenticationResult> RefreshTokenAsync(string Token, string RefreshToken)
        {
            var validatedToken = GetPrincipalFromToken(Token);
            if(validatedToken == null)
            {
                return new AuthenticationResult
                {
                    Errors = new[] { "Invalid Token" }
                };
            }
            var expiryDateUnix = long.Parse(validatedToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
            var expiryDateTimeUTC = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddSeconds(expiryDateUnix);

            if(expiryDateTimeUTC > DateTime.UtcNow)
            {
                return new AuthenticationResult { Errors = new[] { "This token hasn't expired yet" } };
            }
            var jti = validatedToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
            var storedRefreshToken = await dataContext.RefreshTokens.SingleOrDefaultAsync(x => x.Token == RefreshToken);

            if(storedRefreshToken == null)
            {
                return new AuthenticationResult { Errors = new[] { "This token doesn't exist." } };
            }

            if(DateTime.UtcNow > storedRefreshToken.ExpiryDate)
            {
                return new AuthenticationResult { Errors = new[] { "This token has expired." } }; 
            }

            if (!storedRefreshToken.Invalided)
            {
                return new AuthenticationResult { Errors = new[] { "This token has been invalidated." } };
            }

            if (storedRefreshToken.Used)
            {
                return new AuthenticationResult { Errors = new[] { "This token has been used." } };
            }

            if(storedRefreshToken.JwtId != jti)
            {
                return new AuthenticationResult { Errors = new[] { "This token doesn't match this JWT." } };
            }

            storedRefreshToken.Used = true;
            dataContext.RefreshTokens.Update(storedRefreshToken);
            await dataContext.SaveChangesAsync();

            var user = await userManager.FindByNameAsync(validatedToken.Claims.Single(x => x.Type == "id").Value);
            return await GenerateAuthenticationResultForUserAsync(user);
        }



    }
}

﻿using Cosmonaut.Extensions;
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
        private readonly IFacebookAuthService facebookAuthService;

        public IdentityService(UserManager<IdentityUser> userManager, JwtSettings jwtSettings,
            TokenValidationParameters tokenValidationParameters, DataContext dataContext, IFacebookAuthService facebookAuthService)
        {
            this.userManager = userManager;
            this.jwtSettings = jwtSettings;
            this.tokenValidationParameters = tokenValidationParameters;
            this.dataContext = dataContext;
            this.facebookAuthService = facebookAuthService;
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

            var newUserId = Guid.NewGuid();
            IdentityUser newUser = new IdentityUser
            {
                Id = newUserId.ToString(),
                Email = Email,
                UserName = Email,
            };

            var result = await userManager.CreateAsync(newUser, Password);

            //Add Claim
            await userManager.AddClaimAsync(newUser, new Claim("tags.view", "true"));

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

            var claims = new List<Claim>
            {
                // sub: Subject
                // Jti: JWT ID
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim("id",user.Id)
            };

            var userClaims = await userManager.GetClaimsAsync(user);
            var userRoles = await userManager.GetRolesAsync(user);
            foreach(var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            //thêm claims vào token để phân quyèn
            claims.AddRange(userClaims);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                //Subject: Get hoặc là Set những claims(nằm ở phần payload) của token đã phát hành
                //Token: Get hoặc là Set security token
                //SigningCredentials: Get hoặc là Set những thông tin để kí hiệu token (signature)
                Subject = new System.Security.Claims.ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(jwtSettings.TokenLifeTime),
                //Tạo ra signature bằng cách Mã hóa khóa bí mật bằng thuật toán HmacSha256Singnature (signature, header)
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHanlder.CreateToken(tokenDescriptor);
            var refreshToken = new RefreshToken()
            {
                Token = token.Id,
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
        //Lấy nội dung trong phần payload
        private ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                //kiểm tra token có valid không, nếu valid thì trả về claimspricipal ở payload, không thì ném exceptions
                // trả về phần identity chứa trong token đó.
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
                //chưa hết hạn
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
                //hết hạn
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

            //nếu refresh token được lưu trữ cho token đó chưa được sử dụng, chưa hết hạn, và invalid thì
            storedRefreshToken.Used = true;
            dataContext.RefreshTokens.Update(storedRefreshToken);
            await dataContext.SaveChangesAsync();

            //Sau đó, chung ta có thể refresh lại token của mình bằng cách gọi lại hàm tạo token
            var user = await userManager.FindByNameAsync(validatedToken.Claims.Single(x => x.Type == "id").Value);
            return await GenerateAuthenticationResultForUserAsync(user);
        }

        public async Task<AuthenticationResult> LoginWithFacebookAsync(string accessToken)
        {
            var validatedTokenResult = await facebookAuthService.ValidateAccessTokenAsync(accessToken);
            if(!validatedTokenResult.Data.IsValid)
            {
                return new AuthenticationResult
                {
                    Errors = new[] { "Invalid Facebook token" }
                };
            }

            var userInfo = await facebookAuthService.GetUserInfoAsync(accessToken);
            var user = await userManager.FindByEmailAsync(userInfo.Email);

            if(user == null)
            {
                var identityUser = new IdentityUser
                {
                    Email = userInfo.Email,
                    Id = Guid.NewGuid().ToString(),
                    UserName = userInfo.Email
                };

                var createdUser = await userManager.CreateAsync(identityUser);

                if(!createdUser.Succeeded)
                {
                    return new AuthenticationResult
                    {
                        Errors = new string[] { "Something went wrong!" }
                    };
                }

            }
            return await GenerateAuthenticationResultForUserAsync(user);
        }
    }
}

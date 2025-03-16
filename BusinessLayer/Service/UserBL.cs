using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using ModelLayer.Model;
using RepositoryLayer.Interface;
using BusinessLayer.Interface;
using Microsoft.Extensions.Configuration;
using RepositoryLayer.Entity;
using RepositoryLayer.Service;
using StackExchange.Redis;

namespace BusinessLayer.Service
{
    public class UserBL : IUserBL
    {
        private readonly IUserRL _userRL;
        private readonly JwtServices _jwtServices;
        private readonly IEmailService _emailService;
        private readonly IDatabase _redisDb;
        public UserBL(IUserRL userRL, JwtServices jwtServices, IEmailService emailService, IDatabase redisDb)
        {
            _userRL = userRL;
            _jwtServices = jwtServices;
            _emailService = emailService;
            _redisDb = redisDb;
        }

        public User RegisterBL(UserDTO userDTO)
        {
            try
            {
                var existingUser = _userRL.GetEmail(userDTO.Email);
                if (existingUser != null)
                {
                    throw new Exception("User already Registered!");
                }


                var newUser = new User
                {
                    Name = userDTO.Name,
                    Email = userDTO.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(userDTO.Password)

                };

                return _userRL.RegisterRL(newUser);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string LoginBL(UserDTO userDTO)
        {
            try
            {
                if (userDTO == null)
                {
                    throw new ArgumentNullException(nameof(userDTO), "Login data cannot be null.");
                }

                string cacheKey = $"Token: {userDTO.Email}";

                var cachedToken = _redisDb.StringGet(cacheKey);
                if (!cachedToken.IsNullOrEmpty)
                {
                    return cachedToken;
                }

                var data = _userRL.LoginRL(userDTO);

                if (data == null)
                {
                    throw new Exception("User not found! Please register first.");
                }

                if (!BCrypt.Net.BCrypt.Verify(userDTO.Password, data.Password))
                {
                    throw new Exception("Invalid credentials.");
                }

                string token = _jwtServices.GenerateToken(data);
                _redisDb.StringSet(cacheKey, token, TimeSpan.FromMinutes(30));
                return token;
            }

            catch (ArgumentNullException ex)
            {
                throw new Exception("Invalid login request: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Login failed: " + ex.Message);
            }
        }

        public bool ForgetPasswordBL(ForgetPasswordDTO forgetPasswordDTO)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(forgetPasswordDTO.Email))
                {
                    throw new ArgumentException();
                }
                var user = _userRL.GetEmail(forgetPasswordDTO.Email);
                if (user == null)
                {
                    return false;
                }
                string token = _jwtServices.GenerateToken(user);
                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException();
                }

                return _emailService.SendEmail(forgetPasswordDTO.Email, "Reset Password", token);
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
        }

        public bool ResetPasswordBL(ResetPasswordDTO resetPasswordDTO)
        {
            try
            {
                string email = _jwtServices.ValidateToken(resetPasswordDTO.Token);
                if (string.IsNullOrEmpty(email))
                {
                    return false;
                }

                var user = _userRL.GetEmail(email);
                if (user == null)
                {
                    return false;
                }

                user.Password = BCrypt.Net.BCrypt.HashPassword(resetPasswordDTO.NewPassword);

                return _userRL.UpdatePasswordRL(user);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool LogoutBL(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    throw new ArgumentNullException(nameof(email), "Email cannot be null or empty.");
                }

                string cacheKey = $"Token: {email}";

                if (_redisDb.KeyExists(cacheKey))
                {
                    _redisDb.KeyDelete(cacheKey);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception("Logout failed: " + ex.Message);
            }
        }
    }
}

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

namespace BusinessLayer.Service
{
    public class AuthBL : IAuthBL
    {
        private readonly IAuthRL _authRL;
        private readonly JwtServices _jwtServices;

        public AuthBL(IAuthRL authRL, JwtServices jwtServices)
        {
            _authRL = authRL;
            _jwtServices = jwtServices;
        }

        public User RegisterBL(UserDTO userDTO)
        {
            try
            {
                var existingUser = _authRL.GetEmail(userDTO.Email);
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

                return _authRL.RegisterRL(newUser);
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

                var data = _authRL.LoginRL(userDTO);

                if (data == null)
                {
                    throw new Exception("User not found! Please register first.");
                }

                if (!BCrypt.Net.BCrypt.Verify(userDTO.Password, data.Password))
                {
                    throw new Exception("Invalid credentials.");
                }

                return _jwtServices.GenerateToken(data); 
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
    }
}

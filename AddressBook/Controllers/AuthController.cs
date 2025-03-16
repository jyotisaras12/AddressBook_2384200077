using BusinessLayer.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.Model;
using RepositoryLayer.Entity;
using Newtonsoft.Json;
using RepositoryLayer.Interface;

namespace AddressBook.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserBL _userBL;
        private readonly IRabbitMQProducer _rabbitMQProducer;
        private readonly IEmailService _emailService;

        public AuthController(IUserBL userBL, IRabbitMQProducer rabbitMQProducer, IEmailService emailservice)
        {
            _userBL = userBL;
            _rabbitMQProducer = rabbitMQProducer;
            _emailService = emailservice;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] UserDTO userDTO)
        {
            try
            {
                var response = new ResponseModel<string>();
                var data = _userBL.RegisterBL(userDTO);
                if (data == null)
                {
                    response.Success = false;
                    response.Message = "User Already registered Successfully.";
                    response.Data = data.Email;

                    return BadRequest(response);
                }


                response.Success = true;
                response.Message = "User registered Successfully.";
                response.Data = data.Email;

                string jsonMessage = JsonConvert.SerializeObject(userDTO);

                try
                {
                    _rabbitMQProducer.PublishMessage(jsonMessage);
                    _emailService.SendEmail(data.Email, "Registration Successful", "You are registered successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"RabbitMQ or Email Error: {ex.Message}");
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Internal Server Error", error = ex.Message });
            }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserDTO userDTO)
        {
            try
            {
                var response = new ResponseModel<string>();
                var user = _userBL.LoginBL(userDTO);
                if (user != null)
                {
                    response.Success = true;
                    response.Message = "User Login Successfully.";
                    response.Data = user;
                    return Ok(response);
                }
                return BadRequest("Invalid Credentials!");
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("forget-password")]
        public IActionResult ForgetPassword(ForgetPasswordDTO forgetPasswordDTO)
        {
            try
            {
                var result = _userBL.ForgetPasswordBL(forgetPasswordDTO);
                if (!result)
                {
                    return BadRequest("Email not found!");
                }

                return Ok("Reset password email sent successfully.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

        }
        
        [HttpPost("reset-password")]
        public IActionResult ResetPassword(ResetPasswordDTO resetPasswordDTO)
        {
            try
            {
                var result = _userBL.ResetPasswordBL(resetPasswordDTO);
                if (!result)
                {
                    return BadRequest("Invalid or expired token.");
                }
                return Ok("Password reset successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout([FromBody] string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest("Email is required for logout.");
                }

                bool isLoggedOut = _userBL.LogoutBL(email);
                if (isLoggedOut)
                {
                    return Ok(new { Success = true, Message = "User logged out successfully." });
                }
                return BadRequest("User is not logged in or session expired.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

    }
}

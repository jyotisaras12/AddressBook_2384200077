using BusinessLayer.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.Model;
using RepositoryLayer.Entity;

namespace AddressBook.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthBL _authBL;

        public AuthController(IAuthBL authBL)
        {
            _authBL = authBL;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] UserDTO userDTO)
        {
            try
            {
                var response = new ResponseModel<string>();
                var data = _authBL.RegisterBL(userDTO);
                if (data == null)
                {
                    response.Success = false;
                    response.Message = "User Already registered Successfully.";
                    response.Data = data.Email;

                    return Ok(response);
                }


                response.Success = true;
                response.Message = "User registered Successfully.";
                response.Data = data.Email;
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserDTO userDTO)
        {
            try
            {
                var response = new ResponseModel<string>();
                var user = _authBL.LoginBL(userDTO);
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
    }
}

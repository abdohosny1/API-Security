using API_Security.Dto;
using API_Security.helper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API_Security.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthenticationController(UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistractionrDto userRegistractionrDto)
        {
            if (ModelState.IsValid)
            {
                //check user
                var existing_user = await _userManager.FindByEmailAsync(userRegistractionrDto.Email);

                if (existing_user != null)
                {
                    return BadRequest(new AuthResult()
                    {
                        Errors = new List<string>()
                                    {
                                        "Email already exist"
                                    },
                        Success = false
                    });
                }
                //create new user
                var new_user = new IdentityUser()
                {
                    Email = userRegistractionrDto.Email,
                    UserName = userRegistractionrDto.Email
                };

                var is_created=await _userManager.CreateAsync(new_user, userRegistractionrDto.Password);

                if (is_created.Succeeded)
                {

                    //require email confirmation
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(new_user);
                    return Ok($"plese confirm your email with the code that you have recieved {code}");
                }

                return BadRequest(new AuthResult()
                {

                    Success = true,
                    Errors =  new List<string> { "server error"}
                });

            }
            return BadRequest();

        }

        [HttpPost]
        [Route(template: "EmailVerification")]

          public async Task<IActionResult> EmailVerification(string? email, string? code)
        {
            if (email is null || code is null)
                return BadRequest(error: "invalid payload");

            var user = await _userManager.FindByEmailAsync(email);

            if(user is null)
                return BadRequest(error: "invalid payload");

            var is_verified =await _userManager.ConfirmEmailAsync(user, code);

            if (is_verified.Succeeded)
            {
                return Ok(new
                {
                    message = "email confirmed"
                });
            }

            return BadRequest("some thing went wrong");
        }


        [HttpPost]
        [Route("Login")]

        public async Task<IActionResult> Login([FromBody] LoginUserDto loginUserDto)
        {
            if (ModelState.IsValid)
            {
                //check user
                var existing_user = await _userManager.FindByEmailAsync(loginUserDto.Email);

                if (existing_user == null)
                {
                    return BadRequest(new AuthResult()
                    {
                        Errors = new List<string>()
                            {
                                "Invalid payload"
                            },
                        Success = false
                    });
                }

                //check email confirm
                var email_confirm = await _userManager.IsEmailConfirmedAsync(existing_user);
                if (!email_confirm)
                {
                    return Unauthorized(new AuthResult()
                    {
                        Errors = new List<string>()
                            {
                                "email is not confirmed"
                            },
                        Success = false
                    });
                }
                //check if the user is looked out
                var is_locked_out = await _userManager.IsLockedOutAsync(existing_user);

                if (is_locked_out)
                {
                    return Unauthorized(new AuthResult()
                    {
                        Errors = new List<string>()
                            {
                                "user is locked out please try again later"
                            },
                        Success = false
                    });
                }

                var is_correct = await _userManager.CheckPasswordAsync(existing_user, loginUserDto.Password);

                if (!is_correct)
                {
                    // increace the locked count
                    await _userManager.AccessFailedAsync(existing_user);

                    // check if the user is lockedout

                    var user_status = await _userManager.IsLockedOutAsync(existing_user);

                    if (user_status)
                    {
                        return Unauthorized(new AuthResult()
                        {
                            Errors = new List<string>()
                            {
                                "user is locked out please try again later"
                            },
                            Success = false
                        });
                    }

                    return BadRequest(new AuthResult()
                        {
                            Errors = new List<string>()
                            {
                                "Invalid credentials"
                            },
                            Success = false
                        });
                    


                }

                await _userManager.ResetAccessFailedCountAsync(existing_user);
                var jwt_token = GenerateJwtToken(existing_user);

                return Ok(new AuthResult()
                {
                   Token= jwt_token,
                    Success = true
                });

            }

            return BadRequest();

        }

        [HttpPost]
        [Route("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
        {
            if (ModelState.IsValid)
            {
                //check user
                var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);

                if (user == null)
                {
                    return BadRequest(new AuthResult()
                    {
                        Errors = new List<string>()
                            {
                                "Invalid payload"
                            },
                        Success = false
                    });
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest("some thing went wrong");
                }

                var call_back_url = $"http://localhost:9999/restpass?code={token}&email={user.Email}";

                //send emaill
                return Ok(new
                {
                    token = token,
                    email = user.Email
                });
            }
            return BadRequest("invalid payload");

        }

        [HttpPost]
        [Route("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestDto requestDto)
        {
            if (!ModelState.IsValid)
                return BadRequest("invalid payload");

            //check user
            var user = await _userManager.FindByEmailAsync(requestDto.Email);

            if (user == null)
            {
                return BadRequest("Invalid payload");
            }

            var result = await _userManager.ResetPasswordAsync(user, requestDto.Token, requestDto.Password);

            if (result.Succeeded)
            {
                return Ok("password reset is successfuly");
            }
            return BadRequest("some thing went wrong");



        }

        private string GenerateJwtToken(IdentityUser user)
        {
            var jwt_token_handler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration.GetSection("jwtConfig:secret").Value);


            var claim_list = new List<Claim>();
            claim_list.Add(new Claim("Id", user.Id));
            claim_list.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Email));
            claim_list.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
            claim_list.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            claim_list.Add(new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString()));

            var token_descriptor = new SecurityTokenDescriptor()
            {
                Subject=new ClaimsIdentity(claim_list),
                Expires=DateTime.Now.AddHours(1),
                SigningCredentials=new SigningCredentials(new SymmetricSecurityKey(key),SecurityAlgorithms.HmacSha256)
            };

            var token = jwt_token_handler.CreateToken(token_descriptor);
            return jwt_token_handler.WriteToken(token);
        }

    }
}

using API_Security.Dto;
using API_Security.helper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace API_Security.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;

        public AuthenticationController(UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
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
                    //generate the token

                    // var token = new GenerateJwtToken(new_user);

                    return Ok(new AuthResult()
                    {

                        Success = true,
                       // Token = token
                    });
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
        }

    }
}

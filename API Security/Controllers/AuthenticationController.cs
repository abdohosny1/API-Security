using API_Security.Dto;
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
        public async Task<IActionResult> Register([FromBody] )

        //    [HttpPost]

        //    public async Task<IActionResult> Login([FromBody] LoginUserDto loginUserDto)
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            //check user
        //            var existing_user= await _userManager.FindByEmailAsync(loginUserDto.Email);

        //            if (existing_user == null) 
        //            {
        //                return BadRequest(new AuthResult()
        //                {
        //                    DataErrorsChangedEventArgs = new List<string>()
        //                    {
        //                        "Invalid payload"
        //                    },
        //                    Results = false
        //                });
        //            }
        //            //check if the user is looked out
        //            var is_locked_out = await _userManager.IsLockedOutAsync(existing_user);

        //            if (is_locked_out) {
        //                return Unauthorized(new AuthResult()
        //                {
        //                    DataErrorsChangedEventArgs = new List<string>()
        //                    {
        //                        "user is locked out please try again later"
        //                    },
        //                    Results = false
        //                });
        //            }

        //            var is_correct = await _userManager.CheckPasswordAsync(existing_user, loginUserDto.Password);

        //            if (!is_correct)
        //            {
        //                return BadRequest(new AuthResult()
        //                {
        //                    DataErrorsChangedEventArgs = new List<string>()
        //                    {
        //                        "Invalid credentials"
        //                    },
        //                    Results = false
        //                });
        //            }

        //            var jwt_token = GenerateJwtToken(existing_user);
        //    }
        //}

    }
}

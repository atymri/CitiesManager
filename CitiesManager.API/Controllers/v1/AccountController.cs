using Asp.Versioning;
using CitiesManager.API.Filters.ActionFilters;
using CitiesManager.Core.DTOs.Identity;
using CitiesManager.Core.Identity;
using CitiesManager.Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CitiesManager.API.Controllers.v1
{
    [AllowAnonymous]
    [ApiVersion("1.0")]
    public class AccountController : BaseController
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IJwtService _jwtService;

        public AccountController(UserManager<User> userManager, 
            SignInManager<User> signInManager, 
            RoleManager<Role> roleManager,
            IJwtService jwtService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem();

            var user = request.ToUser(HttpContext.Connection.RemoteIpAddress?.ToString());

            var res = await _userManager.CreateAsync(user, request.Password);

            if (!res.Succeeded)
                return Problem(string.Join(',',
                    res.Errors.Select(e => e.Description)),
                        statusCode: StatusCodes.Status400BadRequest);

            var authResponse = _jwtService.CreateJwtToken(user);
            await _signInManager.SignInAsync(user, false);

            return Ok(authResponse);

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem();

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Problem("User was not found", 
                    statusCode: StatusCodes.Status400BadRequest);

            var res = await _signInManager.PasswordSignInAsync(request.Email, request.Password, false, true);
            if (!res.Succeeded)
                return Problem("Invalid email or password", 
                    statusCode: StatusCodes.Status400BadRequest);

            var authResponse = _jwtService.CreateJwtToken(user);

            return Ok(authResponse);
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return NoContent();
        }

        [HttpDelete("delete-account")]
        public async Task<IActionResult> DeleteAccount(DeleteAccountDTO request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem();

            var account = await _userManager.FindByEmailAsync(request.Email);
            if (account == null)
                return Problem("There is no such account", statusCode: StatusCodes.Status400BadRequest);

            var passwordValid = await _userManager.CheckPasswordAsync(account, request.Password);
            if (!passwordValid)
                return Problem("Invalid Password", statusCode: StatusCodes.Status400BadRequest);

            var res = await _userManager.DeleteAsync(account);
            if (!res.Succeeded)
                return Problem(string.Join(',',
                    res.Errors.Select(e => e.Description)), 
                    statusCode: StatusCodes.Status400BadRequest);
            return NoContent();
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem();

            var account = await _userManager.FindByEmailAsync(request.Email);
            if (account == null)
                return Problem("There is no such account", statusCode: StatusCodes.Status400BadRequest);
            
            var passwordValid = await _userManager.CheckPasswordAsync(account, request.OldPassword);
            if (!passwordValid)
                return Problem("Invalid Old Password", statusCode: StatusCodes.Status400BadRequest);
            
            var res = await _userManager.ChangePasswordAsync(account, request.OldPassword, request.NewPassword);
            if (!res.Succeeded)
                return Problem(string.Join(',',
                    res.Errors.Select(e => e.Description)),
                    statusCode: StatusCodes.Status400BadRequest);
            
            return NoContent();
        }


        [HttpGet("register-email-check")]
        [TypeFilter(typeof(AjaxOnlyActionFilter))]
        public async Task<IActionResult> IsEmailInUseForRegister(string email)
            => Ok(await _userManager.FindByEmailAsync(email) == null);

        [HttpGet("login-email-check")]
        [TypeFilter(typeof(AjaxOnlyActionFilter))]
        public async Task<IActionResult> IsEmailInUseForLogin(string email)
            => Ok(await _userManager.FindByEmailAsync(email) != null);

        [HttpGet("register-number-check")]
        [TypeFilter(typeof(AjaxOnlyActionFilter))]
        public async Task<IActionResult> IsNumberInUseForRegister(string phoneNumber)
            => Ok(await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber) == null);

        protected IActionResult ValidationProblem()
        {
            return Problem(string.Join(',',
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
        }

    }
}

using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.Marshalling;
using CitiesManager.Core.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CitiesManager.Core.DTOs.Identity
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "First name can't be null or empty")]
        [StringLength(50, ErrorMessage = "First name can't be more than 50 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name can't be null or empty")]
        [StringLength(50, ErrorMessage = "Last name can't be more than 50 characters")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address can't be null or empty")]
        [EmailAddress(ErrorMessage = "Email address is not in the correct format")]
        [Remote(action: "IsEmailInUseForRegister", controller: "Account", ErrorMessage = "Email address is already in use")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number can't be null or empty")]
        [Remote(action: "IsNumberInUseForRegister", controller: "Account", ErrorMessage = "Phone number is already in use")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Phone number is not in the correct format")]
        [StringLength(11, ErrorMessage = "Phone number can't be more that 11 digits")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password can't be null or empty")]
        public string Password { get; set; } = string.Empty;
        [Required(ErrorMessage = "Confirm Password can't be null or empty")]
        [Compare(nameof(Password), ErrorMessage = "Password and its confirmation are not the same")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public User ToUser(string? IpAddress)
        {
            return new User()
            {
                FirstName = this.FirstName,
                LastName = this.LastName,
                UserName = this.Email,
                Email = this.Email,
                PhoneNumber = this.PhoneNumber,
                IpAddress = IpAddress ?? "Unknown"
            };
        }


    }
}

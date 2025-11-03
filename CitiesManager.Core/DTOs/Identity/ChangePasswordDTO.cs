using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.Marshalling;
using CitiesManager.Core.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CitiesManager.Core.DTOs.Identity
{
    public class ChangePasswordDTO
    {

        [Required(ErrorMessage = "Email address can't be null or empty")]
        [EmailAddress(ErrorMessage = "Email address is not in the correct format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password can't be null or empty")]
        public string OldPassword { get; set; } = string.Empty;


        [Required(ErrorMessage = "New password can't be null or empty")]
        public string NewPassword { get; set; } = string.Empty;


    }
}

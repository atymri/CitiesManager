using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CitiesManager.Core.DTOs;
using CitiesManager.Core.Identity;

namespace CitiesManager.Core.ServiceContracts
{
    public interface IJwtService
    {
        AuthenticationResponse CreateJwtToken(User user);
        ClaimsPrincipal? GetPrincipalFromAccessToken(string token);
    }
}

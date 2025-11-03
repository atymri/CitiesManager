using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace CitiesManager.Core.Identity
{
    public class User : IdentityUser<Guid>
    {
        public string IpAddress { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}

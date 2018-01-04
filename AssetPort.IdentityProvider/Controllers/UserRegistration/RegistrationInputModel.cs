using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetPort.IdentityProvider.Controllers.UserRegistration
{
    public class RegistrationInputModel
    {
        public string ReturnUrl { get; set; }
        public string Provider { get; set; }
        public string ProviderUserId { get; set; }
    }
}

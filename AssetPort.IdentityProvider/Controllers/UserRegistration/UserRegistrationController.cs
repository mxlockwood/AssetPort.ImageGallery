using AssetPort.IdentityProvider.Services;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AssetPort.IdentityProvider.Controllers.UserRegistration
{
    public class UserRegistrationController : Controller
    {
        private IIdentityProviderUserRepository _userRespository;
        private IIdentityServerInteractionService _interaction;

        public UserRegistrationController(IIdentityProviderUserRepository userRepository, IIdentityServerInteractionService interaction)
        {
            _userRespository = userRepository;
            _interaction = interaction;
        }
        [HttpGet]
        public IActionResult RegisterUser(RegistrationInputModel registrationInputModel) //(string returnUrl)
        {
            //var vm = new RegisterUserViewModel()
            //{
            //    ReturnUrl = returnUrl
            //};

            var vm = new RegisterUserViewModel()
            {
                ReturnUrl = registrationInputModel.ReturnUrl,
                Provider = registrationInputModel.Provider,
                ProviderUserId = registrationInputModel.ProviderUserId
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterUser(RegisterUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Create user with default claims
                var userToCreate = new Entities.User();
                userToCreate.Password = model.Password;
                userToCreate.Username = model.Username;
                // This is where some other steps such as e-mail should be used for validation before setting IsActive to true.
                userToCreate.IsActive = true;
                userToCreate.Claims.Add(new Entities.UserClaim("country", model.Country));
                userToCreate.Claims.Add(new Entities.UserClaim("address", model.Address));
                userToCreate.Claims.Add(new Entities.UserClaim("given_name", model.Firstname));
                userToCreate.Claims.Add(new Entities.UserClaim("family_name", model.Lastname));
                userToCreate.Claims.Add(new Entities.UserClaim("email", model.Email));
                userToCreate.Claims.Add(new Entities.UserClaim("subscriptionlevel", "Guest"));

                // Provisioning a user via external login
                if (model.IsProvisioningFromExternal)
                {
                    userToCreate.Logins.Add(new Entities.UserLogin()
                    {
                        LoginProvider = model.Provider,
                        ProviderKey = model.ProviderUserId
                    });
                }

                // add it through the repository
                _userRespository.AddUser(userToCreate);

                if (!_userRespository.Save())
                {
                    throw new Exception($"Creating a user failed.");
                }

                if (!model.IsProvisioningFromExternal)
                {                 // log the user in HttpContext.Authentication is obsolete ... 
                    await HttpContext.SignInAsync(userToCreate.SubjectId, userToCreate.Username);
                }

                // continue with the flow     
                if (_interaction.IsValidReturnUrl(model.ReturnUrl) || Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                return Redirect("~/");
            }

            // ModelState invalid, return the view with the passed-in model
            // so changes can be made
            return View(model);
        }
    }
}

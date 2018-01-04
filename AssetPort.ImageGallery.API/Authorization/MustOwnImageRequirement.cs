using Microsoft.AspNetCore.Authorization;

namespace AssetPort.ImageGallery.API.Authorization
{
    public class MustOwnImageRequirement : IAuthorizationRequirement
    {
        public MustOwnImageRequirement()
        {

        }
    }
}

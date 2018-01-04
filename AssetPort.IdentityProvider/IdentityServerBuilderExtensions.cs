using AssetPort.IdentityProvider.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetPort.IdentityProvider
{
    public static class IdentityServerBuilderExtensions
    {
        public static IIdentityServerBuilder AddIdentityProviderUserStore(this IIdentityServerBuilder builder)
        {
            builder.Services.AddSingleton<IIdentityProviderUserRepository, IdentityProviderUserRepository>();
            builder.AddProfileService<IdentityProviderUserProfileService>();
            return builder;
        }
    }
}

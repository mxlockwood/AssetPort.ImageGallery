using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using AssetPort.ImageGallery.Client.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using System.Security.Claims;
using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Microsoft.ApplicationInsights.Extensibility;

namespace AssetPort.ImageGallery.Client
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            TelemetryConfiguration.Active.DisableTelemetry = true;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddAuthorization((options) => {
                options.AddPolicy("CanOrderFrame", policybuilder =>
                {
                    policybuilder.RequireAuthenticatedUser();
                    policybuilder.RequireClaim("subscriptionlevel", "Subscriber");
                    policybuilder.RequireClaim("country", "be");
                    //policybuilder.RequireRole("Subscriber");
                });
            });

            // Register an IHttpContextAccessor so we can access the current
            // HttpContext in services by injecting it
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Register an IImageGalleryHttpClient
            services.AddScoped<IImageGalleryHttpClient, ImageGalleryHttpClient>();

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // Frans is not using this

            // Configure OIDC
            services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.AccessDeniedPath = new PathString("/Authorization/AccessDenied");
            })
            .AddOpenIdConnect(options => 
            {
                //options.SignInScheme = "Cookies";
                options.Authority = "https://localhost:44355/"; // IdentityProvider; 385 is API
                options.ClientId = "imagegalleryclient";
                options.ClientSecret = "secret";
                options.ResponseType = "code id_token";
                options.SaveTokens = true;
                options.Scope.Add("offline_access");
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("address");
                options.Scope.Add("roles");
                options.Scope.Add("imagegalleryapi");
                options.Scope.Add("subscriptionlevel");
                options.Scope.Add("country");
                options.GetClaimsFromUserInfoEndpoint = true; // UserInfo Endpoint
                options.RequireHttpsMetadata = true;
                options.ClaimActions.MapUniqueJsonKey("role", "role");
                //options.ClaimActions.MapUniqueJsonKey(ClaimTypes.Email, "email");
                //options.ClaimActions.MapUniqueJsonKey("email", "email");
                options.ClaimActions.MapUniqueJsonKey("subscriptionlevel", "subscriptionlevel");
                options.ClaimActions.MapUniqueJsonKey("country", "country");

                options.Events = new OpenIdConnectEvents()
                {
                    OnTicketReceived = ticketReceivedContext =>
                    {
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = tokenValidatedContext =>
                    {
                        var identity = tokenValidatedContext.Principal.Identity as ClaimsIdentity;

                        var targetClaims = identity.Claims.Where(c => new[] { "subscriptionlevel", "country", "role", "sub" }.Contains(c.Type));

                        var newClaimsIdentity = new ClaimsIdentity(
                          targetClaims,
                          identity.AuthenticationType,
                          "given_name",
                          "role");

                        tokenValidatedContext.Principal = new ClaimsPrincipal(newClaimsIdentity);

                        return Task.CompletedTask;
                    },
                    OnUserInformationReceived = userInformationReceived =>
                    {
                        userInformationReceived.User.Remove("address");
                        return Task.FromResult(0);
                    }
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Shared/Error");
            }

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.DefaultInboundClaimFilter.Clear();

            app.UseAuthentication();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Gallery}/{action=Index}/{id?}");
            });
        }

        // Not used, but interesting.
        // Source: https://stackoverflow.com/questions/46038509/unable-to-retrieve-claims-in-net-core-2-0
        private Task OnUserInformationReceivedHandler(UserInformationReceivedContext context)
        {
            if (!(context.Principal.Identity is ClaimsIdentity claimsId))
            {
                throw new Exception();
            }

            // Get a list of all claims attached to the UserInformationRecieved context
            var ctxClaims = context.User.Children().ToList();

            foreach (var ctxClaim in ctxClaims)
            {
                var claimType = ctxClaim.Path;
                var token = ctxClaim.FirstOrDefault();
                if (token == null)
                {
                    continue;
                }

                var claims = new List<Claim>();
                if (token.Children().Any())
                {
                    claims.AddRange(
                        token.Children()
                            .Select(c => new Claim(claimType, c.Value<string>())));
                }
                else
                {
                    claims.Add(new Claim(claimType, token.Value<string>()));
                }

                foreach (var claim in claims)
                {
                    if (!claimsId.Claims.Any(
                        c => c.Type == claim.Type &&
                             c.Value == claim.Value))
                    {
                        claimsId.AddClaim(claim);
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}

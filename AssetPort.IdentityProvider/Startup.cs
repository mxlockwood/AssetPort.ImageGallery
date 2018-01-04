using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AssetPort.IdentityProvider.Entities;
using AssetPort.IdentityProvider.Services;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Cryptography.X509Certificates;
using System;
using System.Reflection;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.ApplicationInsights.Extensibility;

namespace AssetPort.IdentityProvider
{
    public class Startup
    {
        public static IConfigurationRoot Configuration;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
            TelemetryConfiguration.Active.DisableTelemetry = true;
        }

        public X509Certificate2 LoadCertificateFromStore(string thumbPrint)
        {

            using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.ReadOnly);
                var certCollection = store.Certificates.Find(X509FindType.FindByThumbprint,
                    thumbPrint, true);
                if (certCollection.Count == 0)
                {
                    throw new Exception("The specified certificate wasn't found. Check the specified thumbprint.");
                }
                return certCollection[0];
            }
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration["connectionStrings:DefaultConnection"];
            services.AddDbContext<IdentityProviderUserContext>(o => o.UseSqlServer(connectionString));

            var identityServerConnectionString = Configuration["connectionStrings:IdentityServerConnection"];
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.AddMvc();

            services.AddIdentityServer()
                .AddSigningCredential(LoadCertificateFromStore(Configuration["signingCredentialCertificateThumbPrint"]))
                .AddIdentityProviderUserStore()
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = (context) => context.UseSqlServer(identityServerConnectionString, sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(migrationsAssembly);
                    });
                })
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = (context) => context.UseSqlServer(identityServerConnectionString, sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(migrationsAssembly);
                    });
                });
                //.AddDeveloperSigningCredential()
                //.AddInMemoryIdentityResources(Config.GetIdentityResources())
                //.AddInMemoryApiResources(Config.GetApiResources())
                //.AddInMemoryClients(Config.GetClients());

            // If this is moved up before services.AddIdentityServer, it fails
            services.AddScoped<IIdentityProviderUserRepository, IdentityProviderUserRepository>();

            services.Configure<IISOptions>(iis =>
            {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });

            //services.Configure<FacebookAuthenticationOptions>(options =>
            //{
            //    options.AppId = Configuration["Authentication:Facebook:AppId"];
            //    options.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
            //    options.Scope.Add("email");
            //});

            //services.AddAuthentication(options => {
            //    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            //    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //})
            //.AddCookie(options =>
            //{
            //    options.AccessDeniedPath = new PathString("/Authorization/AccessDenied");
            //})

            services.AddAuthentication().AddFacebook("Facebook", "Facebook", options =>
            {

                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                options.AppId = "1236026753197527";
                options.AppSecret = "ca1c5fac1a72c14482bcdc0e179a81f6";
                //options.Scope.Add("email");
                //options.Fields.Add("email");
                //options.ClaimActions.MapUniqueJsonKey("email", "email");
                //options.SaveTokens = true;
                //options.UserInformationEndpoint = "https://graph.facebook.com/v2.5/me?fields=id,name,email";
                //options.ClaimActions.MapUniqueJsonKey("Email", "email");
            })
            .AddCookie("idsrv.2FA", options =>
            { });
            // OR
            //.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            //{
            //    options.Auh               
            //)};
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, 
            IdentityProviderUserContext userContext, ConfigurationDbContext configurationDbContext, PersistedGrantDbContext persistedGrantDbContext)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            configurationDbContext.Database.Migrate();
            configurationDbContext.EnsureSeedDataForContext();

            persistedGrantDbContext.Database.Migrate();

            userContext.Database.Migrate();
            userContext.EnsureSeedDataForContext();

            app.UseIdentityServer(); // Try localhost:59866/.well-known/openid-configuration

            app.UseStaticFiles();

            app.UseMvcWithDefaultRoute();
        }
    }
}

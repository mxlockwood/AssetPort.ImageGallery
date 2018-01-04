using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.IO;

namespace AssetPort.IdentityProvider.Entities
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<IdentityProviderUserContext>
    {
        public IdentityProviderUserContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            var builder = new DbContextOptionsBuilder<IdentityProviderUserContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            builder.UseSqlServer(connectionString);
            return new IdentityProviderUserContext(builder.Options);
        }
    }
}

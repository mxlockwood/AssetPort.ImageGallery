using Microsoft.EntityFrameworkCore;

namespace AssetPort.IdentityProvider.Entities
{
    public class IdentityProviderUserContext : DbContext
    {
        public IdentityProviderUserContext(DbContextOptions<IdentityProviderUserContext> options)
           : base(options)
        {
           
        }

        public DbSet<User> Users { get; set; }
    }
}

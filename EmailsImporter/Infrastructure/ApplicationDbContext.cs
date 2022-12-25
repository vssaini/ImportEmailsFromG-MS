using System.Data.Entity;

namespace EmailsImporter.Infrastructure
{
    internal class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("DefaultConnection")
        {
            //Database.SetInitializer(new DropCreateDatabaseAlways<ApplicationDbContext>());
        }

        public DbSet<GoogleAuthStore> GoogleAuthStore { get; set; }
    }
}

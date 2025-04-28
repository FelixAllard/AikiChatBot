using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.IdentityModel.Tokens;

namespace AikiDataBuilder.Database
{
    /// <summary>
    /// This is used by ASP.Net EntityFramework for building the database instead of the program.cs
    /// </summary>
    public class SherwebDbContextFactory : IDesignTimeDbContextFactory<SherwebDbContext>
    {
        /// <summary>
        /// Create the DB Context
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public SherwebDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SherwebDbContext>();

            // Make sure this connection string is valid at design time!
            var connectionString = Environment.GetEnvironmentVariable("DefaultConnection")
                                   ?? "Server=localhost,1433;Database=my_database;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;";
            if(connectionString.IsNullOrEmpty())
                connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION_STRING_AIKIBUILDER");

            optionsBuilder.UseSqlServer(connectionString);

            return new SherwebDbContext(optionsBuilder.Options);
        }
    }
}

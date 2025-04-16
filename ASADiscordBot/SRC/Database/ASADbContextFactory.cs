
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ASADiscordBot.Database
{
    /// <summary>
    /// This is used by ASP.Net EntityFramework for building the database instead of the program.cs
    /// </summary>
    public class ASADbContextFactory : IDesignTimeDbContextFactory<ASADbContext>
    {
        /// <summary>
        /// Create the DB Context
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public ASADbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ASADbContext>();

            // Make sure this connection string is valid at design time!
            var connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION_STRING")
                                   ?? "Server=localhost,1433;Database=asa_database;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;";

            optionsBuilder.UseSqlServer(connectionString);

            return new ASADbContext(optionsBuilder.Options);
        }
    }
}

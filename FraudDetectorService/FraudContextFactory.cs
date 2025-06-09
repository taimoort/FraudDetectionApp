using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FraudDetectorService
{
    public class FraudContextFactory : IDesignTimeDbContextFactory<FraudContext>
    {
        public FraudContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<FraudContext>();
            optionsBuilder.UseNpgsql(
                "Host=localhost;Port=5432;Database=fraud_db;Username=postgres;Password=postgres");

            return new FraudContext(optionsBuilder.Options);
        }
    }
}
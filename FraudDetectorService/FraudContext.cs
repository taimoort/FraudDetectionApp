using Microsoft.EntityFrameworkCore;
namespace FraudDetectorService
{
    public class FraudContext : DbContext
    {
        public FraudContext(DbContextOptions<FraudContext> options) : base(options) { }

        public DbSet<FlaggedTransaction> FlaggedTransactions { get; set; }
    }
}
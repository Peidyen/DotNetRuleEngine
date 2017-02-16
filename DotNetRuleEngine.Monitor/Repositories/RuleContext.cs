using DotNetRuleEngine.Monitor.Models;
using Microsoft.EntityFrameworkCore;

namespace DotNetRuleEngine.Monitor.Repositories
{
    public class DotNetRuleEngineModelContext : DbContext
    {
        public DbSet<DotNetRuleEngineModel> DotNetRuleEngineModel { get; set; }
        public DbSet<RuleModel> RuleModel { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=(local);Initial Catalog=DotNetRuleEngineRuleDB;Integrated Security=True");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DotNetRuleEngineModel>()
                .HasAlternateKey(model => model.RuleEngineId);

            modelBuilder.Entity<DotNetRuleEngineModel>()
                .HasIndex(model => model.RuleEngineId);

            modelBuilder.Entity<DotNetRuleEngineModel>()
                .Property(model => model.Timestamp)
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();

            modelBuilder.Entity<RuleModel>()
               .Property(model => model.RuleType)
               .IsRequired();

            modelBuilder.Entity<RuleModel>()
              .Property(model => model.JsonModel)
              .IsRequired();

            modelBuilder.Entity<RuleModel>()
              .Property(model => model.Rule)
              .IsRequired();
        }
    }
}

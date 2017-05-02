using DotNetRuleEngine.Monitor.Domain;
using Microsoft.EntityFrameworkCore;
using Model = DotNetRuleEngine.Monitor.Domain.Model;


namespace DotNetRuleEngine.Monitor.Repositories
{
    public class DotNetRuleEngineModelContext : DbContext
    {
        public DbSet<RuleEngine> DotNetRuleEngineModel { get; set; }
        public DbSet<Domain.Model> RuleModel { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=(local);Initial Catalog=DotNetRuleEngineRuleDB;Integrated Security=True");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RuleEngine>()
                .HasAlternateKey(model => model.RuleEngineId);

            modelBuilder.Entity<RuleEngine>()
                .HasIndex(model => model.RuleEngineId);

            modelBuilder.Entity<RuleEngine>()
                .Property(model => model.Timestamp)
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();

            modelBuilder.Entity<Model>()
               .Property(model => model.RuleType)
               .IsRequired();

            modelBuilder.Entity<Model>()
              .Property(model => model.JsonModel)
              .IsRequired();

            modelBuilder.Entity<Model>()
              .Property(model => model.Rule)
              .IsRequired();
        }
    }
}

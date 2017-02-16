using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using DotNetRuleEngine.Monitor.Repositories;

namespace DotNetRuleEngine.Monitor.Migrations
{
    [DbContext(typeof(DotNetRuleEngineModelContext))]
    [Migration("20161205213315_db_init")]
    partial class db_init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.0-rtm-22752")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("DotNetRuleEngine.Monitor.Models.DotNetRuleEngineModel", b =>
                {
                    b.Property<int>("DotNetRuleEngineModelId")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("RuleEngineId");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                    b.HasKey("DotNetRuleEngineModelId");

                    b.HasAlternateKey("RuleEngineId");

                    b.HasIndex("RuleEngineId");

                    b.ToTable("DotNetRuleEngineModel");
                });

            modelBuilder.Entity("DotNetRuleEngine.Monitor.Models.RuleModel", b =>
                {
                    b.Property<int>("RuleModelId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("DotNetRuleEngineModelId");

                    b.Property<string>("JsonModel")
                        .IsRequired();

                    b.Property<string>("ObservingRule");

                    b.Property<string>("Rule")
                        .IsRequired();

                    b.Property<int>("RuleType");

                    b.HasKey("RuleModelId");

                    b.HasIndex("DotNetRuleEngineModelId");

                    b.ToTable("RuleModel");
                });

            modelBuilder.Entity("DotNetRuleEngine.Monitor.Models.RuleModel", b =>
                {
                    b.HasOne("DotNetRuleEngine.Monitor.Models.DotNetRuleEngineModel")
                        .WithMany("RuleModels")
                        .HasForeignKey("DotNetRuleEngineModelId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}

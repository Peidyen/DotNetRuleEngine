using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DotNetRuleEngine.Monitor.Migrations
{
    public partial class db_init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DotNetRuleEngineDomain",
                columns: table => new
                {
                    DotNetRuleEngineModelId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RuleEngineId = table.Column<Guid>(nullable: false),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DotNetRuleEngineModel", x => x.DotNetRuleEngineModelId);
                    table.UniqueConstraint("AK_DotNetRuleEngineModel_RuleEngineId", x => x.RuleEngineId);
                });

            migrationBuilder.CreateTable(
                name: "RuleModel",
                columns: table => new
                {
                    RuleModelId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DotNetRuleEngineModelId = table.Column<int>(nullable: false),
                    JsonModel = table.Column<string>(nullable: false),
                    ObservingRule = table.Column<string>(nullable: true),
                    Rule = table.Column<string>(nullable: false),
                    RuleType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuleModel", x => x.RuleModelId);
                    table.ForeignKey(
                        name: "FK_RuleModel_DotNetRuleEngineModel_DotNetRuleEngineModelId",
                        column: x => x.DotNetRuleEngineModelId,
                        principalTable: "DotNetRuleEngineDomain",
                        principalColumn: "DotNetRuleEngineModelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DotNetRuleEngineModel_RuleEngineId",
                table: "DotNetRuleEngineDomain",
                column: "RuleEngineId");

            migrationBuilder.CreateIndex(
                name: "IX_RuleModel_DotNetRuleEngineModelId",
                table: "RuleModel",
                column: "DotNetRuleEngineModelId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RuleModel");

            migrationBuilder.DropTable(
                name: "DotNetRuleEngineDomain");
        }
    }
}

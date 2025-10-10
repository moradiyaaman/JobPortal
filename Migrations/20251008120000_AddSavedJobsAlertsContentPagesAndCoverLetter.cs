using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace JobPortal.Migrations
{
    public partial class AddSavedJobsAlertsContentPagesAndCoverLetter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoverLetter",
                table: "JobApplications",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ContentPages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Slug = table.Column<string>(maxLength: 100, nullable: false),
                    Title = table.Column<string>(maxLength: 160, nullable: false),
                    Body = table.Column<string>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentPages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentPages_Slug",
                table: "ContentPages",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateTable(
                name: "JobAlertSubscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(nullable: false),
                    Keyword = table.Column<string>(maxLength: 160, nullable: true),
                    Country = table.Column<string>(maxLength: 80, nullable: true),
                    JobType = table.Column<string>(maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    LastNotifiedAt = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobAlertSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobAlertSubscriptions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobAlertSubscriptions_UserId_Keyword_Country_JobType",
                table: "JobAlertSubscriptions",
                columns: new[] { "UserId", "Keyword", "Country", "JobType" },
                unique: true);

            migrationBuilder.CreateTable(
                name: "SavedJobs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<int>(nullable: false),
                    UserId = table.Column<string>(nullable: false),
                    SavedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedJobs_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SavedJobs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SavedJobs_JobId_UserId",
                table: "SavedJobs",
                columns: new[] { "JobId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SavedJobs_UserId",
                table: "SavedJobs",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentPages");

            migrationBuilder.DropTable(
                name: "JobAlertSubscriptions");

            migrationBuilder.DropTable(
                name: "SavedJobs");

            migrationBuilder.DropColumn(
                name: "CoverLetter",
                table: "JobApplications");
        }
    }
}

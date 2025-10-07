using Microsoft.EntityFrameworkCore.Migrations;

namespace JobPortal.Migrations
{
    public partial class AddProviderAndAdminFeatures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProviderDisplayName",
                table: "Jobs",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderId",
                table: "Jobs",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderSummary",
                table: "Jobs",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyDescription",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyLocation",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyLogoPath",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyWebsite",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsProvider",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_ProviderId",
                table: "Jobs",
                column: "ProviderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_AspNetUsers_ProviderId",
                table: "Jobs",
                column: "ProviderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_AspNetUsers_ProviderId",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_ProviderId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "ProviderDisplayName",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "ProviderId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "ProviderSummary",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "CompanyDescription",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CompanyLocation",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CompanyLogoPath",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CompanyWebsite",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsProvider",
                table: "AspNetUsers");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectImageClientId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientId",
                table: "ProjectImages",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectImages_ProjectId_ClientId",
                table: "ProjectImages",
                columns: new[] { "ProjectId", "ClientId" },
                unique: true,
                filter: "[ClientId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectImages_ProjectId_ClientId",
                table: "ProjectImages");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "ProjectImages");
        }
    }
}

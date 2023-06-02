using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrucibleBugTracker.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedTicketAttachmentModel_003 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "TicketAttachments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "TicketAttachments");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nino.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddAirNotificationEstimates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AirNotificationPosted",
                table: "Episodes",
                newName: "AirNotificationStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AirNotificationStatus",
                table: "Episodes",
                newName: "AirNotificationPosted");
        }
    }
}

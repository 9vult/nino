using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nino.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddObserverProgressGroupInfoOption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IncludeGroupNameInObserverProgress",
                table: "Configuration",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IncludeGroupNameInObserverProgress",
                table: "Configuration");
        }
    }
}

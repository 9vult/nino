using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nino.Migrations
{
    /// <inheritdoc />
    public partial class PublishPrivateProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PublishPrivateProgress",
                table: "Configurations",
                type: "INTEGER",
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "PublishPrivateProgress", table: "Configurations");
        }
    }
}

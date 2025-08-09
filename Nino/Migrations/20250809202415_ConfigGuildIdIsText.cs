using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nino.Migrations
{
    /// <inheritdoc />
    public partial class ConfigGuildIdIsText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "GuildId",
                table: "Configurations",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "INTEGER");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "Configurations",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");
        }
    }
}

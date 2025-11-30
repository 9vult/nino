using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nino.Migrations
{
    /// <inheritdoc />
    public partial class AirReminderDelay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "AirReminderDelay",
                table: "Projects",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AirReminderDelay",
                table: "Projects");
        }
    }
}

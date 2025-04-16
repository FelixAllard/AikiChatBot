using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASADiscordBot.Migrations
{
    /// <inheritdoc />
    public partial class AddedChatSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastChat",
                table: "Identities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastQuerry",
                table: "Identities",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastChat",
                table: "Identities");

            migrationBuilder.DropColumn(
                name: "LastQuerry",
                table: "Identities");
        }
    }
}

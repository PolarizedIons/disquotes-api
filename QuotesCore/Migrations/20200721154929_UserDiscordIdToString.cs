using Microsoft.EntityFrameworkCore.Migrations;

namespace QuotesApi.Migrations
{
    public partial class UserDiscordIdToString : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DiscordId",
                table: "Users",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<ulong>(
                name: "DiscordId",
                table: "Users",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}

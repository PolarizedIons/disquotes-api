using Microsoft.EntityFrameworkCore.Migrations;

namespace QuotesCore.Migrations
{
    public partial class AddQuoteTitle : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Quotes",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "Quotes");
        }
    }
}

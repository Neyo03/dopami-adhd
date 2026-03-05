using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeManager.Migrations
{
    /// <inheritdoc />
    public partial class AddXpEarned : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "XpEarned",
                table: "TaskCompletions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "XpEarned",
                table: "TaskCompletions");
        }
    }
}

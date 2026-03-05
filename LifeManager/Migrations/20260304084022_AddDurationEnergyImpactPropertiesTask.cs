using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeManager.Migrations
{
    /// <inheritdoc />
    public partial class AddDurationEnergyImpactPropertiesTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "HouseTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Energy",
                table: "HouseTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Impact",
                table: "HouseTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "HouseTasks");

            migrationBuilder.DropColumn(
                name: "Energy",
                table: "HouseTasks");

            migrationBuilder.DropColumn(
                name: "Impact",
                table: "HouseTasks");
        }
    }
}

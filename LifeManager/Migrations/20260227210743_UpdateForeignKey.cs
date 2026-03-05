using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeManager.Migrations
{
    /// <inheritdoc />
    public partial class UpdateForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HouseTasks_Users_UserAssignedId",
                table: "HouseTasks");

            migrationBuilder.AlterColumn<int>(
                name: "UserAssignedId",
                table: "HouseTasks",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_HouseTasks_Users_UserAssignedId",
                table: "HouseTasks",
                column: "UserAssignedId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HouseTasks_Users_UserAssignedId",
                table: "HouseTasks");

            migrationBuilder.AlterColumn<int>(
                name: "UserAssignedId",
                table: "HouseTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_HouseTasks_Users_UserAssignedId",
                table: "HouseTasks",
                column: "UserAssignedId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

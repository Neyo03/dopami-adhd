using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeManager.Migrations
{
    /// <inheritdoc />
    public partial class ChangeRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Tags_HomeId",
                table: "Tags",
                column: "HomeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Homes_HomeId",
                table: "Tags",
                column: "HomeId",
                principalTable: "Homes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Homes_HomeId",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_HomeId",
                table: "Tags");
        }
    }
}

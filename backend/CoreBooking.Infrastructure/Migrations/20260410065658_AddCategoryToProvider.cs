using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryToProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Providers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Providers_CategoryId",
                table: "Providers",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Providers_Categories_CategoryId",
                table: "Providers",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Providers_Categories_CategoryId",
                table: "Providers");

            migrationBuilder.DropIndex(
                name: "IX_Providers_CategoryId",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Providers");
        }
    }
}

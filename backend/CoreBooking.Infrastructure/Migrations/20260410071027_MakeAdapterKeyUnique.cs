using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeAdapterKeyUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AdapterKey",
                table: "Providers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Providers_AdapterKey",
                table: "Providers",
                column: "AdapterKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Providers_AdapterKey",
                table: "Providers");

            migrationBuilder.AlterColumn<string>(
                name: "AdapterKey",
                table: "Providers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}

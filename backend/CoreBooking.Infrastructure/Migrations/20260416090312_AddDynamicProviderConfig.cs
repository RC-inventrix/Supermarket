using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDynamicProviderConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Providers_AdapterKey",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "AdapterKey",
                table: "Providers");

            migrationBuilder.AddColumn<string>(
                name: "AvailabilityEndpoint",
                table: "Providers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CatalogEndpoint",
                table: "Providers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CheckoutEndpoint",
                table: "Providers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MappingConfigJson",
                table: "Providers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SupplierBaseUrl",
                table: "Providers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailabilityEndpoint",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "CatalogEndpoint",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "CheckoutEndpoint",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "MappingConfigJson",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "SupplierBaseUrl",
                table: "Providers");

            migrationBuilder.AddColumn<string>(
                name: "AdapterKey",
                table: "Providers",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Providers_AdapterKey",
                table: "Providers",
                column: "AdapterKey",
                unique: true);
        }
    }
}

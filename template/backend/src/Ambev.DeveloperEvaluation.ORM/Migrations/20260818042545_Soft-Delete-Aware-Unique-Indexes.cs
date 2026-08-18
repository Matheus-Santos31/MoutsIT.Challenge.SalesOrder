using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ambev.DeveloperEvaluation.ORM.Migrations
{
    /// <inheritdoc />
    public partial class SoftDeleteAwareUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_UserAddresses_UserId_AddressId",
                table: "UserAddresses");

            migrationBuilder.DropIndex(
                name: "IX_ProductRates_ProductId",
                table: "ProductRates");

            migrationBuilder.DropIndex(
                name: "IX_ProductEvaluations_ProductId_UserId",
                table: "ProductEvaluations");

            migrationBuilder.DropIndex(
                name: "IX_BranchRates_BranchId",
                table: "BranchRates");

            migrationBuilder.DropIndex(
                name: "IX_BranchEvaluations_BranchId_UserId",
                table: "BranchEvaluations");

            migrationBuilder.DropIndex(
                name: "IX_BranchAddresses_BranchId_AddressId",
                table: "BranchAddresses");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserAddresses_UserId_AddressId",
                table: "UserAddresses",
                columns: new[] { "UserId", "AddressId" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRates_ProductId",
                table: "ProductRates",
                column: "ProductId",
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProductEvaluations_ProductId_UserId",
                table: "ProductEvaluations",
                columns: new[] { "ProductId", "UserId" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BranchRates_BranchId",
                table: "BranchRates",
                column: "BranchId",
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BranchEvaluations_BranchId_UserId",
                table: "BranchEvaluations",
                columns: new[] { "BranchId", "UserId" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BranchAddresses_BranchId_AddressId",
                table: "BranchAddresses",
                columns: new[] { "BranchId", "AddressId" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_UserAddresses_UserId_AddressId",
                table: "UserAddresses");

            migrationBuilder.DropIndex(
                name: "IX_ProductRates_ProductId",
                table: "ProductRates");

            migrationBuilder.DropIndex(
                name: "IX_ProductEvaluations_ProductId_UserId",
                table: "ProductEvaluations");

            migrationBuilder.DropIndex(
                name: "IX_BranchRates_BranchId",
                table: "BranchRates");

            migrationBuilder.DropIndex(
                name: "IX_BranchEvaluations_BranchId_UserId",
                table: "BranchEvaluations");

            migrationBuilder.DropIndex(
                name: "IX_BranchAddresses_BranchId_AddressId",
                table: "BranchAddresses");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAddresses_UserId_AddressId",
                table: "UserAddresses",
                columns: new[] { "UserId", "AddressId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductRates_ProductId",
                table: "ProductRates",
                column: "ProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductEvaluations_ProductId_UserId",
                table: "ProductEvaluations",
                columns: new[] { "ProductId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchRates_BranchId",
                table: "BranchRates",
                column: "BranchId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchEvaluations_BranchId_UserId",
                table: "BranchEvaluations",
                columns: new[] { "BranchId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchAddresses_BranchId_AddressId",
                table: "BranchAddresses",
                columns: new[] { "BranchId", "AddressId" },
                unique: true);
        }
    }
}

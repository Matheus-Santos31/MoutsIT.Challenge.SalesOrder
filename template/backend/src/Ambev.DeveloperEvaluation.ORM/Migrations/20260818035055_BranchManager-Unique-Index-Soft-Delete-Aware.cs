using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ambev.DeveloperEvaluation.ORM.Migrations
{
    /// <inheritdoc />
    public partial class BranchManagerUniqueIndexSoftDeleteAware : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BranchManagers_UserId",
                table: "BranchManagers");

            migrationBuilder.CreateIndex(
                name: "IX_BranchManagers_UserId",
                table: "BranchManagers",
                column: "UserId",
                unique: true,
                filter: "\"DeletedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BranchManagers_UserId",
                table: "BranchManagers");

            migrationBuilder.CreateIndex(
                name: "IX_BranchManagers_UserId",
                table: "BranchManagers",
                column: "UserId",
                unique: true);
        }
    }
}

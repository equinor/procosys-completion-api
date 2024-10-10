using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Equinor.ProCoSys.Completion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HistoryIndexMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_History_EventByOid",
                table: "History",
                column: "EventByOid");

            migrationBuilder.CreateIndex(
                name: "IX_History_EventForGuid",
                table: "History",
                column: "EventForGuid");

            migrationBuilder.CreateIndex(
                name: "IX_History_EventForParentGuid",
                table: "History",
                column: "EventForParentGuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_History_EventByOid",
                table: "History");

            migrationBuilder.DropIndex(
                name: "IX_History_EventForGuid",
                table: "History");

            migrationBuilder.DropIndex(
                name: "IX_History_EventForParentGuid",
                table: "History");
        }
    }
}

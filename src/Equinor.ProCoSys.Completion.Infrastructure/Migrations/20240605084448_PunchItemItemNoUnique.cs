using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Equinor.ProCoSys.Completion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PunchItemItemNoUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PunchItems_ItemNo",
                table: "PunchItems",
                column: "ItemNo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PunchItems_ItemNo",
                table: "PunchItems");
        }
    }
}

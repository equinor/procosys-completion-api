using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Equinor.ProCoSys.Completion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PunchItemIndexMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PunchItems_Plant_CheckListGuid",
                table: "PunchItems",
                columns: new[] { "Plant", "CheckListGuid" })
                .Annotation("SqlServer:Include", new[] { "Guid" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PunchItems_Plant_CheckListGuid",
                table: "PunchItems");
        }
    }
}

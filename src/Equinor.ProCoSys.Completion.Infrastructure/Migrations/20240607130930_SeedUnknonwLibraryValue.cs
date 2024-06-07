using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Equinor.ProCoSys.Completion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedUnknonwLibraryValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Library",
                columns: new[] { "Id", "Code", "Description", "Guid", "IsVoided", "Plant", "ProCoSys4LastUpdated", "SyncTimestamp", "Type" },
                values: new object[] { -1, "UNKNOWN", "Null value in oracle db", new Guid("0e8be842-6821-4733-b1b1-801574b4fe2b"), true, "N/A", new DateTime(2024, 6, 7, 15, 9, 29, 737, DateTimeKind.Local).AddTicks(7292), new DateTime(2024, 6, 7, 15, 9, 29, 737, DateTimeKind.Local).AddTicks(7360), "COMPLETION_ORGANIZATION" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Library",
                keyColumn: "Id",
                keyValue: -1);
        }
    }
}

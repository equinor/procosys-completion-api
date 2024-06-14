using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Equinor.ProCoSys.Completion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovSeededLibValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Library",
                keyColumn: "Id",
                keyValue: -1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Library",
                columns: new[] { "Id", "Code", "Description", "Guid", "IsVoided", "Plant", "ProCoSys4LastUpdated", "SyncTimestamp", "Type" },
                values: new object[] { -1, "UNKNOWN", "Null value in oracle db", new Guid("dbd52718-a64e-45a8-b1c5-8779d6c7170b"), true, "N/A", new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "COMPLETION_ORGANIZATION" });
        }
    }
}

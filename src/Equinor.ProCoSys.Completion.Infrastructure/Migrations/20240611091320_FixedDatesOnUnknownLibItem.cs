using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Equinor.ProCoSys.Completion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixedDatesOnUnknownLibItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Library",
                keyColumn: "Id",
                keyValue: -1,
                columns: new[] { "Guid", "ProCoSys4LastUpdated", "SyncTimestamp" },
                values: new object[] { new Guid("78b49af6-e872-488c-8d69-3a8046e5a53c"), new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Library",
                keyColumn: "Id",
                keyValue: -1,
                columns: new[] { "Guid", "ProCoSys4LastUpdated", "SyncTimestamp" },
                values: new object[] { new Guid("b6498bdf-c063-4855-874d-9a12fb5f5406"), new DateTime(2024, 6, 11, 7, 17, 46, 418, DateTimeKind.Local).AddTicks(2485), new DateTime(2024, 6, 11, 7, 17, 46, 418, DateTimeKind.Local).AddTicks(2591) });
        }
    }
}

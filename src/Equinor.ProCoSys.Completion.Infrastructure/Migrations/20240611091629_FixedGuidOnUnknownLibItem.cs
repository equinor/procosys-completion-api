using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Equinor.ProCoSys.Completion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixedGuidOnUnknownLibItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Library",
                keyColumn: "Id",
                keyValue: -1,
                column: "Guid",
                value: new Guid("a075d9c9-313e-49be-b9d7-f6bd48abb18c"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Library",
                keyColumn: "Id",
                keyValue: -1,
                column: "Guid",
                value: new Guid("78b49af6-e872-488c-8d69-3a8046e5a53c"));
        }
    }
}

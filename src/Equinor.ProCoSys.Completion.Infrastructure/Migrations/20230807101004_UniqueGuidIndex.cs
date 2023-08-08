using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Equinor.ProCoSys.Completion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UniqueGuidIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PunchItems_Guid",
                table: "PunchItems");

            migrationBuilder.DropIndex(
                name: "IX_Projects_Name_ASC",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_Plant_ASC",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_LibraryItems_Guid",
                table: "Library");

            migrationBuilder.CreateIndex(
                name: "IX_PunchItems_Guid",
                table: "PunchItems",
                column: "Guid",
                unique: true)
                .Annotation("SqlServer:Include", new[] { "Id", "Description", "ProjectId", "CreatedById", "CreatedAtUtc", "ModifiedById", "ModifiedAtUtc", "ClearedById", "ClearedAtUtc", "VerifiedById", "VerifiedAtUtc", "RejectedById", "RejectedAtUtc", "RaisedByOrgId", "ClearingByOrgId", "SortingId", "TypeId", "PriorityId", "RowVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Guid",
                table: "Projects",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Links_Guid",
                table: "Links",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LibraryItems_Guid",
                table: "Library",
                column: "Guid",
                unique: true)
                .Annotation("SqlServer:Include", new[] { "Code", "Description", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_Guid",
                table: "Comments",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_Guid",
                table: "Attachments",
                column: "Guid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PunchItems_Guid",
                table: "PunchItems");

            migrationBuilder.DropIndex(
                name: "IX_Projects_Guid",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Links_Guid",
                table: "Links");

            migrationBuilder.DropIndex(
                name: "IX_LibraryItems_Guid",
                table: "Library");

            migrationBuilder.DropIndex(
                name: "IX_Comments_Guid",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Attachments_Guid",
                table: "Attachments");

            migrationBuilder.CreateIndex(
                name: "IX_PunchItems_Guid",
                table: "PunchItems",
                column: "Guid")
                .Annotation("SqlServer:Include", new[] { "Id", "Description", "ProjectId", "CreatedById", "CreatedAtUtc", "ModifiedById", "ModifiedAtUtc", "ClearedById", "ClearedAtUtc", "VerifiedById", "VerifiedAtUtc", "RejectedById", "RejectedAtUtc", "RaisedByOrgId", "ClearingByOrgId", "SortingId", "TypeId", "PriorityId", "RowVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Name_ASC",
                table: "Projects",
                column: "Name")
                .Annotation("SqlServer:Include", new[] { "Plant" });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Plant_ASC",
                table: "Projects",
                column: "Plant")
                .Annotation("SqlServer:Include", new[] { "Name", "IsClosed", "CreatedAtUtc", "ModifiedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_LibraryItems_Guid",
                table: "Library",
                column: "Guid")
                .Annotation("SqlServer:Include", new[] { "Code", "Description", "Type" });
        }
    }
}

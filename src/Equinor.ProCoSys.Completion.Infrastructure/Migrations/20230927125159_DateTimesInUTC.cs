using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Equinor.ProCoSys.Completion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DateTimesInUTC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PunchItems_Guid",
                table: "PunchItems");

            migrationBuilder.RenameColumn(
                name: "MaterialETA",
                table: "PunchItems",
                newName: "MaterialETAUtc")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PunchItemsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.RenameColumn(
                name: "DueDate",
                table: "PunchItems",
                newName: "DueTimeUtc")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PunchItemsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.CreateIndex(
                name: "IX_PunchItems_Guid",
                table: "PunchItems",
                column: "Guid",
                unique: true)
                .Annotation("SqlServer:Include", new[] { "Id", "Category", "Description", "ProjectId", "CreatedById", "CreatedAtUtc", "ModifiedById", "ModifiedAtUtc", "ClearedById", "ClearedAtUtc", "VerifiedById", "VerifiedAtUtc", "RejectedById", "RejectedAtUtc", "RaisedByOrgId", "ClearingByOrgId", "SortingId", "TypeId", "PriorityId", "Estimate", "DueTimeUtc", "ExternalItemNo", "MaterialRequired", "MaterialExternalNo", "MaterialETAUtc", "WorkOrderId", "OriginalWorkOrderId", "DocumentId", "SWCRId", "ActionById", "RowVersion" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PunchItems_Guid",
                table: "PunchItems");

            migrationBuilder.RenameColumn(
                name: "MaterialETAUtc",
                table: "PunchItems",
                newName: "MaterialETA")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PunchItemsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.RenameColumn(
                name: "DueTimeUtc",
                table: "PunchItems",
                newName: "DueDate")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PunchItemsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.CreateIndex(
                name: "IX_PunchItems_Guid",
                table: "PunchItems",
                column: "Guid",
                unique: true)
                .Annotation("SqlServer:Include", new[] { "Id", "Category", "Description", "ProjectId", "CreatedById", "CreatedAtUtc", "ModifiedById", "ModifiedAtUtc", "ClearedById", "ClearedAtUtc", "VerifiedById", "VerifiedAtUtc", "RejectedById", "RejectedAtUtc", "RaisedByOrgId", "ClearingByOrgId", "SortingId", "TypeId", "PriorityId", "Estimate", "DueDate", "ExternalItemNo", "MaterialRequired", "MaterialExternalNo", "MaterialETA", "WorkOrderId", "OriginalWorkOrderId", "DocumentId", "SWCRId", "ActionById", "RowVersion" });
        }
    }
}

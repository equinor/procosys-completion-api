using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Equinor.ProCoSys.Completion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PunchItemLibraryRefs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PunchItems_Guid",
                table: "PunchItems");

            migrationBuilder.AddColumn<int>(
                name: "ClearingByOrgId",
                table: "PunchItems",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PunchItemsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AddColumn<int>(
                name: "PriorityId",
                table: "PunchItems",
                type: "int",
                nullable: true)
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PunchItemsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AddColumn<int>(
                name: "RaisedByOrgId",
                table: "PunchItems",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PunchItemsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AddColumn<int>(
                name: "SortingId",
                table: "PunchItems",
                type: "int",
                nullable: true)
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PunchItemsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AddColumn<int>(
                name: "TypeId",
                table: "PunchItems",
                type: "int",
                nullable: true)
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PunchItemsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.CreateIndex(
                name: "IX_PunchItems_ClearingByOrgId",
                table: "PunchItems",
                column: "ClearingByOrgId");

            migrationBuilder.CreateIndex(
                name: "IX_PunchItems_Guid",
                table: "PunchItems",
                column: "Guid")
                .Annotation("SqlServer:Include", new[] { "Id", "Description", "ProjectId", "CreatedById", "CreatedAtUtc", "ModifiedById", "ModifiedAtUtc", "ClearedById", "ClearedAtUtc", "VerifiedById", "VerifiedAtUtc", "RejectedById", "RejectedAtUtc", "RaisedByOrgId", "ClearingByOrgId", "SortingId", "TypeId", "PriorityId", "RowVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_PunchItems_PriorityId",
                table: "PunchItems",
                column: "PriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_PunchItems_RaisedByOrgId",
                table: "PunchItems",
                column: "RaisedByOrgId");

            migrationBuilder.CreateIndex(
                name: "IX_PunchItems_SortingId",
                table: "PunchItems",
                column: "SortingId");

            migrationBuilder.CreateIndex(
                name: "IX_PunchItems_TypeId",
                table: "PunchItems",
                column: "TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PunchItems_Library_ClearingByOrgId",
                table: "PunchItems",
                column: "ClearingByOrgId",
                principalTable: "Library",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PunchItems_Library_PriorityId",
                table: "PunchItems",
                column: "PriorityId",
                principalTable: "Library",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PunchItems_Library_RaisedByOrgId",
                table: "PunchItems",
                column: "RaisedByOrgId",
                principalTable: "Library",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PunchItems_Library_SortingId",
                table: "PunchItems",
                column: "SortingId",
                principalTable: "Library",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PunchItems_Library_TypeId",
                table: "PunchItems",
                column: "TypeId",
                principalTable: "Library",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PunchItems_Library_ClearingByOrgId",
                table: "PunchItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PunchItems_Library_PriorityId",
                table: "PunchItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PunchItems_Library_RaisedByOrgId",
                table: "PunchItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PunchItems_Library_SortingId",
                table: "PunchItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PunchItems_Library_TypeId",
                table: "PunchItems");

            migrationBuilder.DropIndex(
                name: "IX_PunchItems_ClearingByOrgId",
                table: "PunchItems");

            migrationBuilder.DropIndex(
                name: "IX_PunchItems_Guid",
                table: "PunchItems");

            migrationBuilder.DropIndex(
                name: "IX_PunchItems_PriorityId",
                table: "PunchItems");

            migrationBuilder.DropIndex(
                name: "IX_PunchItems_RaisedByOrgId",
                table: "PunchItems");

            migrationBuilder.DropIndex(
                name: "IX_PunchItems_SortingId",
                table: "PunchItems");

            migrationBuilder.DropIndex(
                name: "IX_PunchItems_TypeId",
                table: "PunchItems");

            migrationBuilder.DropColumn(
                name: "ClearingByOrgId",
                table: "PunchItems")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PunchItemsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.DropColumn(
                name: "PriorityId",
                table: "PunchItems")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PunchItemsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.DropColumn(
                name: "RaisedByOrgId",
                table: "PunchItems")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PunchItemsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.DropColumn(
                name: "SortingId",
                table: "PunchItems")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PunchItemsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.DropColumn(
                name: "TypeId",
                table: "PunchItems")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PunchItemsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.CreateIndex(
                name: "IX_PunchItems_Guid",
                table: "PunchItems",
                column: "Guid")
                .Annotation("SqlServer:Include", new[] { "Id", "Description", "ProjectId", "CreatedById", "CreatedAtUtc", "ModifiedById", "ModifiedAtUtc", "ClearedById", "ClearedAtUtc", "VerifiedById", "VerifiedAtUtc", "RejectedById", "RejectedAtUtc", "RowVersion" });
        }
    }
}

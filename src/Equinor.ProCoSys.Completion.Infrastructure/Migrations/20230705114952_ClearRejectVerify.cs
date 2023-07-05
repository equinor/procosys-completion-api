using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Equinor.ProCoSys.Completion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ClearRejectVerify : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PunchItems_Guid",
                table: "PunchItems");

            migrationBuilder.AddColumn<DateTime>(
                name: "ClearedAtUtc",
                table: "PunchItems",
                type: "datetime2",
                nullable: true)
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PunchItemsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AddColumn<int>(
                name: "ClearedById",
                table: "PunchItems",
                type: "int",
                nullable: true)
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PunchItemsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAtUtc",
                table: "PunchItems",
                type: "datetime2",
                nullable: true)
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PunchItemsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AddColumn<int>(
                name: "RejectedById",
                table: "PunchItems",
                type: "int",
                nullable: true)
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PunchItemsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifiedAtUtc",
                table: "PunchItems",
                type: "datetime2",
                nullable: true)
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PunchItemsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AddColumn<int>(
                name: "VerifiedById",
                table: "PunchItems",
                type: "int",
                nullable: true)
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PunchItemsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.CreateIndex(
                name: "IX_PunchItems_ClearedById",
                table: "PunchItems",
                column: "ClearedById");

            migrationBuilder.CreateIndex(
                name: "IX_PunchItems_Guid",
                table: "PunchItems",
                column: "Guid")
                .Annotation("SqlServer:Include", new[] { "ItemNo", "Description", "ProjectId", "CreatedById", "CreatedAtUtc", "ModifiedById", "ModifiedAtUtc", "ClearedById", "ClearedAtUtc", "VerifiedById", "VerifiedAtUtc", "RejectedById", "RejectedAtUtc", "RowVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_PunchItems_RejectedById",
                table: "PunchItems",
                column: "RejectedById");

            migrationBuilder.CreateIndex(
                name: "IX_PunchItems_VerifiedById",
                table: "PunchItems",
                column: "VerifiedById");

            migrationBuilder.AddCheckConstraint(
                name: "punch_item_check_cleared",
                table: "PunchItems",
                sql: "(ClearedAtUtc is null and ClearedById is null) or (ClearedAtUtc is not null and ClearedById is not null)");

            migrationBuilder.AddCheckConstraint(
                name: "punch_item_check_cleared_rejected",
                table: "PunchItems",
                sql: "not (ClearedAtUtc is not null and RejectedAtUtc is not null)");

            migrationBuilder.AddCheckConstraint(
                name: "punch_item_check_cleared_verified",
                table: "PunchItems",
                sql: "not (ClearedAtUtc is null and VerifiedAtUtc is not null)");

            migrationBuilder.AddCheckConstraint(
                name: "punch_item_check_rejected",
                table: "PunchItems",
                sql: "(RejectedAtUtc is null and RejectedById is null) or (RejectedAtUtc is not null and RejectedById is not null)");

            migrationBuilder.AddCheckConstraint(
                name: "punch_item_check_verified",
                table: "PunchItems",
                sql: "(VerifiedAtUtc is null and VerifiedById is null) or (VerifiedAtUtc is not null and VerifiedById is not null)");

            migrationBuilder.AddForeignKey(
                name: "FK_PunchItems_Persons_ClearedById",
                table: "PunchItems",
                column: "ClearedById",
                principalTable: "Persons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PunchItems_Persons_RejectedById",
                table: "PunchItems",
                column: "RejectedById",
                principalTable: "Persons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PunchItems_Persons_VerifiedById",
                table: "PunchItems",
                column: "VerifiedById",
                principalTable: "Persons",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PunchItems_Persons_ClearedById",
                table: "PunchItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PunchItems_Persons_RejectedById",
                table: "PunchItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PunchItems_Persons_VerifiedById",
                table: "PunchItems");

            migrationBuilder.DropIndex(
                name: "IX_PunchItems_ClearedById",
                table: "PunchItems");

            migrationBuilder.DropIndex(
                name: "IX_PunchItems_Guid",
                table: "PunchItems");

            migrationBuilder.DropIndex(
                name: "IX_PunchItems_RejectedById",
                table: "PunchItems");

            migrationBuilder.DropIndex(
                name: "IX_PunchItems_VerifiedById",
                table: "PunchItems");

            migrationBuilder.DropCheckConstraint(
                name: "punch_item_check_cleared",
                table: "PunchItems");

            migrationBuilder.DropCheckConstraint(
                name: "punch_item_check_cleared_rejected",
                table: "PunchItems");

            migrationBuilder.DropCheckConstraint(
                name: "punch_item_check_cleared_verified",
                table: "PunchItems");

            migrationBuilder.DropCheckConstraint(
                name: "punch_item_check_rejected",
                table: "PunchItems");

            migrationBuilder.DropCheckConstraint(
                name: "punch_item_check_verified",
                table: "PunchItems");

            migrationBuilder.DropColumn(
                name: "ClearedAtUtc",
                table: "PunchItems")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PunchItemsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.DropColumn(
                name: "ClearedById",
                table: "PunchItems")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PunchItemsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.DropColumn(
                name: "RejectedAtUtc",
                table: "PunchItems")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PunchItemsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.DropColumn(
                name: "RejectedById",
                table: "PunchItems")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PunchItemsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.DropColumn(
                name: "VerifiedAtUtc",
                table: "PunchItems")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PunchItemsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.DropColumn(
                name: "VerifiedById",
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
                .Annotation("SqlServer:Include", new[] { "ItemNo", "Description", "ProjectId", "CreatedById", "CreatedAtUtc", "ModifiedById", "ModifiedAtUtc", "RowVersion" });
        }
    }
}

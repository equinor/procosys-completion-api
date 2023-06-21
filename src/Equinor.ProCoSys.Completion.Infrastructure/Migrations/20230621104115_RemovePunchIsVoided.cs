using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Equinor.ProCoSys.Completion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovePunchIsVoided : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Punches_Guid",
                table: "Punches");

            migrationBuilder.DropIndex(
                name: "IX_Punches_ProjectId",
                table: "Punches");

            migrationBuilder.DropColumn(
                name: "IsVoided",
                table: "Punches")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PunchesHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.CreateIndex(
                name: "IX_Punches_Guid",
                table: "Punches",
                column: "Guid")
                .Annotation("SqlServer:Include", new[] { "ItemNo", "Description", "ProjectId", "CreatedById", "CreatedAtUtc", "ModifiedById", "ModifiedAtUtc", "RowVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_Punches_ProjectId",
                table: "Punches",
                column: "ProjectId")
                .Annotation("SqlServer:Include", new[] { "ItemNo", "RowVersion" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Punches_Guid",
                table: "Punches");

            migrationBuilder.DropIndex(
                name: "IX_Punches_ProjectId",
                table: "Punches");

            migrationBuilder.AddColumn<bool>(
                name: "IsVoided",
                table: "Punches",
                type: "bit",
                nullable: false,
                defaultValue: false)
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PunchesHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.CreateIndex(
                name: "IX_Punches_Guid",
                table: "Punches",
                column: "Guid")
                .Annotation("SqlServer:Include", new[] { "ItemNo", "Description", "ProjectId", "CreatedById", "CreatedAtUtc", "ModifiedById", "ModifiedAtUtc", "IsVoided", "RowVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_Punches_ProjectId",
                table: "Punches",
                column: "ProjectId")
                .Annotation("SqlServer:Include", new[] { "ItemNo", "IsVoided", "RowVersion" });
        }
    }
}

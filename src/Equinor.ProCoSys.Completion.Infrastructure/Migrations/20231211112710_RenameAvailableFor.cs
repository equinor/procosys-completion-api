using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Equinor.ProCoSys.Completion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameAvailableFor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabelLabelEntity_LabelEntities_EntitiesWithLabelId",
                table: "LabelLabelEntity");

            migrationBuilder.DropCheckConstraint(
                name: "valid_entity_type",
                table: "LabelEntities");

            migrationBuilder.RenameColumn(
                name: "EntitiesWithLabelId",
                table: "LabelLabelEntity",
                newName: "AvailableForId")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "LabelLabelEntityHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.RenameColumn(
                name: "EntityWithLabel",
                table: "LabelEntities",
                newName: "EntityType")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "LabelEntitiesHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.RenameIndex(
                name: "IX_LabelEntities_EntityWithLabel",
                table: "LabelEntities",
                newName: "IX_LabelEntities_EntityType");

            migrationBuilder.AddCheckConstraint(
                name: "valid_entity_type",
                table: "LabelEntities",
                sql: "EntityType in ('PunchPicture','PunchComment')");

            migrationBuilder.AddForeignKey(
                name: "FK_LabelLabelEntity_LabelEntities_AvailableForId",
                table: "LabelLabelEntity",
                column: "AvailableForId",
                principalTable: "LabelEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabelLabelEntity_LabelEntities_AvailableForId",
                table: "LabelLabelEntity");

            migrationBuilder.DropCheckConstraint(
                name: "valid_entity_type",
                table: "LabelEntities");

            migrationBuilder.RenameColumn(
                name: "AvailableForId",
                table: "LabelLabelEntity",
                newName: "EntitiesWithLabelId")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "LabelLabelEntityHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.RenameColumn(
                name: "EntityType",
                table: "LabelEntities",
                newName: "EntityWithLabel")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "LabelEntitiesHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.RenameIndex(
                name: "IX_LabelEntities_EntityType",
                table: "LabelEntities",
                newName: "IX_LabelEntities_EntityWithLabel");

            migrationBuilder.AddCheckConstraint(
                name: "valid_entity_type",
                table: "LabelEntities",
                sql: "EntityWithLabel in ('PunchPicture','PunchComment')");

            migrationBuilder.AddForeignKey(
                name: "FK_LabelLabelEntity_LabelEntities_EntitiesWithLabelId",
                table: "LabelLabelEntity",
                column: "EntitiesWithLabelId",
                principalTable: "LabelEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

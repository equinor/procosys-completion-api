using System;
using Equinor.ProCoSys.Completion.Domain;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Equinor.ProCoSys.Completion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InsertMailTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MailTemplates_Persons_CreatedById",
                table: "MailTemplates");

            migrationBuilder.DropIndex(
                name: "IX_MailTemplates_CreatedById",
                table: "MailTemplates");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "MailTemplates")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "MailTemplatesHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "MailTemplates")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "MailTemplatesHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.Sql($"delete from MailTemplates where Code in ('{MailTemplateCode.PunchCommented}','{MailTemplateCode.PunchRejected}')");

            var subject = "Punch {{Entity.ItemNo}} commented";
            var body = "Punch {{Entity.ItemNo}} commented by {{Comment.CreatedBy.FirstName}} {{Comment.CreatedBy.LastName}}.<br>  Comment: {{Comment.Text}}<br>  Url: {{Url}}";
            InsertSql(migrationBuilder, MailTemplateCode.PunchCommented, subject, body);

            subject = "Punch {{Entity.ItemNo}} rejected";
            body = "Punch {{Entity.ItemNo}} rejected by {{Entity.RejectedBy.FirstName}} {{Entity.RejectedBy.LastName}}.<br>  Reject reason: {{Comment}}<br>  Link: {{Url}}";
            InsertSql(migrationBuilder, MailTemplateCode.PunchRejected, subject, body);
        }

        private void InsertSql(MigrationBuilder migrationBuilder, string code, string subject, string body)
        {
            migrationBuilder.Sql(
                @$"INSERT INTO [dbo].[MailTemplates]
                       ([Code]
                       ,[Subject]
                       ,[Body]
                       ,[IsVoided])
                 VALUES
                       ('{code}'
                       ,'{subject}'
                       ,'{body}'
                       ,0)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "MailTemplates",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified))
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "MailTemplatesHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AddColumn<int>(
                name: "CreatedById",
                table: "MailTemplates",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "MailTemplatesHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.CreateIndex(
                name: "IX_MailTemplates_CreatedById",
                table: "MailTemplates",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_MailTemplates_Persons_CreatedById",
                table: "MailTemplates",
                column: "CreatedById",
                principalTable: "Persons",
                principalColumn: "Id");
        }
    }
}

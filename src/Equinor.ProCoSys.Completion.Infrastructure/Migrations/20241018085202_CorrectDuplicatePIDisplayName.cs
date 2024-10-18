using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Equinor.ProCoSys.Completion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CorrectDuplicatePIDisplayName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- Create a temporary table to hold old values from EventDisplayName
                DECLARE @TempUpdates TABLE (
                    HistoryId INT,
                    EventDisplayName NVARCHAR(MAX),
                    Value1 NVARCHAR(MAX),
                    Value2 NVARCHAR(MAX),
                    PropValue NVARCHAR(MAX)
                );

                -- Conditional insertions into the temporary table
                INSERT INTO @TempUpdates (HistoryId, EventDisplayName, Value1, Value2, PropValue)
                SELECT
                    H.id,
                    H.EventDisplayName,
                    TRIM(SUBSTRING(H.EventDisplayName, 14, 8)),
                    TRIM(SUBSTRING(H.EventDisplayName, CHARINDEX(' duplicated from ', H.EventDisplayName) + 17, LEN(H.EventDisplayName))),
                    P.Value
                FROM 
                    History H
                INNER JOIN 
                    Properties P ON H.id = P.HistoryItemId
                WHERE 
                    CHARINDEX(' duplicated from ', H.EventDisplayName) > 0 AND
                    P.Name = 'ItemNo';

                -- Update History table
                UPDATE H
                SET 
                    H.EventDisplayName = STUFF(H.EventDisplayName, CHARINDEX(' duplicated from ', H.EventDisplayName) + 17, LEN(H.EventDisplayName), T.PropValue)
                FROM 
                    History H
                INNER JOIN 
                    @TempUpdates T ON H.id = T.HistoryId
                WHERE 
                    CHARINDEX(' duplicated from ', H.EventDisplayName) > 0 AND t.Value1 = t.Value2;

                -- Update Properties table
                UPDATE P
                SET 
                    P.Value = T.Value1
                FROM 
                    Properties P
                INNER JOIN 
                    @TempUpdates T ON P.HistoryItemId = T.HistoryId
                WHERE
                    P.Name = 'ItemNo' AND t.Value1 = t.Value2;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoopstationCompanionApi.Migrations
{
    /// <inheritdoc />
    public partial class ChangePayloadJsonToJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE presets
                ALTER COLUMN payload_json TYPE json
                USING payload_json::json;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE presets
                ALTER COLUMN payload_json TYPE jsonb
                USING payload_json::jsonb;
            ");
        }
    }
}

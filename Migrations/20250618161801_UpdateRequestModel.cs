using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArqanumServer.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRequestModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "PayloadSignature",
                table: "Requests",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayloadSignature",
                table: "Requests");
        }
    }
}

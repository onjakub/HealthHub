using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthHub.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientForeignKeyToDiagnosticResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PatientId",
                table: "DiagnosticResults",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_DiagnosticResults_PatientId",
                table: "DiagnosticResults",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_DiagnosticResults_Patients_PatientId",
                table: "DiagnosticResults",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiagnosticResults_Patients_PatientId",
                table: "DiagnosticResults");

            migrationBuilder.DropIndex(
                name: "IX_DiagnosticResults_PatientId",
                table: "DiagnosticResults");

            migrationBuilder.DropColumn(
                name: "PatientId",
                table: "DiagnosticResults");
        }
    }
}
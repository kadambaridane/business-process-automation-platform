using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessProcessAutomation.Api.Migrations
{
    /// <inheritdoc />
    public partial class WorkflowStepOrderIndexNotUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkflowSteps_WorkflowDefinitionId_StepOrder",
                table: "WorkflowSteps");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowSteps_WorkflowDefinitionId_StepOrder",
                table: "WorkflowSteps",
                columns: new[] { "WorkflowDefinitionId", "StepOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkflowSteps_WorkflowDefinitionId_StepOrder",
                table: "WorkflowSteps");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowSteps_WorkflowDefinitionId_StepOrder",
                table: "WorkflowSteps",
                columns: new[] { "WorkflowDefinitionId", "StepOrder" },
                unique: true);
        }
    }
}

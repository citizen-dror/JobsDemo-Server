using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobsServer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedWorker",
                table: "Jobs",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedTime",
                table: "Jobs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "Jobs",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobData",
                table: "Jobs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobType",
                table: "Jobs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MaxRetries",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledTime",
                table: "Jobs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "JobExecutionLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LogLevel = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkerId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobExecutionLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkerNode",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    LastHeartbeat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CurrentJobId = table.Column<int>(type: "int", nullable: false),
                    ConcurrencyLimit = table.Column<int>(type: "int", nullable: false),
                    ActiveJobCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkerNode", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_AssignedWorker",
                table: "Jobs",
                column: "AssignedWorker");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Priority",
                table: "Jobs",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_ScheduledTime",
                table: "Jobs",
                column: "ScheduledTime");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Status",
                table: "Jobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_JobExecutionLog_JobId",
                table: "JobExecutionLog",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_JobExecutionLog_Timestamp",
                table: "JobExecutionLog",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_WorkerNode_LastHeartbeat",
                table: "WorkerNode",
                column: "LastHeartbeat");

            migrationBuilder.CreateIndex(
                name: "IX_WorkerNode_Status",
                table: "WorkerNode",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobExecutionLog");

            migrationBuilder.DropTable(
                name: "WorkerNode");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_AssignedWorker",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_Priority",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_ScheduledTime",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_Status",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "AssignedWorker",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "CreatedTime",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "JobData",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "JobType",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "MaxRetries",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "ScheduledTime",
                table: "Jobs");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <summary>
    /// Migration: Add Integration Layer Performance Indexes
    /// Purpose: Improve query performance for sync scheduling, event processing, and health monitoring
    /// Created: 2026-01-10
    /// </summary>
    public partial class AddIntegrationIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ══════════════════════════════════════════════════════════════
            // SYNC JOB TABLE - Scheduler needs fast lookup of due jobs
            // ══════════════════════════════════════════════════════════════
            migrationBuilder.CreateIndex(
                name: "IX_SyncJobs_NextRunAt",
                table: "SyncJobs",
                column: "NextRunAt",
                filter: "\"IsDeleted\" = false AND \"IsActive\" = true AND \"NextRunAt\" IS NOT NULL");

            // ══════════════════════════════════════════════════════════════
            // DOMAIN EVENTS TABLE - Event publisher needs pending events
            // ══════════════════════════════════════════════════════════════
            migrationBuilder.CreateIndex(
                name: "IX_DomainEvents_Status_OccurredAt",
                table: "DomainEvents",
                columns: new[] { "Status", "OccurredAt" },
                descending: new[] { false, true },
                filter: "\"IsDeleted\" = false AND \"Status\" IN ('Pending', 'Published')");

            // ══════════════════════════════════════════════════════════════
            // EVENT DELIVERY LOG TABLE - Event dispatcher needs pending deliveries
            // ══════════════════════════════════════════════════════════════
            migrationBuilder.CreateIndex(
                name: "IX_EventDeliveryLog_Status_NextRetryAt",
                table: "EventDeliveryLog",
                columns: new[] { "Status", "NextRetryAt" },
                filter: "\"IsDeleted\" = false AND \"Status\" = 'Failed' AND \"NextRetryAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EventDeliveryLog_Status_AttemptedAt",
                table: "EventDeliveryLog",
                columns: new[] { "Status", "AttemptedAt" },
                descending: new[] { false, true },
                filter: "\"IsDeleted\" = false AND \"Status\" = 'Pending'");

            // ══════════════════════════════════════════════════════════════
            // INTEGRATION HEALTH METRIC TABLE - Health monitoring queries
            // ══════════════════════════════════════════════════════════════
            migrationBuilder.CreateIndex(
                name: "IX_IntegrationHealthMetric_ConnectorId_RecordedAt",
                table: "IntegrationHealthMetric",
                columns: new[] { "ConnectorId", "RecordedAt" },
                descending: new[] { false, true },
                filter: "\"IsDeleted\" = false");

            // ══════════════════════════════════════════════════════════════
            // SYNC EXECUTION LOG TABLE - Health analysis needs recent logs
            // ══════════════════════════════════════════════════════════════
            migrationBuilder.CreateIndex(
                name: "IX_SyncExecutionLog_SyncJobId_StartedAt",
                table: "SyncExecutionLog",
                columns: new[] { "SyncJobId", "StartedAt" },
                descending: new[] { false, true },
                filter: "\"IsDeleted\" = false");

            // ══════════════════════════════════════════════════════════════
            // CROSS REFERENCE MAPPING TABLE - ID lookup optimization
            // ══════════════════════════════════════════════════════════════
            migrationBuilder.CreateIndex(
                name: "IX_CrossReferenceMapping_ObjectType_InternalId",
                table: "CrossReferenceMapping",
                columns: new[] { "ObjectType", "InternalId" },
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_CrossReferenceMapping_ExternalSystemCode_ExternalId",
                table: "CrossReferenceMapping",
                columns: new[] { "ExternalSystemCode", "ExternalId" },
                filter: "\"IsDeleted\" = false");

            // ══════════════════════════════════════════════════════════════
            // DEAD LETTER ENTRY TABLE - Pending entries for retry UI
            // ══════════════════════════════════════════════════════════════
            migrationBuilder.CreateIndex(
                name: "IX_DeadLetterEntry_Status_FailedAt",
                table: "DeadLetterEntry",
                columns: new[] { "Status", "FailedAt" },
                descending: new[] { false, true },
                filter: "\"IsDeleted\" = false AND \"Status\" = 'Pending'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SyncJobs_NextRunAt",
                table: "SyncJobs");

            migrationBuilder.DropIndex(
                name: "IX_DomainEvents_Status_OccurredAt",
                table: "DomainEvents");

            migrationBuilder.DropIndex(
                name: "IX_EventDeliveryLog_Status_NextRetryAt",
                table: "EventDeliveryLog");

            migrationBuilder.DropIndex(
                name: "IX_EventDeliveryLog_Status_AttemptedAt",
                table: "EventDeliveryLog");

            migrationBuilder.DropIndex(
                name: "IX_IntegrationHealthMetric_ConnectorId_RecordedAt",
                table: "IntegrationHealthMetric");

            migrationBuilder.DropIndex(
                name: "IX_SyncExecutionLog_SyncJobId_StartedAt",
                table: "SyncExecutionLog");

            migrationBuilder.DropIndex(
                name: "IX_CrossReferenceMapping_ObjectType_InternalId",
                table: "CrossReferenceMapping");

            migrationBuilder.DropIndex(
                name: "IX_CrossReferenceMapping_ExternalSystemCode_ExternalId",
                table: "CrossReferenceMapping");

            migrationBuilder.DropIndex(
                name: "IX_DeadLetterEntry_Status_FailedAt",
                table: "DeadLetterEntry");
        }
    }
}

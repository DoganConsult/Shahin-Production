using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class ShahinAIPlatform : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Metadata",
                table: "WorkflowTasks",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ToUserName",
                table: "TaskDelegations",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ToType",
                table: "TaskDelegations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ToAgentTypesJson",
                table: "TaskDelegations",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ToAgentType",
                table: "TaskDelegations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SelectedAgentType",
                table: "TaskDelegations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "TaskDelegations",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FromUserName",
                table: "TaskDelegations",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FromType",
                table: "TaskDelegations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "FromAgentType",
                table: "TaskDelegations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DelegationStrategy",
                table: "TaskDelegations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "TaskDelegations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "AgentDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AgentType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CapabilitiesJson = table.Column<string>(type: "text", nullable: true),
                    DataSourcesJson = table.Column<string>(type: "text", nullable: true),
                    AllowedActionsJson = table.Column<string>(type: "text", nullable: true),
                    ApprovalRequiredActionsJson = table.Column<string>(type: "text", nullable: true),
                    AutoApprovalConfidenceThreshold = table.Column<int>(type: "integer", nullable: false),
                    OversightRoleCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EscalationRoleCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ActivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AgentSoDRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Action1 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Action1AgentTypes = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Action2 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Action2AgentTypes = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    RiskDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Enforcement = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentSoDRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicabilityRuleCatalogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RuleCategory = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ConditionExpression = table.Column<string>(type: "text", nullable: false),
                    ActionType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ActionTarget = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ActionParametersJson = table.Column<string>(type: "text", nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    IsBlocking = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicabilityRuleCatalogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentScopes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScopeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ScopeName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ScopeType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ScopeDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Jurisdictions = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BusinessLines = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SystemsInScope = table.Column<string>(type: "text", nullable: true),
                    DataTypes = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    HostingModels = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ThirdPartiesInScope = table.Column<string>(type: "text", nullable: true),
                    OutOfScopeDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ApprovedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentScopes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentScopes_Assessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "Assessments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssessmentScopes_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BaselineControlSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BaselineCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    BaselineType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaselineControlSets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ControlChangeHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangeType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PreviousValueJson = table.Column<string>(type: "text", nullable: true),
                    NewValueJson = table.Column<string>(type: "text", nullable: true),
                    ChangeReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ChangeRequestId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ChangedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApprovalStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ApprovedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControlChangeHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ControlDomains",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DomainCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DescriptionAr = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControlDomains", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CrossReferenceMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InternalId = table.Column<Guid>(type: "uuid", nullable: false),
                    InternalCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExternalSystemCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ExternalUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LastSyncAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SyncStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LastSyncError = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrossReferenceMappings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CryptographicAssets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    AssetType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SystemName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CurrentAlgorithm = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    KeySizeBits = table.Column<int>(type: "integer", nullable: true),
                    IsQuantumVulnerable = table.Column<bool>(type: "boolean", nullable: false),
                    PQCMigrationStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TargetPQCAlgorithm = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MigrationPriority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OwnerId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CryptographicAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CryptographicAssets_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DomainEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SchemaVersion = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    SourceSystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ObjectType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ObjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ProcessingAttempts = table.Column<int>(type: "integer", nullable: false),
                    LastError = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TriggeredBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ERPSystemConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SystemCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ERPType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Environment = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ConnectionMethod = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ConnectionConfigJson = table.Column<string>(type: "text", nullable: true),
                    ServiceAccountId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsReadOnlyReplica = table.Column<bool>(type: "boolean", nullable: false),
                    ConnectionStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LastHealthCheck = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AvailableModules = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ERPSystemConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ERPSystemConfigs_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventSchemaRegistries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SchemaVersion = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    JsonSchema = table.Column<string>(type: "text", nullable: false),
                    ExamplePayloadJson = table.Column<string>(type: "text", nullable: true),
                    RequiredFields = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeprecatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventSchemaRegistries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubscriptionCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    EventTypePattern = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SubscriberSystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DeliveryMethod = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DeliveryEndpoint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RetryPolicy = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MaxRetries = table.Column<int>(type: "integer", nullable: false),
                    FilterExpression = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EvidencePackFamilies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FamilyCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IconClass = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvidencePackFamilies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EvidencePacks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PackCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    EvidenceItemsJson = table.Column<string>(type: "text", nullable: true),
                    RequiredFrequency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RetentionMonths = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvidencePacks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EvidenceSourceIntegrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ConnectionStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ConnectionConfigJson = table.Column<string>(type: "text", nullable: true),
                    LastSyncDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SyncFrequency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EvidenceTypesProvided = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ControlsCoveredJson = table.Column<string>(type: "text", nullable: true),
                    KRIsFed = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvidenceSourceIntegrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvidenceSourceIntegrations_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GovernanceCadences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CadenceCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CadenceType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Frequency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: true),
                    DayOfMonth = table.Column<int>(type: "integer", nullable: true),
                    WeekOfMonth = table.Column<int>(type: "integer", nullable: true),
                    TimeOfDay = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    Timezone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OwnerRoleCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ParticipantRoleCodes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ActivitiesJson = table.Column<string>(type: "text", nullable: true),
                    DeliverablesJson = table.Column<string>(type: "text", nullable: true),
                    TeamsChannelId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ReminderHoursBefore = table.Column<int>(type: "integer", nullable: false),
                    LastExecutionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextScheduledDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GovernanceCadences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GovernanceCadences_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GovernanceRhythmTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    RhythmItemsJson = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GovernanceRhythmTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HumanRetainedResponsibilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponsibilityCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RoleCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NonDelegableReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RegulatoryReference = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AgentSupportDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HumanRetainedResponsibilities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImportantBusinessServices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CriticalityTier = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    RTO_Hours = table.Column<int>(type: "integer", nullable: false),
                    RPO_Hours = table.Column<int>(type: "integer", nullable: false),
                    MTD_Hours = table.Column<int>(type: "integer", nullable: false),
                    ServiceOwnerId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ServiceOwnerName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SupportingSystemsJson = table.Column<string>(type: "text", nullable: true),
                    Dependencies = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LastDRTestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastDRTestResult = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportantBusinessServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportantBusinessServices_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IntegrationConnectors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConnectorCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ConnectorType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TargetSystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConnectionConfigJson = table.Column<string>(type: "text", nullable: true),
                    AuthType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ConnectionStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LastHealthCheck = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSuccessfulSync = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorCount = table.Column<int>(type: "integer", nullable: false),
                    SupportedOperationsJson = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationConnectors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LegalDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    TitleAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ContentEn = table.Column<string>(type: "text", nullable: false),
                    ContentAr = table.Column<string>(type: "text", nullable: true),
                    Summary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false),
                    ContentHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MappingWorkflowTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    StepsJson = table.Column<string>(type: "text", nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MappingWorkflowTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OnePageGuides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    GuideCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    TitleAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TargetAudience = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    WhatIsInScope = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    HowToDecideApplicability = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    WhereToStoreEvidence = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    WhoApprovesExceptions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    HowAuditsAreServed = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    QuickLinksJson = table.Column<string>(type: "text", nullable: true),
                    ContactInfo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnePageGuides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnePageGuides_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OverlayCatalogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OverlayCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OverlayType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AppliesTo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OverlayCatalogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegulatoryRequirements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequirementCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RegulatorCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FrameworkCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FrameworkVersion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Section = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RequirementText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    RequirementTextAr = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RequirementType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Jurisdictions = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Industries = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DataTypes = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegulatoryRequirements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleTransitionPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CurrentRoleName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CurrentAutomationPercent = table.Column<int>(type: "integer", nullable: false),
                    TargetAutomationPercent = table.Column<int>(type: "integer", nullable: false),
                    TasksToAutomateJson = table.Column<string>(type: "text", nullable: true),
                    TasksToRetainJson = table.Column<string>(type: "text", nullable: true),
                    AssignedAgentCodes = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    NewHumanRole = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Phase = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TargetCompletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RiskMitigationNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleTransitionPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShahinAIBrandConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    BrandCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BrandName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    BrandNameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Tagline = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TaglineAr = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    OperatingSentence = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    OperatingSentenceAr = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PrimaryColor = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    SecondaryColor = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    AccentColor = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LogoDarkUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FaviconUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PublicWebsiteUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    AppUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SupportEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DefaultLanguage = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    RTLEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShahinAIBrandConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShahinAIBrandConfigs_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ShahinAIModules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ShortDescription = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ShortDescriptionAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FullDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FullDescriptionAr = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Outcome = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OutcomeAr = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IconClass = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ModuleColor = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    RoutePath = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresLicense = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShahinAIModules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SiteMapEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PageCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SiteType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    TitleEn = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    TitleAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    UrlPath = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ParentPageCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MetaDescriptionEn = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MetaDescriptionAr = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ShowInNav = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresAuth = table.Column<bool>(type: "boolean", nullable: false),
                    IconClass = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteMapEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StrategicRoadmapMilestones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MilestoneCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CapabilityArea = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Phase = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TargetDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CompletionPercent = table.Column<int>(type: "integer", nullable: false),
                    OwnerId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Dependencies = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SuccessCriteria = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StrategicRoadmapMilestones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StrategicRoadmapMilestones_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupportConversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    SessionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Subject = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsAgentHandled = table.Column<bool>(type: "boolean", nullable: false),
                    AssignedAgentId = table.Column<string>(type: "text", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SatisfactionRating = table.Column<int>(type: "integer", nullable: true),
                    Feedback = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportConversations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemOfRecordDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SystemCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SystemName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsAuthoritative = table.Column<bool>(type: "boolean", nullable: false),
                    AllowExternalCreate = table.Column<bool>(type: "boolean", nullable: false),
                    AllowExternalUpdate = table.Column<bool>(type: "boolean", nullable: false),
                    ApiEndpoint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemOfRecordDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeamsNotificationConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConfigCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NotificationType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    WebhookUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ChannelId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TriggerConditionsJson = table.Column<string>(type: "text", nullable: true),
                    MessageTemplateJson = table.Column<string>(type: "text", nullable: true),
                    UseAdaptiveCard = table.Column<bool>(type: "boolean", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamsNotificationConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamsNotificationConfigs_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestProcedures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TestCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TestType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TestStepsJson = table.Column<string>(type: "text", nullable: true),
                    ExpectedResults = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SampleSizeGuidance = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Frequency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestProcedures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ThirdPartyConcentrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    VendorName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    VendorType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ServicesProvidedJson = table.Column<string>(type: "text", nullable: true),
                    CriticalityTier = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Substitutability = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ConcentrationRiskScore = table.Column<int>(type: "integer", nullable: false),
                    HasTestedExitPlan = table.Column<bool>(type: "boolean", nullable: false),
                    ExitPlanLastTested = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExitTimeMonths = table.Column<int>(type: "integer", nullable: true),
                    HasContinuousAssurance = table.Column<bool>(type: "boolean", nullable: false),
                    HasEvidenceAPI = table.Column<bool>(type: "boolean", nullable: false),
                    ContractEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OwnerId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThirdPartyConcentrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThirdPartyConcentrations_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UITextEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TextKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TextEn = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    TextAr = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    UsageNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UITextEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UniversalEvidencePacks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PackCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ControlFamily = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IconClass = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EvidenceItemsJson = table.Column<string>(type: "text", nullable: true),
                    NamingStandard = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    StorageLocationPattern = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    MinimalTestStepsJson = table.Column<string>(type: "text", nullable: true),
                    SatisfiesFrameworks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UniversalEvidencePacks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserConsents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ConsentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DocumentVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsGranted = table.Column<bool>(type: "boolean", nullable: false),
                    ConsentedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    WithdrawnAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WithdrawalReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DocumentHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserConsents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserConsents_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserConsents_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserWorkspaces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RoleName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    RoleNameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    WorkspaceConfigJson = table.Column<string>(type: "text", nullable: true),
                    AssignedFrameworks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AssignedAssessmentIds = table.Column<string>(type: "text", nullable: true),
                    DefaultLandingPage = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    QuickActionsJson = table.Column<string>(type: "text", nullable: true),
                    DashboardWidgetsJson = table.Column<string>(type: "text", nullable: true),
                    IsConfigured = table.Column<bool>(type: "boolean", nullable: false),
                    LastAccessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWorkspaces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserWorkspaces_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserWorkspaces_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkspaceTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DefaultLandingPage = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DashboardWidgetsJson = table.Column<string>(type: "text", nullable: true),
                    QuickActionsJson = table.Column<string>(type: "text", nullable: true),
                    MenuItemsJson = table.Column<string>(type: "text", nullable: true),
                    AssignableTaskTypes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkspaceTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AgentActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionCorrelationId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ActionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ActionDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TargetObjectType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TargetObjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    InputDataJson = table.Column<string>(type: "text", nullable: true),
                    OutputDataJson = table.Column<string>(type: "text", nullable: true),
                    ConfidenceScore = table.Column<int>(type: "integer", nullable: true),
                    Reasoning = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RequiredApproval = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovalGateId = table.Column<Guid>(type: "uuid", nullable: true),
                    WasApproved = table.Column<bool>(type: "boolean", nullable: true),
                    ApprovedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovalNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DurationMs = table.Column<int>(type: "integer", nullable: true),
                    TriggeredByActionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentActions_AgentDefinitions_AgentId",
                        column: x => x.AgentId,
                        principalTable: "AgentDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AgentApprovalGates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentId = table.Column<Guid>(type: "uuid", nullable: false),
                    GateCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TriggerActionTypes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TriggerConditionJson = table.Column<string>(type: "text", nullable: true),
                    ApproverRoleCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ApprovalSLAHours = table.Column<int>(type: "integer", nullable: false),
                    EscalationRoleCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AutoRejectHours = table.Column<int>(type: "integer", nullable: false),
                    BypassConfidenceThreshold = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentApprovalGates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentApprovalGates_AgentDefinitions_AgentId",
                        column: x => x.AgentId,
                        principalTable: "AgentDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AgentCapabilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CapabilityCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RiskLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RequiresApproval = table.Column<bool>(type: "boolean", nullable: false),
                    MaxUsesPerHour = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentCapabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentCapabilities_AgentDefinitions_AgentId",
                        column: x => x.AgentId,
                        principalTable: "AgentDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GeneratedControlSuites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SuiteCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    BaselineSetId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppliedOverlaysJson = table.Column<string>(type: "text", nullable: true),
                    TotalControls = table.Column<int>(type: "integer", nullable: false),
                    MandatoryControls = table.Column<int>(type: "integer", nullable: false),
                    OptionalControls = table.Column<int>(type: "integer", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GeneratedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ProfileSnapshotJson = table.Column<string>(type: "text", nullable: true),
                    RulesExecutionLogJson = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneratedControlSuites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeneratedControlSuites_BaselineControlSets_BaselineSetId",
                        column: x => x.BaselineSetId,
                        principalTable: "BaselineControlSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GeneratedControlSuites_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MAPFrameworkConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConfigCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Tagline = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    BaselineSetId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActiveOverlays = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    GovernanceRhythmJson = table.Column<string>(type: "text", nullable: true),
                    EvidenceNamingStandard = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ActivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MAPFrameworkConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MAPFrameworkConfigs_BaselineControlSets_BaselineSetId",
                        column: x => x.BaselineSetId,
                        principalTable: "BaselineControlSets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MAPFrameworkConfigs_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ControlObjectives",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectiveCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DomainId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectiveStatement = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ObjectiveStatementAr = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControlObjectives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ControlObjectives_ControlDomains_DomainId",
                        column: x => x.DomainId,
                        principalTable: "ControlDomains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ERPExtractConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ERPSystemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExtractCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ProcessArea = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DataSource = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    QueryExpression = table.Column<string>(type: "text", nullable: true),
                    FieldMappingsJson = table.Column<string>(type: "text", nullable: true),
                    Frequency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CronExpression = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastExtractAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastExtractRecordCount = table.Column<int>(type: "integer", nullable: true),
                    NextExtractAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ERPExtractConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ERPExtractConfigs_ERPSystemConfigs_ERPSystemId",
                        column: x => x.ERPSystemId,
                        principalTable: "ERPSystemConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SoDRuleDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ProcessArea = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RiskLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Function1 = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Function1Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Function1AccessPatternsJson = table.Column<string>(type: "text", nullable: true),
                    Function2 = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Function2Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Function2AccessPatternsJson = table.Column<string>(type: "text", nullable: true),
                    BusinessRiskDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    MitigatingControls = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ERPSystemId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoDRuleDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SoDRuleDefinitions_ERPSystemConfigs_ERPSystemId",
                        column: x => x.ERPSystemId,
                        principalTable: "ERPSystemConfigs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SoDRuleDefinitions_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventDeliveryLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AttemptNumber = table.Column<int>(type: "integer", nullable: false),
                    HttpStatusCode = table.Column<int>(type: "integer", nullable: true),
                    ResponseBody = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    LatencyMs = table.Column<int>(type: "integer", nullable: true),
                    AttemptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NextRetryAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventDeliveryLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventDeliveryLogs_DomainEvents_EventId",
                        column: x => x.EventId,
                        principalTable: "DomainEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventDeliveryLogs_EventSubscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "EventSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StandardEvidenceItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    EvidenceType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RequiredFrequency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false),
                    SampleFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CollectionGuidance = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StandardEvidenceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StandardEvidenceItems_EvidencePackFamilies_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "EvidencePackFamilies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CadenceExecutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CadenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExecutedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AttendeesJson = table.Column<string>(type: "text", nullable: true),
                    ActivitiesCompletedJson = table.Column<string>(type: "text", nullable: true),
                    DeliverablesProducedJson = table.Column<string>(type: "text", nullable: true),
                    ActionItemsJson = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    ReminderSent = table.Column<bool>(type: "boolean", nullable: false),
                    ReminderSentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CadenceExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CadenceExecutions_GovernanceCadences_CadenceId",
                        column: x => x.CadenceId,
                        principalTable: "GovernanceCadences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GovernanceRhythmItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Frequency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    OwnerRoleCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ParticipantRoleCodes = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ActivitiesJson = table.Column<string>(type: "text", nullable: true),
                    DeliverablesJson = table.Column<string>(type: "text", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GovernanceRhythmItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GovernanceRhythmItems_GovernanceRhythmTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "GovernanceRhythmTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IntegrationHealthMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConnectorId = table.Column<Guid>(type: "uuid", nullable: false),
                    MetricType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Value = table.Column<decimal>(type: "numeric", nullable: false),
                    Unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AlertThreshold = table.Column<decimal>(type: "numeric", nullable: true),
                    IsBreaching = table.Column<bool>(type: "boolean", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationHealthMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntegrationHealthMetrics_IntegrationConnectors_ConnectorId",
                        column: x => x.ConnectorId,
                        principalTable: "IntegrationConnectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SyncJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConnectorId = table.Column<Guid>(type: "uuid", nullable: false),
                    JobCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Direction = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ObjectType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Frequency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CronExpression = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FieldMappingJson = table.Column<string>(type: "text", nullable: true),
                    FilterExpression = table.Column<string>(type: "text", nullable: true),
                    UseUpsert = table.Column<bool>(type: "boolean", nullable: false),
                    LastRunAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastRunStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LastRunRecordCount = table.Column<int>(type: "integer", nullable: true),
                    NextRunAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SyncJobs_IntegrationConnectors_ConnectorId",
                        column: x => x.ConnectorId,
                        principalTable: "IntegrationConnectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupportMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SenderId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Content = table.Column<string>(type: "text", nullable: false),
                    MessageType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MetadataJson = table.Column<string>(type: "text", nullable: true),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportMessages_SupportConversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "SupportConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UniversalEvidencePackItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PackId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    EvidenceType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Frequency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false),
                    CollectionGuidance = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SampleFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    RetentionMonths = table.Column<int>(type: "integer", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UniversalEvidencePackItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UniversalEvidencePackItems_UniversalEvidencePacks_PackId",
                        column: x => x.PackId,
                        principalTable: "UniversalEvidencePacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserWorkspaceTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    TitleAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TaskType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RelatedEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    RelatedEntityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ActionUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FrameworkCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EstimatedHours = table.Column<int>(type: "integer", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWorkspaceTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserWorkspaceTasks_UserWorkspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "UserWorkspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AgentConfidenceScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionId = table.Column<Guid>(type: "uuid", nullable: false),
                    OverallScore = table.Column<int>(type: "integer", nullable: false),
                    ConfidenceLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ScoreBreakdownJson = table.Column<string>(type: "text", nullable: true),
                    LowConfidenceFactorsJson = table.Column<string>(type: "text", nullable: true),
                    RecommendedAction = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    HumanReviewTriggered = table.Column<bool>(type: "boolean", nullable: false),
                    HumanReviewOutcome = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    HumanReviewerFeedback = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ScoredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentConfidenceScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentConfidenceScores_AgentActions_ActionId",
                        column: x => x.ActionId,
                        principalTable: "AgentActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AgentSoDViolations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action1Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Action2Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    WasBlocked = table.Column<bool>(type: "boolean", nullable: false),
                    OverrideApprovedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OverrideReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DetectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentSoDViolations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentSoDViolations_AgentActions_Action1Id",
                        column: x => x.Action1Id,
                        principalTable: "AgentActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AgentSoDViolations_AgentSoDRules_RuleId",
                        column: x => x.RuleId,
                        principalTable: "AgentSoDRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PendingApprovals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovalGateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AssignedApproverId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AssignedApproverName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DueAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReminderSent = table.Column<bool>(type: "boolean", nullable: false),
                    IsEscalated = table.Column<bool>(type: "boolean", nullable: false),
                    EscalatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Decision = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DecisionNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DecidedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PendingApprovals_AgentActions_ActionId",
                        column: x => x.ActionId,
                        principalTable: "AgentActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PendingApprovals_AgentApprovalGates_ApprovalGateId",
                        column: x => x.ApprovalGateId,
                        principalTable: "AgentApprovalGates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    EntityType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ParentEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    Sectors = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Jurisdictions = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DataTypes = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TechnologyProfile = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CriticalityTier = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    InheritsFromParent = table.Column<bool>(type: "boolean", nullable: false),
                    AppliedOverlays = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    GeneratedSuiteId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationEntities_GeneratedControlSuites_GeneratedSuiteId",
                        column: x => x.GeneratedSuiteId,
                        principalTable: "GeneratedControlSuites",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrganizationEntities_OrganizationEntities_ParentEntityId",
                        column: x => x.ParentEntityId,
                        principalTable: "OrganizationEntities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrganizationEntities_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CanonicalControls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ControlId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ObjectiveId = table.Column<Guid>(type: "uuid", nullable: false),
                    ControlName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ControlNameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ControlStatement = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ControlStatementAr = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ControlType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ControlNature = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Frequency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RiskRating = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ImplementationGuidance = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SunsetDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ApplicabilityJson = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CanonicalControls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CanonicalControls_ControlObjectives_ObjectiveId",
                        column: x => x.ObjectiveId,
                        principalTable: "ControlObjectives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ERPExtractExecutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExtractConfigId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RecordsExtracted = table.Column<int>(type: "integer", nullable: false),
                    RecordsPassedToCCM = table.Column<int>(type: "integer", nullable: false),
                    StorageLocation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FileHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ERPExtractExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ERPExtractExecutions_ERPExtractConfigs_ExtractConfigId",
                        column: x => x.ExtractConfigId,
                        principalTable: "ERPExtractConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SoDConflicts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConflictCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UserName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UserDepartment = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Function1AccessJson = table.Column<string>(type: "text", nullable: true),
                    Function2AccessJson = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    HasMitigatingControl = table.Column<bool>(type: "boolean", nullable: false),
                    MitigatingControlDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RiskAcceptanceOwnerId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RiskAcceptanceOwnerName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AcceptanceExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ITSMTicketId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DetectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoDConflicts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SoDConflicts_SoDRuleDefinitions_RuleId",
                        column: x => x.RuleId,
                        principalTable: "SoDRuleDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SoDConflicts_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeadLetterEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: true),
                    SyncJobId = table.Column<Guid>(type: "uuid", nullable: true),
                    EntryType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OriginalPayloadJson = table.Column<string>(type: "text", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    StackTrace = table.Column<string>(type: "text", nullable: true),
                    FailureCount = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ResolutionNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ResolvedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastRetryAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeadLetterEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeadLetterEntries_DomainEvents_EventId",
                        column: x => x.EventId,
                        principalTable: "DomainEvents",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeadLetterEntries_SyncJobs_SyncJobId",
                        column: x => x.SyncJobId,
                        principalTable: "SyncJobs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SyncExecutionLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SyncJobId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RecordsProcessed = table.Column<int>(type: "integer", nullable: false),
                    RecordsCreated = table.Column<int>(type: "integer", nullable: false),
                    RecordsUpdated = table.Column<int>(type: "integer", nullable: false),
                    RecordsFailed = table.Column<int>(type: "integer", nullable: false),
                    RecordsSkipped = table.Column<int>(type: "integer", nullable: false),
                    ErrorsJson = table.Column<string>(type: "text", nullable: true),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncExecutionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SyncExecutionLogs_SyncJobs_SyncJobId",
                        column: x => x.SyncJobId,
                        principalTable: "SyncJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicabilityEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ControlId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequirementId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    InheritedFrom = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ExceptionReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ExceptionExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    JurisdictionDriver = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BusinessLineDriver = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SystemTierDriver = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DataTypeDriver = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    HostingModelDriver = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    EvidencePackId = table.Column<Guid>(type: "uuid", nullable: true),
                    ControlOwnerId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ControlOwnerName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ApprovedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicabilityEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicabilityEntries_Assessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "Assessments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicabilityEntries_CanonicalControls_ControlId",
                        column: x => x.ControlId,
                        principalTable: "CanonicalControls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicabilityEntries_EvidencePacks_EvidencePackId",
                        column: x => x.EvidencePackId,
                        principalTable: "EvidencePacks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicabilityEntries_RegulatoryRequirements_RequirementId",
                        column: x => x.RequirementId,
                        principalTable: "RegulatoryRequirements",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicabilityEntries_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicabilityRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    RuleType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Attribute = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Operator = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ControlId = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicabilityRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicabilityRules_CanonicalControls_ControlId",
                        column: x => x.ControlId,
                        principalTable: "CanonicalControls",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BaselineControlMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BaselineSetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ControlId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultParametersJson = table.Column<string>(type: "text", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaselineControlMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaselineControlMappings_BaselineControlSets_BaselineSetId",
                        column: x => x.BaselineSetId,
                        principalTable: "BaselineControlSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaselineControlMappings_CanonicalControls_ControlId",
                        column: x => x.ControlId,
                        principalTable: "CanonicalControls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CapturedEvidences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvidenceCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SourceIntegrationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ControlId = table.Column<Guid>(type: "uuid", nullable: true),
                    PeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EvidenceTypeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CollectionMethod = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StorageLocation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FileHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    PreviousVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    TagsJson = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OwnerId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OwnerName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ReviewerId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RetentionUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CapturedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CapturedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapturedEvidences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CapturedEvidences_CanonicalControls_ControlId",
                        column: x => x.ControlId,
                        principalTable: "CanonicalControls",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CapturedEvidences_EvidenceSourceIntegrations_SourceIntegrat~",
                        column: x => x.SourceIntegrationId,
                        principalTable: "EvidenceSourceIntegrations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CapturedEvidences_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CCMControlTests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TestCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ControlId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProcessArea = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TestCategory = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ERPSystemId = table.Column<Guid>(type: "uuid", nullable: true),
                    PopulationDefinitionJson = table.Column<string>(type: "text", nullable: false),
                    RuleDefinitionJson = table.Column<string>(type: "text", nullable: false),
                    ThresholdSettingsJson = table.Column<string>(type: "text", nullable: true),
                    Frequency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RiskLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ExceptionOwnerRoleCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ExceptionSLADays = table.Column<int>(type: "integer", nullable: false),
                    AutoCreateTicket = table.Column<bool>(type: "boolean", nullable: false),
                    SendTeamsNotification = table.Column<bool>(type: "boolean", nullable: false),
                    LastExecutionAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastPassRate = table.Column<decimal>(type: "numeric", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CCMControlTests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CCMControlTests_CanonicalControls_ControlId",
                        column: x => x.ControlId,
                        principalTable: "CanonicalControls",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CCMControlTests_ERPSystemConfigs_ERPSystemId",
                        column: x => x.ERPSystemId,
                        principalTable: "ERPSystemConfigs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CCMControlTests_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComplianceGuardrails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    GuardrailCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    GuardrailType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EnforcementPoint = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RuleDefinitionJson = table.Column<string>(type: "text", nullable: true),
                    EnforcementMode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ControlId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastEvaluatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastEvaluationResult = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ViolationsCount = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceGuardrails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplianceGuardrails_CanonicalControls_ControlId",
                        column: x => x.ControlId,
                        principalTable: "CanonicalControls",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ComplianceGuardrails_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ControlEvidencePacks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ControlId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvidencePackId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControlEvidencePacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ControlEvidencePacks_CanonicalControls_ControlId",
                        column: x => x.ControlId,
                        principalTable: "CanonicalControls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ControlEvidencePacks_EvidencePacks_EvidencePackId",
                        column: x => x.EvidencePackId,
                        principalTable: "EvidencePacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ControlExceptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExceptionCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ControlId = table.Column<Guid>(type: "uuid", nullable: false),
                    Scope = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    RiskImpact = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CompensatingControls = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RemediationPlan = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TargetRemediationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RiskAcceptanceOwnerId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RiskAcceptanceOwnerName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ApprovedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastReviewDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextReviewDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewFrequency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ExpiryReminderSent = table.Column<bool>(type: "boolean", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RequestedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControlExceptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ControlExceptions_CanonicalControls_ControlId",
                        column: x => x.ControlId,
                        principalTable: "CanonicalControls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ControlExceptions_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ControlTestProcedures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ControlId = table.Column<Guid>(type: "uuid", nullable: false),
                    TestProcedureId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControlTestProcedures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ControlTestProcedures_CanonicalControls_ControlId",
                        column: x => x.ControlId,
                        principalTable: "CanonicalControls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ControlTestProcedures_TestProcedures_TestProcedureId",
                        column: x => x.TestProcedureId,
                        principalTable: "TestProcedures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OverlayControlMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OverlayId = table.Column<Guid>(type: "uuid", nullable: false),
                    ControlId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OverlayControlMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OverlayControlMappings_CanonicalControls_ControlId",
                        column: x => x.ControlId,
                        principalTable: "CanonicalControls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OverlayControlMappings_OverlayCatalogs_OverlayId",
                        column: x => x.OverlayId,
                        principalTable: "OverlayCatalogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OverlayParameterOverrides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OverlayId = table.Column<Guid>(type: "uuid", nullable: false),
                    ControlId = table.Column<Guid>(type: "uuid", nullable: true),
                    ParameterName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OriginalValue = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    OverrideValue = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OverlayParameterOverrides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OverlayParameterOverrides_CanonicalControls_ControlId",
                        column: x => x.ControlId,
                        principalTable: "CanonicalControls",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OverlayParameterOverrides_OverlayCatalogs_OverlayId",
                        column: x => x.OverlayId,
                        principalTable: "OverlayCatalogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlainLanguageControls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ControlId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlainStatement = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PlainStatementAr = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    WhoPerforms = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    HowOften = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    WhatProvesIt = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PassCriteria = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FailCriteria = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FullExample = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlainLanguageControls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlainLanguageControls_CanonicalControls_ControlId",
                        column: x => x.ControlId,
                        principalTable: "CanonicalControls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RequirementMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequirementId = table.Column<Guid>(type: "uuid", nullable: false),
                    ControlId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectiveId = table.Column<Guid>(type: "uuid", nullable: true),
                    MappingType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ConfidenceLevel = table.Column<int>(type: "integer", nullable: false),
                    Rationale = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ApprovedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequirementMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequirementMappings_CanonicalControls_ControlId",
                        column: x => x.ControlId,
                        principalTable: "CanonicalControls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequirementMappings_ControlObjectives_ObjectiveId",
                        column: x => x.ObjectiveId,
                        principalTable: "ControlObjectives",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RequirementMappings_RegulatoryRequirements_RequirementId",
                        column: x => x.RequirementId,
                        principalTable: "RegulatoryRequirements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RiskIndicators",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IndicatorCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IndicatorType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ControlId = table.Column<Guid>(type: "uuid", nullable: true),
                    DataSource = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MeasurementFrequency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TargetValue = table.Column<decimal>(type: "numeric", nullable: true),
                    WarningThreshold = table.Column<decimal>(type: "numeric", nullable: true),
                    CriticalThreshold = table.Column<decimal>(type: "numeric", nullable: true),
                    Direction = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OwnerRoleCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AutoEscalate = table.Column<bool>(type: "boolean", nullable: false),
                    EscalationDays = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskIndicators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiskIndicators_CanonicalControls_ControlId",
                        column: x => x.ControlId,
                        principalTable: "CanonicalControls",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RiskIndicators_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SuiteControlEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SuiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    ControlId = table.Column<Guid>(type: "uuid", nullable: false),
                    Source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SourceOverlayCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false),
                    AppliedParametersJson = table.Column<string>(type: "text", nullable: true),
                    InclusionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AssignedOwnerRoleCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuiteControlEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SuiteControlEntries_CanonicalControls_ControlId",
                        column: x => x.ControlId,
                        principalTable: "CanonicalControls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SuiteControlEntries_GeneratedControlSuites_SuiteId",
                        column: x => x.SuiteId,
                        principalTable: "GeneratedControlSuites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SuiteEvidenceRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SuiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    ControlId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvidencePackId = table.Column<Guid>(type: "uuid", nullable: true),
                    EvidenceItemCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EvidenceItemName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    RequiredFrequency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RetentionMonths = table.Column<int>(type: "integer", nullable: false),
                    AssignedOwnerId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AssignedOwnerName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuiteEvidenceRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SuiteEvidenceRequests_CanonicalControls_ControlId",
                        column: x => x.ControlId,
                        principalTable: "CanonicalControls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SuiteEvidenceRequests_EvidencePacks_EvidencePackId",
                        column: x => x.EvidencePackId,
                        principalTable: "EvidencePacks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SuiteEvidenceRequests_GeneratedControlSuites_SuiteId",
                        column: x => x.SuiteId,
                        principalTable: "GeneratedControlSuites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CCMTestExecutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PopulationCount = table.Column<int>(type: "integer", nullable: false),
                    PassedCount = table.Column<int>(type: "integer", nullable: false),
                    FailedCount = table.Column<int>(type: "integer", nullable: false),
                    PassRate = table.Column<decimal>(type: "numeric", nullable: false),
                    ResultStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EvidenceSnapshotLocation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EvidenceSnapshotHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CCMTestExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CCMTestExecutions_CCMControlTests_TestId",
                        column: x => x.TestId,
                        principalTable: "CCMControlTests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MappingQualityGates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MappingId = table.Column<Guid>(type: "uuid", nullable: false),
                    CoverageStatement = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    HasCoverageStatement = table.Column<bool>(type: "boolean", nullable: false),
                    HasEvidenceLinkage = table.Column<bool>(type: "boolean", nullable: false),
                    EvidenceLinkageNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TestMethod = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    HasTestMethod = table.Column<bool>(type: "boolean", nullable: false),
                    GapStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RemediationRequired = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ConfidenceRating = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    QualityScore = table.Column<int>(type: "integer", nullable: false),
                    PassedQualityGate = table.Column<bool>(type: "boolean", nullable: false),
                    ReviewedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MappingQualityGates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MappingQualityGates_RequirementMappings_MappingId",
                        column: x => x.MappingId,
                        principalTable: "RequirementMappings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MappingWorkflowSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MappingId = table.Column<Guid>(type: "uuid", nullable: false),
                    StepNumber = table.Column<int>(type: "integer", nullable: false),
                    RoleCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RaciType = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    StepName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StepDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AssignedToUserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AssignedToUserName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Decision = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Comments = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MappingWorkflowSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MappingWorkflowSteps_RequirementMappings_MappingId",
                        column: x => x.MappingId,
                        principalTable: "RequirementMappings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RiskIndicatorMeasurements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IndicatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Value = table.Column<decimal>(type: "numeric", nullable: false),
                    Target = table.Column<decimal>(type: "numeric", nullable: true),
                    Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Trend = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RawDataJson = table.Column<string>(type: "text", nullable: true),
                    Commentary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    MeasuredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MeasuredBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskIndicatorMeasurements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiskIndicatorMeasurements_RiskIndicators_IndicatorId",
                        column: x => x.IndicatorId,
                        principalTable: "RiskIndicators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AutoTaggedEvidences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ControlId = table.Column<Guid>(type: "uuid", nullable: false),
                    Process = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    System = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Period = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OwnerId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OwnerName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    EvidenceType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    StorageLocation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    MimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    Source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CCMTestExecutionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ReviewedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RetentionUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CapturedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CapturedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutoTaggedEvidences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutoTaggedEvidences_CCMTestExecutions_CCMTestExecutionId",
                        column: x => x.CCMTestExecutionId,
                        principalTable: "CCMTestExecutions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AutoTaggedEvidences_CanonicalControls_ControlId",
                        column: x => x.ControlId,
                        principalTable: "CanonicalControls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AutoTaggedEvidences_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CCMExceptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TestExecutionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExceptionCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ExceptionType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Summary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Details = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AffectedEntity = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AffectedEntityId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TransactionReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AmountInvolved = table.Column<decimal>(type: "numeric", nullable: true),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    RawDataJson = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AssignedToId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AssignedToName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ITSMTicketId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ITSMTicketUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TeamsNotificationSent = table.Column<bool>(type: "boolean", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolutionNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ResolvedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DetectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CCMExceptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CCMExceptions_CCMTestExecutions_TestExecutionId",
                        column: x => x.TestExecutionId,
                        principalTable: "CCMTestExecutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RiskIndicatorAlerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IndicatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    MeasurementId = table.Column<Guid>(type: "uuid", nullable: true),
                    Severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ThresholdValue = table.Column<decimal>(type: "numeric", nullable: true),
                    ActualValue = table.Column<decimal>(type: "numeric", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DaysInBreach = table.Column<int>(type: "integer", nullable: false),
                    IsEscalated = table.Column<bool>(type: "boolean", nullable: false),
                    EscalatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EscalatedTo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AssignedTo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AcknowledgedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AcknowledgedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ResolutionNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TriggeredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskIndicatorAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiskIndicatorAlerts_RiskIndicatorMeasurements_MeasurementId",
                        column: x => x.MeasurementId,
                        principalTable: "RiskIndicatorMeasurements",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RiskIndicatorAlerts_RiskIndicators_IndicatorId",
                        column: x => x.IndicatorId,
                        principalTable: "RiskIndicators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskDelegations_DelegatedAt",
                table: "TaskDelegations",
                column: "DelegatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TaskDelegations_TenantId_IsActive_IsRevoked",
                table: "TaskDelegations",
                columns: new[] { "TenantId", "IsActive", "IsRevoked" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskDelegations_TenantId_TaskId",
                table: "TaskDelegations",
                columns: new[] { "TenantId", "TaskId" });

            migrationBuilder.CreateIndex(
                name: "IX_AgentActions_AgentId",
                table: "AgentActions",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentApprovalGates_AgentId",
                table: "AgentApprovalGates",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentCapabilities_AgentId",
                table: "AgentCapabilities",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentConfidenceScores_ActionId",
                table: "AgentConfidenceScores",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentSoDViolations_Action1Id",
                table: "AgentSoDViolations",
                column: "Action1Id");

            migrationBuilder.CreateIndex(
                name: "IX_AgentSoDViolations_RuleId",
                table: "AgentSoDViolations",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicabilityEntries_AssessmentId",
                table: "ApplicabilityEntries",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicabilityEntries_ControlId",
                table: "ApplicabilityEntries",
                column: "ControlId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicabilityEntries_EvidencePackId",
                table: "ApplicabilityEntries",
                column: "EvidencePackId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicabilityEntries_RequirementId",
                table: "ApplicabilityEntries",
                column: "RequirementId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicabilityEntries_TenantId",
                table: "ApplicabilityEntries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicabilityRules_ControlId",
                table: "ApplicabilityRules",
                column: "ControlId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentScopes_AssessmentId",
                table: "AssessmentScopes",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentScopes_TenantId",
                table: "AssessmentScopes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AutoTaggedEvidences_CCMTestExecutionId",
                table: "AutoTaggedEvidences",
                column: "CCMTestExecutionId");

            migrationBuilder.CreateIndex(
                name: "IX_AutoTaggedEvidences_ControlId",
                table: "AutoTaggedEvidences",
                column: "ControlId");

            migrationBuilder.CreateIndex(
                name: "IX_AutoTaggedEvidences_TenantId",
                table: "AutoTaggedEvidences",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_BaselineControlMappings_BaselineSetId",
                table: "BaselineControlMappings",
                column: "BaselineSetId");

            migrationBuilder.CreateIndex(
                name: "IX_BaselineControlMappings_ControlId",
                table: "BaselineControlMappings",
                column: "ControlId");

            migrationBuilder.CreateIndex(
                name: "IX_CadenceExecutions_CadenceId",
                table: "CadenceExecutions",
                column: "CadenceId");

            migrationBuilder.CreateIndex(
                name: "IX_CanonicalControls_ObjectiveId",
                table: "CanonicalControls",
                column: "ObjectiveId");

            migrationBuilder.CreateIndex(
                name: "IX_CapturedEvidences_ControlId",
                table: "CapturedEvidences",
                column: "ControlId");

            migrationBuilder.CreateIndex(
                name: "IX_CapturedEvidences_SourceIntegrationId",
                table: "CapturedEvidences",
                column: "SourceIntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_CapturedEvidences_TenantId",
                table: "CapturedEvidences",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CCMControlTests_ControlId",
                table: "CCMControlTests",
                column: "ControlId");

            migrationBuilder.CreateIndex(
                name: "IX_CCMControlTests_ERPSystemId",
                table: "CCMControlTests",
                column: "ERPSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_CCMControlTests_TenantId",
                table: "CCMControlTests",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CCMExceptions_TestExecutionId",
                table: "CCMExceptions",
                column: "TestExecutionId");

            migrationBuilder.CreateIndex(
                name: "IX_CCMTestExecutions_TestId",
                table: "CCMTestExecutions",
                column: "TestId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceGuardrails_ControlId",
                table: "ComplianceGuardrails",
                column: "ControlId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceGuardrails_TenantId",
                table: "ComplianceGuardrails",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ControlEvidencePacks_ControlId",
                table: "ControlEvidencePacks",
                column: "ControlId");

            migrationBuilder.CreateIndex(
                name: "IX_ControlEvidencePacks_EvidencePackId",
                table: "ControlEvidencePacks",
                column: "EvidencePackId");

            migrationBuilder.CreateIndex(
                name: "IX_ControlExceptions_ControlId",
                table: "ControlExceptions",
                column: "ControlId");

            migrationBuilder.CreateIndex(
                name: "IX_ControlExceptions_TenantId",
                table: "ControlExceptions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ControlObjectives_DomainId",
                table: "ControlObjectives",
                column: "DomainId");

            migrationBuilder.CreateIndex(
                name: "IX_ControlTestProcedures_ControlId",
                table: "ControlTestProcedures",
                column: "ControlId");

            migrationBuilder.CreateIndex(
                name: "IX_ControlTestProcedures_TestProcedureId",
                table: "ControlTestProcedures",
                column: "TestProcedureId");

            migrationBuilder.CreateIndex(
                name: "IX_CryptographicAssets_TenantId",
                table: "CryptographicAssets",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DeadLetterEntries_EventId",
                table: "DeadLetterEntries",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_DeadLetterEntries_SyncJobId",
                table: "DeadLetterEntries",
                column: "SyncJobId");

            migrationBuilder.CreateIndex(
                name: "IX_ERPExtractConfigs_ERPSystemId",
                table: "ERPExtractConfigs",
                column: "ERPSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_ERPExtractExecutions_ExtractConfigId",
                table: "ERPExtractExecutions",
                column: "ExtractConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_ERPSystemConfigs_TenantId",
                table: "ERPSystemConfigs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_EventDeliveryLogs_EventId",
                table: "EventDeliveryLogs",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventDeliveryLogs_SubscriptionId",
                table: "EventDeliveryLogs",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_EvidenceSourceIntegrations_TenantId",
                table: "EvidenceSourceIntegrations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedControlSuites_BaselineSetId",
                table: "GeneratedControlSuites",
                column: "BaselineSetId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedControlSuites_TenantId",
                table: "GeneratedControlSuites",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_GovernanceCadences_TenantId",
                table: "GovernanceCadences",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_GovernanceRhythmItems_TemplateId",
                table: "GovernanceRhythmItems",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportantBusinessServices_TenantId",
                table: "ImportantBusinessServices",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationHealthMetrics_ConnectorId",
                table: "IntegrationHealthMetrics",
                column: "ConnectorId");

            migrationBuilder.CreateIndex(
                name: "IX_MAPFrameworkConfigs_BaselineSetId",
                table: "MAPFrameworkConfigs",
                column: "BaselineSetId");

            migrationBuilder.CreateIndex(
                name: "IX_MAPFrameworkConfigs_TenantId",
                table: "MAPFrameworkConfigs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_MappingQualityGates_MappingId",
                table: "MappingQualityGates",
                column: "MappingId");

            migrationBuilder.CreateIndex(
                name: "IX_MappingWorkflowSteps_MappingId",
                table: "MappingWorkflowSteps",
                column: "MappingId");

            migrationBuilder.CreateIndex(
                name: "IX_OnePageGuides_TenantId",
                table: "OnePageGuides",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationEntities_GeneratedSuiteId",
                table: "OrganizationEntities",
                column: "GeneratedSuiteId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationEntities_ParentEntityId",
                table: "OrganizationEntities",
                column: "ParentEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationEntities_TenantId",
                table: "OrganizationEntities",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_OverlayControlMappings_ControlId",
                table: "OverlayControlMappings",
                column: "ControlId");

            migrationBuilder.CreateIndex(
                name: "IX_OverlayControlMappings_OverlayId",
                table: "OverlayControlMappings",
                column: "OverlayId");

            migrationBuilder.CreateIndex(
                name: "IX_OverlayParameterOverrides_ControlId",
                table: "OverlayParameterOverrides",
                column: "ControlId");

            migrationBuilder.CreateIndex(
                name: "IX_OverlayParameterOverrides_OverlayId",
                table: "OverlayParameterOverrides",
                column: "OverlayId");

            migrationBuilder.CreateIndex(
                name: "IX_PendingApprovals_ActionId",
                table: "PendingApprovals",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_PendingApprovals_ApprovalGateId",
                table: "PendingApprovals",
                column: "ApprovalGateId");

            migrationBuilder.CreateIndex(
                name: "IX_PlainLanguageControls_ControlId",
                table: "PlainLanguageControls",
                column: "ControlId");

            migrationBuilder.CreateIndex(
                name: "IX_RequirementMappings_ControlId",
                table: "RequirementMappings",
                column: "ControlId");

            migrationBuilder.CreateIndex(
                name: "IX_RequirementMappings_ObjectiveId",
                table: "RequirementMappings",
                column: "ObjectiveId");

            migrationBuilder.CreateIndex(
                name: "IX_RequirementMappings_RequirementId",
                table: "RequirementMappings",
                column: "RequirementId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskIndicatorAlerts_IndicatorId",
                table: "RiskIndicatorAlerts",
                column: "IndicatorId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskIndicatorAlerts_MeasurementId",
                table: "RiskIndicatorAlerts",
                column: "MeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskIndicatorMeasurements_IndicatorId",
                table: "RiskIndicatorMeasurements",
                column: "IndicatorId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskIndicators_ControlId",
                table: "RiskIndicators",
                column: "ControlId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskIndicators_TenantId",
                table: "RiskIndicators",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShahinAIBrandConfigs_TenantId",
                table: "ShahinAIBrandConfigs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SoDConflicts_RuleId",
                table: "SoDConflicts",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_SoDConflicts_TenantId",
                table: "SoDConflicts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SoDRuleDefinitions_ERPSystemId",
                table: "SoDRuleDefinitions",
                column: "ERPSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_SoDRuleDefinitions_TenantId",
                table: "SoDRuleDefinitions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_StandardEvidenceItems_FamilyId",
                table: "StandardEvidenceItems",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_StrategicRoadmapMilestones_TenantId",
                table: "StrategicRoadmapMilestones",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SuiteControlEntries_ControlId",
                table: "SuiteControlEntries",
                column: "ControlId");

            migrationBuilder.CreateIndex(
                name: "IX_SuiteControlEntries_SuiteId",
                table: "SuiteControlEntries",
                column: "SuiteId");

            migrationBuilder.CreateIndex(
                name: "IX_SuiteEvidenceRequests_ControlId",
                table: "SuiteEvidenceRequests",
                column: "ControlId");

            migrationBuilder.CreateIndex(
                name: "IX_SuiteEvidenceRequests_EvidencePackId",
                table: "SuiteEvidenceRequests",
                column: "EvidencePackId");

            migrationBuilder.CreateIndex(
                name: "IX_SuiteEvidenceRequests_SuiteId",
                table: "SuiteEvidenceRequests",
                column: "SuiteId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportMessages_ConversationId",
                table: "SupportMessages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncExecutionLogs_SyncJobId",
                table: "SyncExecutionLogs",
                column: "SyncJobId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncJobs_ConnectorId",
                table: "SyncJobs",
                column: "ConnectorId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamsNotificationConfigs_TenantId",
                table: "TeamsNotificationConfigs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ThirdPartyConcentrations_TenantId",
                table: "ThirdPartyConcentrations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UniversalEvidencePackItems_PackId",
                table: "UniversalEvidencePackItems",
                column: "PackId");

            migrationBuilder.CreateIndex(
                name: "IX_UserConsents_TenantId",
                table: "UserConsents",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UserConsents_UserId",
                table: "UserConsents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserWorkspaces_TenantId",
                table: "UserWorkspaces",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UserWorkspaces_UserId",
                table: "UserWorkspaces",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserWorkspaceTasks_WorkspaceId",
                table: "UserWorkspaceTasks",
                column: "WorkspaceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentCapabilities");

            migrationBuilder.DropTable(
                name: "AgentConfidenceScores");

            migrationBuilder.DropTable(
                name: "AgentSoDViolations");

            migrationBuilder.DropTable(
                name: "ApplicabilityEntries");

            migrationBuilder.DropTable(
                name: "ApplicabilityRuleCatalogs");

            migrationBuilder.DropTable(
                name: "ApplicabilityRules");

            migrationBuilder.DropTable(
                name: "AssessmentScopes");

            migrationBuilder.DropTable(
                name: "AutoTaggedEvidences");

            migrationBuilder.DropTable(
                name: "BaselineControlMappings");

            migrationBuilder.DropTable(
                name: "CadenceExecutions");

            migrationBuilder.DropTable(
                name: "CapturedEvidences");

            migrationBuilder.DropTable(
                name: "CCMExceptions");

            migrationBuilder.DropTable(
                name: "ComplianceGuardrails");

            migrationBuilder.DropTable(
                name: "ControlChangeHistories");

            migrationBuilder.DropTable(
                name: "ControlEvidencePacks");

            migrationBuilder.DropTable(
                name: "ControlExceptions");

            migrationBuilder.DropTable(
                name: "ControlTestProcedures");

            migrationBuilder.DropTable(
                name: "CrossReferenceMappings");

            migrationBuilder.DropTable(
                name: "CryptographicAssets");

            migrationBuilder.DropTable(
                name: "DeadLetterEntries");

            migrationBuilder.DropTable(
                name: "ERPExtractExecutions");

            migrationBuilder.DropTable(
                name: "EventDeliveryLogs");

            migrationBuilder.DropTable(
                name: "EventSchemaRegistries");

            migrationBuilder.DropTable(
                name: "GovernanceRhythmItems");

            migrationBuilder.DropTable(
                name: "HumanRetainedResponsibilities");

            migrationBuilder.DropTable(
                name: "ImportantBusinessServices");

            migrationBuilder.DropTable(
                name: "IntegrationHealthMetrics");

            migrationBuilder.DropTable(
                name: "LegalDocuments");

            migrationBuilder.DropTable(
                name: "MAPFrameworkConfigs");

            migrationBuilder.DropTable(
                name: "MappingQualityGates");

            migrationBuilder.DropTable(
                name: "MappingWorkflowSteps");

            migrationBuilder.DropTable(
                name: "MappingWorkflowTemplates");

            migrationBuilder.DropTable(
                name: "OnePageGuides");

            migrationBuilder.DropTable(
                name: "OrganizationEntities");

            migrationBuilder.DropTable(
                name: "OverlayControlMappings");

            migrationBuilder.DropTable(
                name: "OverlayParameterOverrides");

            migrationBuilder.DropTable(
                name: "PendingApprovals");

            migrationBuilder.DropTable(
                name: "PlainLanguageControls");

            migrationBuilder.DropTable(
                name: "RiskIndicatorAlerts");

            migrationBuilder.DropTable(
                name: "RoleTransitionPlans");

            migrationBuilder.DropTable(
                name: "ShahinAIBrandConfigs");

            migrationBuilder.DropTable(
                name: "ShahinAIModules");

            migrationBuilder.DropTable(
                name: "SiteMapEntries");

            migrationBuilder.DropTable(
                name: "SoDConflicts");

            migrationBuilder.DropTable(
                name: "StandardEvidenceItems");

            migrationBuilder.DropTable(
                name: "StrategicRoadmapMilestones");

            migrationBuilder.DropTable(
                name: "SuiteControlEntries");

            migrationBuilder.DropTable(
                name: "SuiteEvidenceRequests");

            migrationBuilder.DropTable(
                name: "SupportMessages");

            migrationBuilder.DropTable(
                name: "SyncExecutionLogs");

            migrationBuilder.DropTable(
                name: "SystemOfRecordDefinitions");

            migrationBuilder.DropTable(
                name: "TeamsNotificationConfigs");

            migrationBuilder.DropTable(
                name: "ThirdPartyConcentrations");

            migrationBuilder.DropTable(
                name: "UITextEntries");

            migrationBuilder.DropTable(
                name: "UniversalEvidencePackItems");

            migrationBuilder.DropTable(
                name: "UserConsents");

            migrationBuilder.DropTable(
                name: "UserWorkspaceTasks");

            migrationBuilder.DropTable(
                name: "WorkspaceTemplates");

            migrationBuilder.DropTable(
                name: "AgentSoDRules");

            migrationBuilder.DropTable(
                name: "GovernanceCadences");

            migrationBuilder.DropTable(
                name: "EvidenceSourceIntegrations");

            migrationBuilder.DropTable(
                name: "CCMTestExecutions");

            migrationBuilder.DropTable(
                name: "TestProcedures");

            migrationBuilder.DropTable(
                name: "ERPExtractConfigs");

            migrationBuilder.DropTable(
                name: "DomainEvents");

            migrationBuilder.DropTable(
                name: "EventSubscriptions");

            migrationBuilder.DropTable(
                name: "GovernanceRhythmTemplates");

            migrationBuilder.DropTable(
                name: "RequirementMappings");

            migrationBuilder.DropTable(
                name: "OverlayCatalogs");

            migrationBuilder.DropTable(
                name: "AgentActions");

            migrationBuilder.DropTable(
                name: "AgentApprovalGates");

            migrationBuilder.DropTable(
                name: "RiskIndicatorMeasurements");

            migrationBuilder.DropTable(
                name: "SoDRuleDefinitions");

            migrationBuilder.DropTable(
                name: "EvidencePackFamilies");

            migrationBuilder.DropTable(
                name: "EvidencePacks");

            migrationBuilder.DropTable(
                name: "GeneratedControlSuites");

            migrationBuilder.DropTable(
                name: "SupportConversations");

            migrationBuilder.DropTable(
                name: "SyncJobs");

            migrationBuilder.DropTable(
                name: "UniversalEvidencePacks");

            migrationBuilder.DropTable(
                name: "UserWorkspaces");

            migrationBuilder.DropTable(
                name: "CCMControlTests");

            migrationBuilder.DropTable(
                name: "RegulatoryRequirements");

            migrationBuilder.DropTable(
                name: "AgentDefinitions");

            migrationBuilder.DropTable(
                name: "RiskIndicators");

            migrationBuilder.DropTable(
                name: "BaselineControlSets");

            migrationBuilder.DropTable(
                name: "IntegrationConnectors");

            migrationBuilder.DropTable(
                name: "ERPSystemConfigs");

            migrationBuilder.DropTable(
                name: "CanonicalControls");

            migrationBuilder.DropTable(
                name: "ControlObjectives");

            migrationBuilder.DropTable(
                name: "ControlDomains");

            migrationBuilder.DropIndex(
                name: "IX_TaskDelegations_DelegatedAt",
                table: "TaskDelegations");

            migrationBuilder.DropIndex(
                name: "IX_TaskDelegations_TenantId_IsActive_IsRevoked",
                table: "TaskDelegations");

            migrationBuilder.DropIndex(
                name: "IX_TaskDelegations_TenantId_TaskId",
                table: "TaskDelegations");

            migrationBuilder.AlterColumn<string>(
                name: "Metadata",
                table: "WorkflowTasks",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ToUserName",
                table: "TaskDelegations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ToType",
                table: "TaskDelegations",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "ToAgentTypesJson",
                table: "TaskDelegations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ToAgentType",
                table: "TaskDelegations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SelectedAgentType",
                table: "TaskDelegations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "TaskDelegations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FromUserName",
                table: "TaskDelegations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FromType",
                table: "TaskDelegations",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "FromAgentType",
                table: "TaskDelegations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DelegationStrategy",
                table: "TaskDelegations",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "TaskDelegations",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);
        }
    }
}

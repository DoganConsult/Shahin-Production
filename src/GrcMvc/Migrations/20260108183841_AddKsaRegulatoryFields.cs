using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddKsaRegulatoryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChamberMembership",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CrExpiryDate",
                table: "OrganizationProfiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataTransferCountries",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GosiNumber",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "HasCrossBorderTransfer",
                table: "OrganizationProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasEInvoicingPhase1",
                table: "OrganizationProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasEInvoicingPhase2",
                table: "OrganizationProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTadawulListed",
                table: "OrganizationProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MisaLicenseNumber",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MisaLicenseType",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MunicipalLicense",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NitaqatCategory",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "ProcessesBiometricData",
                table: "OrganizationProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ProcessesLocationData",
                table: "OrganizationProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ProcessesNationalIdData",
                table: "OrganizationProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresDataLocalization",
                table: "OrganizationProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresEsgReporting",
                table: "OrganizationProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresIfrsCompliance",
                table: "OrganizationProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SasoCertification",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "SaudizationPercent",
                table: "OrganizationProfiles",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Vision2030Kpis",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Vision2030Program",
                table: "OrganizationProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ZakatCertExpiry",
                table: "OrganizationProfiles",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChamberMembership",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "CrExpiryDate",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "DataTransferCountries",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "GosiNumber",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "HasCrossBorderTransfer",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "HasEInvoicingPhase1",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "HasEInvoicingPhase2",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "IsTadawulListed",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "MisaLicenseNumber",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "MisaLicenseType",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "MunicipalLicense",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "NitaqatCategory",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "ProcessesBiometricData",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "ProcessesLocationData",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "ProcessesNationalIdData",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "RequiresDataLocalization",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "RequiresEsgReporting",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "RequiresIfrsCompliance",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "SasoCertification",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "SaudizationPercent",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "Vision2030Kpis",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "Vision2030Program",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "ZakatCertExpiry",
                table: "OrganizationProfiles");
        }
    }
}

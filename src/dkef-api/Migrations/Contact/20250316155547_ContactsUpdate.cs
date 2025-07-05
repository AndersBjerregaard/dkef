using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dkef_api.Migrations
{
    /// <inheritdoc />
    public partial class ContactsUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ZIP",
                table: "Contacts",
                newName: "WorkTasks");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "Contacts",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "CountryCode",
                table: "Contacts",
                newName: "Source");

            migrationBuilder.RenameColumn(
                name: "ContactType",
                table: "Contacts",
                newName: "SecondarySection");

            migrationBuilder.RenameColumn(
                name: "ContactName",
                table: "Contacts",
                newName: "RegistrationDate");

            migrationBuilder.RenameColumn(
                name: "City",
                table: "Contacts",
                newName: "PrivateZIP");

            migrationBuilder.RenameColumn(
                name: "AttributingPerson",
                table: "Contacts",
                newName: "PrivatePhone");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Contacts",
                newName: "PrivateCity");

            migrationBuilder.AddColumn<string>(
                name: "ATTInvoice",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CompanyAddress",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CompanyCity",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CompanyEmail",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CompanyPhone",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CompanyZIP",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Contacts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "ElTeknikDelivery",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExpectedEndDateOfBeingStudent",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HelpToStudents",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Invoice",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InvoiceEmail",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Mentor",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Occupation",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OldMemberNumber",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PrimarySection",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PrivateAddress",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ATTInvoice",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "CompanyAddress",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "CompanyCity",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "CompanyEmail",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "CompanyPhone",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "CompanyZIP",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "ElTeknikDelivery",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "ExpectedEndDateOfBeingStudent",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "HelpToStudents",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "Invoice",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "InvoiceEmail",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "Mentor",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "Occupation",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "OldMemberNumber",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "PrimarySection",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "PrivateAddress",
                table: "Contacts");

            migrationBuilder.RenameColumn(
                name: "WorkTasks",
                table: "Contacts",
                newName: "ZIP");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Contacts",
                newName: "Phone");

            migrationBuilder.RenameColumn(
                name: "Source",
                table: "Contacts",
                newName: "CountryCode");

            migrationBuilder.RenameColumn(
                name: "SecondarySection",
                table: "Contacts",
                newName: "ContactType");

            migrationBuilder.RenameColumn(
                name: "RegistrationDate",
                table: "Contacts",
                newName: "ContactName");

            migrationBuilder.RenameColumn(
                name: "PrivateZIP",
                table: "Contacts",
                newName: "City");

            migrationBuilder.RenameColumn(
                name: "PrivatePhone",
                table: "Contacts",
                newName: "AttributingPerson");

            migrationBuilder.RenameColumn(
                name: "PrivateCity",
                table: "Contacts",
                newName: "Address");
        }
    }
}

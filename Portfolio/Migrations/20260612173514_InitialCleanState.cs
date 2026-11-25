using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.Migrations
{
    /// <inheritdoc />
    public partial class InitialCleanState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* ------------------------------------------------------------------
               БЛОК ЗАКОМЕНТОВАНО: Ці таблиці вже існують на сервері
               ------------------------------------------------------------------
            migrationBuilder.CreateTable(
                name: "AdminUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AvailableSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    IsBooked = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvailableSlots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContactMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TodoItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Deadline = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoItems", x => x.Id);
                });
            ------------------------------------------------------------------ */

            // === ЦЮ ТАБЛИЦЮ ЗАЛИШАЄМО АКТИВНОЮ (ВОНА НОВА ДЛЯ PASSKEYS) ===
            migrationBuilder.CreateTable(
                name: "UserCredentials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    CredentialId = table.Column<byte[]>(type: "BLOB", nullable: false),
                    PublicKey = table.Column<byte[]>(type: "BLOB", nullable: false),
                    UserHandle = table.Column<byte[]>(type: "BLOB", nullable: false),
                    SignatureCounter = table.Column<uint>(type: "INTEGER", nullable: false),
                    CredType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    RegDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCredentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCredentials_AdminUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AdminUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            /* ------------------------------------------------------------------
               БЛОК ЗАКОМЕНТОВАНО: Ця таблиця вже існує
               ------------------------------------------------------------------
            migrationBuilder.CreateTable(
                name: "BookingRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GuestName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    GuestEmail = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    RequestedDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    AvailableSlotId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingRequests_AvailableSlots_AvailableSlotId",
                        column: x => x.AvailableSlotId,
                        principalTable: "AvailableSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_Username",
                table: "AdminUsers",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AvailableSlots_Date_StartTime",
                table: "AvailableSlots",
                columns: new[] { "Date", "StartTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AvailableSlots_IsBooked",
                table: "AvailableSlots",
                column: "IsBooked");

            migrationBuilder.CreateIndex(
                name: "IX_BookingRequests_AvailableSlotId",
                table: "BookingRequests",
                column: "AvailableSlotId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookingRequests_RequestedDateTime",
                table: "BookingRequests",
                column: "RequestedDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_BookingRequests_Status",
                table: "BookingRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ContactMessages_CreatedAt",
                table: "ContactMessages",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_Deadline",
                table: "TodoItems",
                column: "Deadline");

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_StartDate",
                table: "TodoItems",
                column: "StartDate");
            ------------------------------------------------------------------ */

            // === ІНДЕКСИ НОВОЇ ТАБЛИЦІ ЗАЛИШАЄМО АКТИВНИМИ ===
            migrationBuilder.CreateIndex(
                name: "IX_UserCredentials_CredentialId",
                table: "UserCredentials",
                column: "CredentialId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserCredentials_UserId",
                table: "UserCredentials",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /* ------------------------------------------------------------------
               БЛОК ЗАКОМЕНТОВАНО: Щоб випадково не видалити бойові дані при Rollback
               ------------------------------------------------------------------
            migrationBuilder.DropTable(
                name: "BookingRequests");

            migrationBuilder.DropTable(
                name: "ContactMessages");

            migrationBuilder.DropTable(
                name: "TodoItems");
            ------------------------------------------------------------------ */

            // === ПРИ ВІДКАТІ ВИДАЛЯЄМО ТІЛЬКИ НОВУ ТАБЛИЦЮ ===
            migrationBuilder.DropTable(
                name: "UserCredentials");

            /* ------------------------------------------------------------------
            migrationBuilder.DropTable(
                name: "AvailableSlots");

            migrationBuilder.DropTable(
                name: "AdminUsers");
            ------------------------------------------------------------------ */
        }
    }
}
using System;
// 파일 용도: MeetSettle MVP 데이터베이스의 최초 스키마를 생성한다.
// 파일 목적: 공개 repo 사용자가 PostgreSQL 스키마를 EF Core migration으로 재현하게 한다.
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeetSettle.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "meetups",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    location = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meetups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "invite_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    meetup_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invite_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_invite_tokens_meetups_meetup_id",
                        column: x => x.meetup_id,
                        principalTable: "meetups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "participants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    meetup_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    is_attending = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_participants", x => x.id);
                    table.ForeignKey(
                        name: "FK_participants_meetups_meetup_id",
                        column: x => x.meetup_id,
                        principalTable: "meetups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "expenses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    meetup_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payer_participant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expenses", x => x.id);
                    table.CheckConstraint("ck_expenses_amount_non_negative", "amount >= 0");
                    table.ForeignKey(
                        name: "FK_expenses_meetups_meetup_id",
                        column: x => x.meetup_id,
                        principalTable: "meetups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_expenses_participants_payer_participant_id",
                        column: x => x.payer_participant_id,
                        principalTable: "participants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "settlement_transfers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    meetup_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_participant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    to_participant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_settlement_transfers", x => x.id);
                    table.CheckConstraint("ck_settlement_transfers_amount_positive", "amount > 0");
                    table.ForeignKey(
                        name: "FK_settlement_transfers_meetups_meetup_id",
                        column: x => x.meetup_id,
                        principalTable: "meetups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_settlement_transfers_participants_from_participant_id",
                        column: x => x.from_participant_id,
                        principalTable: "participants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_settlement_transfers_participants_to_participant_id",
                        column: x => x.to_participant_id,
                        principalTable: "participants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "expense_shares",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    expense_id = table.Column<Guid>(type: "uuid", nullable: false),
                    participant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    weight = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false, defaultValue: 1m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expense_shares", x => x.id);
                    table.CheckConstraint("ck_expense_shares_weight_positive", "weight > 0");
                    table.ForeignKey(
                        name: "FK_expense_shares_expenses_expense_id",
                        column: x => x.expense_id,
                        principalTable: "expenses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_expense_shares_participants_participant_id",
                        column: x => x.participant_id,
                        principalTable: "participants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_expense_shares_participant_id",
                table: "expense_shares",
                column: "participant_id");

            migrationBuilder.CreateIndex(
                name: "ux_expense_shares_expense_id_participant_id",
                table: "expense_shares",
                columns: new[] { "expense_id", "participant_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_expenses_meetup_id",
                table: "expenses",
                column: "meetup_id");

            migrationBuilder.CreateIndex(
                name: "IX_expenses_payer_participant_id",
                table: "expenses",
                column: "payer_participant_id");

            migrationBuilder.CreateIndex(
                name: "IX_invite_tokens_meetup_id",
                table: "invite_tokens",
                column: "meetup_id");

            migrationBuilder.CreateIndex(
                name: "ux_invite_tokens_token",
                table: "invite_tokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_meetups_date",
                table: "meetups",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "ix_participants_meetup_id_name",
                table: "participants",
                columns: new[] { "meetup_id", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_settlement_transfers_from_participant_id",
                table: "settlement_transfers",
                column: "from_participant_id");

            migrationBuilder.CreateIndex(
                name: "ix_settlement_transfers_meetup_id",
                table: "settlement_transfers",
                column: "meetup_id");

            migrationBuilder.CreateIndex(
                name: "IX_settlement_transfers_to_participant_id",
                table: "settlement_transfers",
                column: "to_participant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "expense_shares");

            migrationBuilder.DropTable(
                name: "invite_tokens");

            migrationBuilder.DropTable(
                name: "settlement_transfers");

            migrationBuilder.DropTable(
                name: "expenses");

            migrationBuilder.DropTable(
                name: "participants");

            migrationBuilder.DropTable(
                name: "meetups");
        }
    }
}

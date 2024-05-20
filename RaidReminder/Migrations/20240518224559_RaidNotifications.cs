using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RaidReminder.Migrations
{
	/// <inheritdoc />
	public partial class RaidNotifications : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "RaidNotifications",
				columns: table => new
				{
					Id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					Day = table.Column<int>(type: "integer", nullable: false),
					Time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
					GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
					ChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
					RoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_RaidNotifications", x => x.Id);
				});
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "RaidNotifications");
		}
	}
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaidReminder.Migrations
{
	/// <inheritdoc />
	public partial class AddInputTimezone : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<int>(
				name: "InputTimeZone",
				table: "RaidNotifications",
				type: "integer",
				nullable: false,
				defaultValue: 0);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "InputTimeZone",
				table: "RaidNotifications");
		}
	}
}

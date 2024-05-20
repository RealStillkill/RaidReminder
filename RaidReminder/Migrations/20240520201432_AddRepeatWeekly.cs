using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaidReminder.Migrations
{
	/// <inheritdoc />
	public partial class AddRepeatWeekly : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<bool>(
				name: "RepeatWeekly",
				table: "RaidNotifications",
				type: "boolean",
				nullable: false,
				defaultValue: false);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "RepeatWeekly",
				table: "RaidNotifications");
		}
	}
}

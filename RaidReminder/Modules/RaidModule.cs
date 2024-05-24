using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using RaidReminder.Data;
using RaidReminder.Data.Models;
using RaidReminder.Services;
using System;

namespace RaidReminder.Modules
{
	[Group("raid", "A set of commands for managing your raid.")]
	internal class RaidModule : InteractionModuleBase<SocketInteractionContext>
	{
		private readonly ILogger<RaidModule> _logger;
		private readonly NotificationService _notificationService;

		public RaidModule(ILogger<RaidModule> logger, NotificationService notificationService)
		{
			_logger = logger;
			_notificationService = notificationService;
		}



		//[SlashCommand("add-reminder", "Creates a reminder that will notify your players")]
		//public async Task AddReminder(DayOfWeek dayOfWeek,
		//	[Summary(description:"What hour of the day should the notification happen? (24hr format, MST unless overridden)")][MaxValue(23)][MinValue(0)]int hour,
		//	[Summary(description:"What what minute of the hour should the notificaiton happen?")][MaxValue(59)][MinValue(0)] int minute,
		//	[Summary(description:"What role should we ping? (Default: true)")]IRole role,
		//	[Summary(description:"Should this reminder be repeated each week?")]bool repeatWeekly = true,
		//	[Summary(description:"Override the input timezone, default MST")] USTimeZones timezone = USTimeZones.Mountain)
		//{
		//	_logger.LogInformation("raid add-reminder executed");
		//	await DeferAsync();
		//	hour = AdjustTimezone(hour, timezone);

		//	TimeOnly time = new TimeOnly(hour, minute);


		//	if ((await _notificationService.GetGuildNotifications(Context.Guild.Id)).Count() == 10)
		//	{
		//		await FollowupAsync("You have hit the limit for raid notifications in this server. Delete an existing notification before making a new one");
		//		return;
		//	}

		//	RaidNotificationModel model = new RaidNotificationModel(Context, role.Id, dayOfWeek, time, repeatWeekly);

		//	NotificationTimer timer = await _notificationService.AddNotificationAsync(model);

		//	TimestampTag tag = new TimestampTag(timer.NextNotificationUTC, TimestampTagStyles.Relative);

		//	EmbedBuilder builder = new EmbedBuilder()
		//		.WithTitle("New raid notification created")
		//		.WithDescription($"Next notification will occur {tag}");
		//	await FollowupAsync(embed: builder.Build());
		//}

		[SlashCommand("add-reminder-utc", "Creates a reminder that will notify your players")]
		public async Task AddReminder(ulong unixTimestamp, IRole role, bool repeatWeekly)
		{
			await DeferAsync();
			_logger.LogInformation("raid add-reminder-utc executed");

			if ((await _notificationService.GetGuildNotifications(Context.Guild.Id)).Count() == 10)
			{
				await FollowupAsync("You have hit the limit for raid notifications in this server. Delete an existing notification before making a new one");
				return;
			}

			DateTime targetDateTime = GetDateTimeFromUnix(unixTimestamp);

			DayOfWeek dayOfWeek = targetDateTime.DayOfWeek;
			TimeOnly time = new TimeOnly(targetDateTime.Hour, targetDateTime.Minute);

			RaidNotificationModel model = new RaidNotificationModel(Context, role.Id, dayOfWeek, time, repeatWeekly);

			NotificationTimer timer = await _notificationService.AddNotificationAsync(model);




			TimestampTag tag = new TimestampTag(timer.NextNotificationUTC, TimestampTagStyles.Relative);
			EmbedBuilder builder = new EmbedBuilder()
				.WithTitle("New raid notification created")
				.WithDescription($"Next notification will occur {tag}");
			await FollowupAsync(embed: builder.Build());

		}


		[SlashCommand("delete-reminder", "Opens a menu to delete a notification")]
		public async Task DeleteMenu()
		{
			try
			{
				await DeferAsync();
				_logger.LogInformation("raid delete-reminder executed");

				SelectMenuBuilder builder = new SelectMenuBuilder();
				builder.WithPlaceholder("Select a notification to delete");
				builder.WithMinValues(1);
				builder.WithMaxValues(1);
				builder.WithType(ComponentType.SelectMenu);
				builder.WithCustomId("delete-reminder-menu");
				IEnumerable<RaidNotificationModel> models = await _notificationService.GetGuildNotifications(Context.Guild.Id);

				if (models.Count() == 0)
				{
					await FollowupAsync("There are no notifications for this server");
					return;
				}

				foreach (RaidNotificationModel model in models)
				{
					DateTime baseTime = _notificationService.GetNextNotificationDate(model.Id);

					builder.AddOption(new SelectMenuOptionBuilder()
						.WithLabel($"{Enum.GetName(model.Day)} {model.Time}")
						.WithValue(model.Id.ToString())
						.WithDescription($"Next notification (Server-Time, CST): {baseTime.ToString("dd MMMM, hh:mm:ss tt")}")
						.WithDefault(false));
				}

				ComponentBuilder cBuilder = new ComponentBuilder();
				cBuilder.AddRow(new ActionRowBuilder()
					.WithSelectMenu(builder));

				await FollowupAsync(components: cBuilder.Build(), ephemeral: true);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while opening the notificaiton delete menu");
				await FollowupAsync("An error has occurred");
			}
		}

		[ComponentInteraction("delete-reminder-menu", true)]
		public async Task RoleSelection(string[] selectedNotification)
		{
			await DeferAsync(true);
			int.TryParse(selectedNotification[0], out int notificationId);

			try
			{
				await _notificationService.RemoveNotificationAsync(notificationId);
				await FollowupAsync("Notification deleted", ephemeral:true);
			}
			catch (Exception ex)
			{
				await FollowupAsync("An error has occurred", ephemeral:true);
			}
		}
		

		private DateTime GetDateTimeFromUnix(ulong timestamp)
		{
			DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			dateTime = dateTime.AddSeconds(timestamp);
			return dateTime;
		}


		private int AdjustTimezone(int hour, USTimeZones timezone)
		{
			//Did you know that postgresql doesn't have a DateTIme with Timezone Offset?
			switch (timezone)
			{
				case USTimeZones.Pacific:
					hour -= 8;
					break;
				case USTimeZones.Mountain:
					hour -= 6;
					break;
				case USTimeZones.Central:
					hour += 0;
					break;
				case USTimeZones.Eastern:
					hour += -1;
					break;
			}

			return hour;
		}
	}

	public enum USTimeZones
	{
		Pacific,
		Mountain,
		Central,
		Eastern
	}
}
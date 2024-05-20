using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using RaidReminder.Data;
using RaidReminder.Data.Models;
using RaidReminder.Services;

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



		[SlashCommand("add-reminder", "Creates a reminder that will notify your players")]
		public async Task VivaFaq(DayOfWeek dayOfWeek, 
			[Summary(description:"What hour of the day should the notification happen? (24hr format, CST)")][MaxValue(23)][MinValue(0)]int hour,
			[Summary(description:"What what minute of the hour should the notificaiton happen?")][MaxValue(59)][MinValue(0)] int minute,
			[Summary(description:"What role should we ping? (Default: true)")]IRole role,
			[Summary(description:"Should this reminder be repeated each week?")]bool repeatWeekly = true)
		{
			_logger.LogInformation("raid add-reminder executed");
			await DeferAsync();
			TimeOnly time = new TimeOnly(hour, minute);

			RaidNotificationModel model = new RaidNotificationModel(Context, role.Id, dayOfWeek, time);

			NotificationTimer timer = await _notificationService.AddNotificationAsync(model);

			TimestampTag tag = new TimestampTag(timer.NextNotification, TimestampTagStyles.Relative);

			EmbedBuilder builder = new EmbedBuilder()
				.WithTitle("New raid notification created")
				.WithDescription($"Next notification will occur {tag}");
			await FollowupAsync(embed: builder.Build());
		}
	}
}
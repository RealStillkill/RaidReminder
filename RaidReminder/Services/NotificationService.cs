using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RaidReminder.Data;
using RaidReminder.Data.Models;
using System;
using System.Reactive;
using System.Threading;
using System.Timers;

using Timer = System.Timers.Timer;

namespace RaidReminder.Services
{
	internal class NotificationService
	{

		private List<NotificationTimer> timers = new List<NotificationTimer>();

		private readonly IServiceProvider _serviceProvider;
		private readonly ILogger<NotificationService> _logger;

		public NotificationService(IServiceProvider serviceProvider, ILogger<NotificationService> logger)
		{
			_serviceProvider = serviceProvider;
			_logger = logger;

			_ = Task.Factory.StartNew(async () => await StartAsync());
		}

		public async Task<NotificationTimer> AddNotificationAsync(RaidNotificationModel notification)
		{
			using (var scope = _serviceProvider.CreateScope())
			using (ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
			{
				var model = await dbContext.RaidNotifications.AddAsync(notification);
				await dbContext.SaveChangesAsync();

				return InitializeNotification(model.Entity);
			}
		}

		public async Task<IEnumerable<RaidNotificationModel>> GetGuildNotifications(ulong guildId)
		{
			using (var scope = _serviceProvider.CreateScope())
			using (ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
			{
				return dbContext.RaidNotifications.Where(x => x.GuildId == guildId).ToArray();
			}
		}

		public DateTime GetNextNotificationDate(int id)
		{
			return timers.First(x => x.NotificationId == id).NextNotification;
		}

		public async Task RemoveNotificationAsync(int id)
		{
			using (var scope = _serviceProvider.CreateScope())
			using (ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
			{
				var model = await dbContext.RaidNotifications.FindAsync(id);
				if (model == null)
					return;

				dbContext.RaidNotifications.Remove(model);
				await dbContext.SaveChangesAsync();

				timers.Remove(timers.First(x => x.NotificationId == id));
			}
		}

		private NotificationTimer InitializeNotification(RaidNotificationModel model)
		{
			NotificationTimer timer = new NotificationTimer(model);
			timer.TimerElapsed += Timer_TimerElapsed;
			timers.Add(timer);
			return timer;
		}

		

		private async void Timer_TimerElapsed(object? sender, ElapsedEventArgs e)
		{
			void removeElapsedTimer(int id)
			{
				timers.Remove(timers.First(x => x.NotificationId == id));
			}

			using (var scope = _serviceProvider.CreateAsyncScope())
			using (ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
			{
				DiscordSocketClient client = scope.ServiceProvider.GetRequiredService<DiscordSocketClient>();
				int id = ((NotificationTimer)sender).NotificationId;

				RaidNotificationModel model = await dbContext.RaidNotifications.FindAsync(id);

				if (model == null)
					return;

				if (model.RepeatWeekly)
				{
					removeElapsedTimer(id);
					InitializeNotification(model);
				}
					
				else
				{
					dbContext.RaidNotifications.Remove(model);
					removeElapsedTimer(id);
				}

				SocketTextChannel channel = client.GetChannel(model.ChannelId) as SocketTextChannel;
				IRole role = client.GetGuild(model.GuildId).GetRole(model.RoleId);

				//ComponentBuilder compBuilder = new ComponentBuilder()
				//	.AddRow(new ActionRowBuilder()
				//	.WithButton("delete notification", "delete-notification", ButtonStyle.Danger));


				await channel.SendMessageAsync($"{role.Mention} it is time for raid.");
			}

			
		}

		public async Task StartAsync()
		{
			_logger.LogInformation("Initializing existing notifications");
			using (var scope = _serviceProvider.CreateScope())
			using (ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
			{
				foreach (RaidNotificationModel model in dbContext.RaidNotifications)
				{
					
					NotificationTimer timer = InitializeNotification(model);
					_logger.LogInformation($"Initialized notification on {timer.NextNotification}");
				}
			}
		}
	}

	internal class NotificationTimer
	{
		public int NotificationId { get; private set; }
		public DateTime NextNotification { get; private set; }

		public event EventHandler<ElapsedEventArgs> TimerElapsed;


		private readonly Timer _timer;

		public NotificationTimer(RaidNotificationModel raidNotification)
		{
			NotificationId = raidNotification.Id;
			DateTime currentDateTime = DateTime.Now;
			int daysToAdd = ((int)raidNotification.Day - (int)currentDateTime.DayOfWeek + 7) % 7;
			DateTime result = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, 
			raidNotification.Time.Hour, raidNotification.Time.Minute, raidNotification.Time.Second);
			NextNotification = result.AddDays(daysToAdd);
			if (NextNotification - DateTime.Now < TimeSpan.Zero)
				NextNotification = NextNotification.AddDays(7);
			TimeSpan interval = NextNotification - DateTime.Now;


			_timer = new Timer(interval);
			_timer.Elapsed += _timer_Elapsed;
			_timer.Start();
		}
		private void _timer_Elapsed(object? sender, ElapsedEventArgs e)
		{
			TimerElapsed?.Invoke(this, e);
		}
	}
}
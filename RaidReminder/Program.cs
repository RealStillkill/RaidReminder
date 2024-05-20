using Discord.WebSocket;
using Discord;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RaidReminder.Services;
using Discord.Interactions;
using RaidReminder.Data;
using Microsoft.EntityFrameworkCore;

namespace RaidReminder
{
	internal class Program
	{
		private static IHost App { get; set; }

		static async Task Main(string[] args)
		{
			HostApplicationBuilder builder = new HostApplicationBuilder();
			if (IsDebugMode())
			{
				builder.Configuration.AddJsonFile("appsettings.development.json", false, true);
			}
			else builder.Configuration.AddJsonFile("appsettings.json", false, true);
			string constring = builder.Configuration.GetConnectionString("DefaultConnection");

			builder.Services.AddDbContext<ApplicationDbContext>(options =>
				options.UseNpgsql(constring), optionsLifetime: ServiceLifetime.Singleton);

			builder.Services.AddLogging(options =>
			{
				if (IsDebugMode())
					options.AddConsole();
				else options.AddSystemdConsole();
			});

			var client = new DiscordSocketClient(new DiscordSocketConfig
			{
				ConnectionTimeout = 8000,
				HandlerTimeout = 3000,
				MessageCacheSize = 25,
				LogLevel = LogSeverity.Debug,
				GatewayIntents = GatewayIntents.All
			});
			builder.Services.AddSingleton<NotificationService>();
			builder.Services.AddSingleton(client);
			builder.Services.AddSingleton(new InteractionService(client, new InteractionServiceConfig
			{
				//AutoServiceScopes = true,
				LogLevel = LogSeverity.Debug,
				UseCompiledLambda = true
			}));
			builder.Services.AddHostedService<DiscordService>();
			App = builder.Build();

			using (var scope = App.Services.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
				if ((await dbContext.Database.GetPendingMigrationsAsync()).Count() > 0)
					await dbContext.Database.MigrateAsync();
				dbContext.Dispose();
			}
			Console.CancelKeyPress += Console_CancelKeyPress;
			await App.StartAsync();
			await App.WaitForShutdownAsync();
		}

		private static async void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
		{
			e.Cancel = true;
			await App.StopAsync();
		}

		public static bool IsDebugMode()
		{
#if DEBUG
			return true;
#else
			return false;
#endif
		}
	}
}
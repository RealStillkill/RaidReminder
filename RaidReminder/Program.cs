using Discord.WebSocket;
using Discord;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RaidReminder
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			HostApplicationBuilder builder = new HostApplicationBuilder();
			if (IsDebugMode())
			{
				builder.Configuration.AddJsonFile("appsettings.development.json", false, true);
			}
			else builder.Configuration.AddJsonFile("appsettings.json", false, true);
			string constring = builder.Configuration.GetConnectionString("DefaultConnection");
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

			builder.Services.AddSingleton(client);
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
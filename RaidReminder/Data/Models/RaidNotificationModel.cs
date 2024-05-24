using Discord.Interactions;
using RaidReminder.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidReminder.Data.Models
{
	internal class RaidNotificationModel
	{
		public int Id { get; set; }
		public DayOfWeek Day { get; set; }
		public TimeOnly Time { get; set; }
		public ulong GuildId { get; set; }
		public ulong ChannelId { get; set; }
		public ulong RoleId { get; set; }
		public bool RepeatWeekly { get; set; }

		public RaidNotificationModel()
		{
		}

		public RaidNotificationModel(SocketInteractionContext context, ulong roleId, DayOfWeek day, TimeOnly time, bool repeatWeekly)
		{
			GuildId = context.Guild.Id;
			ChannelId = context.Channel.Id;
			RoleId = roleId;
			Time = time;
			Day = day;
			RepeatWeekly = repeatWeekly;
		}
	}
}

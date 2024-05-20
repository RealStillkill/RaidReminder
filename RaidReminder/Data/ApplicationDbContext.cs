using Microsoft.EntityFrameworkCore;
using RaidReminder.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidReminder.Data
{
	internal class ApplicationDbContext : DbContext
	{
		public DbSet<RaidNotificationModel> RaidNotifications { get; set; }
		public ApplicationDbContext()
		{
		}

		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
		{

		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<RaidNotificationModel>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.Property(e => e.Id)
					.ValueGeneratedOnAdd();
			});
		}
	}
}

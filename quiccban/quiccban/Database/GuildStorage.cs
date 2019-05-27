using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using quiccban.Database.Models;

namespace quiccban.Database
{
    public class GuildStorage : DbContext
    {
        private DbSet<Guild> Guilds { get; set; }

        public GuildStorage()
        { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source=\"{Program.dataPath + "/database.db"}\"");
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Guild>(g =>
            {
                g.HasKey(x => x.Id);

                g.Property(x => x.LogStyle)
                .HasDefaultValue(LogStyle.Modern);

                g.Property(x => x.WarnThreshold)
                .HasDefaultValue(3);

                g.Property(x => x.WarnExpiry)
                .HasDefaultValue(86400);

                g.Property(x => x.WarnThresholdActionType)
                .HasDefaultValue(Models.ActionType.Tempmute);

                g.Property(x => x.WarnThresholdActionExpiry)
                .HasDefaultValue(600);

                g.OwnsOne(x => x.AutoMod);


                g.HasMany(x => x.Cases).WithOne(x => x.Guild).HasForeignKey(x => x.GuildId);


            });


            modelBuilder.Entity<Case>(c => {
                c.HasKey(x => x.Id);

                c.Property(x => x.Id).ValueGeneratedOnAdd();
            });
            
        }

        public async Task<Guild> GetOrCreateGuildAsync(ulong guildId)
            => await Guilds.Include(x => x.Cases).FirstOrDefaultAsync(x => x.Id == guildId) ?? await CreateGuildAsync(guildId);


        public async Task<List<Guild>> GetAllGuildsAsync()
            => await Guilds.Include(x => x.Cases).ToListAsync();



        private async Task<Guild> CreateGuildAsync(ulong guildId)
        {
            var newGuild = new Guild { Id = guildId, AutoMod = new AutoMod { Enabled = true, SpamEnabled = true, SpamActionType = Models.ActionType.Warn, SpamMessageThreshold = 5} };

            await Guilds.AddAsync(newGuild);

            await SaveChangesAsync();

            return await Guilds.Include(x => x.Cases).FirstOrDefaultAsync(x => x.Id == guildId);
        }
    }
}

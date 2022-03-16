using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using SlickReship_Addresses.Models;

namespace SlickReship_Addresses.Modules
{
    class SlashCommands
    {
        public static DiscordSocketClient _client;

        static ulong[] adminRoleId = { 271005417923018764, 534191993820282880, 561592491690688532, 935809270673453096, 404029842548326410 };

        public async Task AddRoleKeyword(SocketSlashCommand command)
        {
            keyword = keyword.ToLower();

            if (!Globals.LoadedKeywords.ContainsKey(keyword))
            {
                Globals.LoadedKeywords.Add(keyword, new KeywordInfo());
            }
            else
            {
                if (Globals.LoadedKeywords[keyword].RoleId.Any(roleId => roleId == role.Id))
                {
                    await ReplyAsync($"That role already has {keyword} in its keyword list!");
                    return;
                }
            }

            Globals.LoadedKeywords[keyword].RoleId.Add(role.Id);

            Functions.Functions.SaveKeywordsToFile();

            await ReplyAsync($"Added {keyword} to <@&{role.Id}> keyword list!");
        }

        public async Task RemoveRoleKeyword(SocketSlashCommand command)
        {
            keyword = keyword.ToLower();

            if (!Globals.LoadedKeywords.ContainsKey(keyword))
            {
                await ReplyAsync($"{keyword} was not in that roles keyword list!");
                return;
            }

            Globals.LoadedKeywords[keyword].RoleId = new List<ulong>();

            Functions.Functions.SaveKeywordsToFile();

            await ReplyAsync($"Removed {keyword} from all role keyword lists!");
        }

        public async Task ViewRoleKeywords(SocketSlashCommand command)
        {
            var message = Globals.LoadedKeywords.Where(keyword => keyword.Value.RoleId.Contains(role.Id)).Aggregate("", (current, keyword) => current + (keyword.Key + "\n"));

            var embedBuilder = new EmbedBuilder();
            var embed = embedBuilder
                .WithAuthor(author =>
                {
                    author.IconUrl =
                        "https://i.imgur.com/4znWswM.png";
                    author.Name = "Astro Alerts Keyword Pinger";
                })
                .WithColor(Color.Purple)
                .WithTitle($"@{role.Name}'s Keyword List!")
                .WithDescription(message)
                .WithFooter($"Keyword Pinger")
                .WithCurrentTimestamp()
                .Build();

            await ReplyAsync("", false, embed);
        }

        // Embed

        public async Task AddEmbedKeyword(SocketSlashCommand command)
        {
            keyword = keyword.ToLower();

            if (!Globals.LoadedKeywords.ContainsKey(keyword))
            {
                Globals.LoadedKeywords.Add(keyword, new KeywordInfo());
            }
            else
            {
                if (Globals.LoadedKeywords[keyword].ChannelId.Any(channelId => channelId == channel.Id))
                {
                    await ReplyAsync($"{keyword} is already in the embed keyword list!");
                    return;
                }
            }

            Globals.LoadedKeywords[keyword].ChannelId.Add(channel.Id);

            Functions.Functions.SaveKeywordsToFile();

            await ReplyAsync($"Added {keyword} to <#{channel.Id}> embed keyword list!");
        }

        public async Task RemoveEmbedKeyword(SocketSlashCommand command)
        {
            keyword = keyword.ToLower();

            if (!Globals.LoadedKeywords.ContainsKey(keyword))
            {
                await ReplyAsync($"{keyword} was not in that roles keyword list!");
                return;
            }

            Globals.LoadedKeywords[keyword].ChannelId = new List<ulong>();

            Functions.Functions.SaveKeywordsToFile();

            await ReplyAsync($"Removed {keyword} from all embed keyword lists!");
        }

        public async Task ViewEmbedKeywords(SocketSlashCommand command)
        {
            var message = Globals.LoadedKeywords.Where(keyword => keyword.Value.ChannelId.Contains(channel.Id)).Aggregate("", (current, keyword) => current + (keyword.Key + "\n"));

            var embedBuilder = new EmbedBuilder();
            var embed = embedBuilder
                .WithAuthor(author =>
                {
                    author.IconUrl =
                        "https://i.imgur.com/4znWswM.png";
                    author.Name = "Astro Alerts Keyword Pinger";
                })
                .WithColor(Color.Purple)
                .WithTitle($"#{channel.Name}'s Embed Keyword List!")
                .WithDescription(message)
                .WithFooter($"Keyword Pinger")
                .WithCurrentTimestamp()
                .Build();

            await ReplyAsync("", false, embed);
        }

        // Admin Set Monitor Channels

        public async Task AddChannel(SocketSlashCommand command)
        {
            if (Globals.MonitoredChannels.Contains(channel.Id))
            {
                await ReplyAsync($"<#{channel.Id}> is already being monitored!");
                return;
            }

            Globals.MonitoredChannels.Add(channel.Id);

            Functions.Functions.SaveKeywordsToFile();

            await ReplyAsync($"Added <#{channel.Id}> to the active monitoring list!");
        }

        public async Task RemoveChannel(SocketSlashCommand command)
        {
            if (Globals.MonitoredChannels.Contains(channel.Id))
            {
                Globals.MonitoredChannels.Remove(channel.Id);

                await ReplyAsync($"Removed <#{channel.Id}> from monitored channels!");
                return;
            }

            Functions.Functions.SaveKeywordsToFile();

            await ReplyAsync($"<#{channel.Id}> wasn't being monitored!");
        }

        public async Task ViewChannels(SocketSlashCommand command)
        {
            var message = Globals.MonitoredChannels.Aggregate("", (current, channel) => current + $"<#{channel}>\n");

            var embedBuilder = new EmbedBuilder();
            var embed = embedBuilder
                .WithAuthor(author =>
                {
                    author.IconUrl =
                        "https://i.imgur.com/4znWswM.png";
                    author.Name = "Astro Alerts Keyword Pinger";
                })
                .WithColor(Color.Purple)
                .WithTitle($"Current Channels Being Monitored")
                .WithDescription(message)
                .WithFooter($"Keyword Pinger")
                .WithCurrentTimestamp()
                .Build();

            await ReplyAsync("", false, embed);
        }

        public async Task AdminHelp(SocketSlashCommand command)
        {
            var embedBuilder = new EmbedBuilder();
            var embed = embedBuilder
                .WithAuthor(author =>
                {
                    author.IconUrl =
                        "https://i.imgur.com/4znWswM.png";
                    author.Name = "Astro Alerts Keyword Pinger";
                })
                .WithColor(Color.Purple)
                .WithTitle($"Admin Command List")
                .WithDescription("**.kw role add** {@role} {keyword} \n*Add a keyword to a roles keyword list*\n" +
                                 "**.kw role remove** {keyword} \n*Remove a keyword from a roles keyword list*\n" +
                                 "**.kw role view** {role} \n*Show all keywords in a roles keyword list*\n\n" +
                                 "**.kw embed add** {#channel} {keyword} \n*Add a keyword to the embed ping keyword list (#channel is channel the ping sent to)*\n" +
                                 "**.kw embed remove** {keyword} \n*Remove a keyword from the embed ping keyword list*\n" +
                                 "**.kw embed view** {channel} \n*View all keywords in a channels custom embed ping list*\n\n" +
                                 "**.kw channel add** {#channel} \n*Add a channel to the active monitoring list*\n" +
                                 "**.kw channel remove** {#channel} \n*Remove a channel from the active monitoring list*\n" +
                                 "**.kw channel view** \n*Show all active monitoring channels*\n")
                .WithFooter($"Keyword Pinger")
                .WithCurrentTimestamp()
                .Build();

            await ReplyAsync("", false, embed);
        }

        public async Task AddKeyword(SocketSlashCommand command)
        {
            keyword = keyword.ToLower();

            if (!Globals.LoadedKeywords.ContainsKey(keyword))
            {
                Globals.LoadedKeywords.Add(keyword, new KeywordInfo());
            }
            else
            {
                if (Globals.LoadedKeywords[keyword].UserId.Any(keywordInfo => keywordInfo == Context.User.Id))
                {
                    await ReplyAsync($"{keyword} already exists in your keyword list!");
                    return;
                }
            }

            Globals.LoadedKeywords[keyword].UserId.Add(Context.User.Id);

            Functions.Functions.SaveKeywordsToFile();

            await ReplyAsync($"Added {keyword} to your keyword list!");
        }

        public async Task RemoveKeyword(SocketSlashCommand command)
        {
            keyword = keyword.ToLower();

            if (!Globals.LoadedKeywords.ContainsKey(keyword) || !Globals.LoadedKeywords[keyword].UserId.Contains(Context.User.Id))
            {
                await ReplyAsync($"{keyword} was not in your keyword list!");
                return;
            }

            Globals.LoadedKeywords[keyword].UserId.RemoveAll(x => x == Context.User.Id);

            Functions.Functions.SaveKeywordsToFile();

            await ReplyAsync($"Removed {keyword} from your keyword list!");
        }

        public async Task ViewKeywords(SocketSlashCommand command)
        {
            var message = Globals.LoadedKeywords.Where(keyword => keyword.Value.UserId.Contains(Context.User.Id)).Aggregate("", (current, keyword) => current + (keyword.Key + "\n"));

            var embedBuilder = new EmbedBuilder();
            var embed = embedBuilder
                .WithAuthor(author =>
                {
                    author.IconUrl =
                        "https://i.imgur.com/4znWswM.png";
                    author.Name = "Astro Alerts Keyword Pinger";
                })
                .WithColor(Color.Purple)
                .WithTitle($"Your Keyword List {Context.User.Username}!")
                .WithDescription(message)
                .WithFooter($"Keyword Pinger")
                .WithCurrentTimestamp()
                .Build();

            await ReplyAsync("", false, embed);
        }

        public async Task Help(SocketSlashCommand command)
        {
            var embedBuilder = new EmbedBuilder();
            var embed = embedBuilder
                .WithAuthor(author =>
                {
                    author.IconUrl =
                        "https://i.imgur.com/4znWswM.png";
                    author.Name = "Astro Alerts Keyword Pinger";
                })
                .WithColor(Color.Purple)
                .WithColor(new Color(77, 169, 214))
                .WithTitle($"Command List")
                .WithDescription("**.kw add** {keyword} \n*Add a keyword to your keyword list*\n" +
                                 "**.kw remove** {keyword} \n*Removes a keyword from your keyword list*\n" +
                                 "**.kw view** \n*Show all keywords in your keyword list*\n")
                .WithFooter($"Keyword Pinger")
                .WithCurrentTimestamp()
                .Build();

            await ReplyAsync("", false, embed);
        }
    }
}
}

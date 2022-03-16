using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Keyword_Pinger.Models;

namespace Keyword_Pinger.Functions
{
    class KeywordChecker
    {
        public static DiscordSocketClient _client;

        public static Task CheckForKeyword(SocketMessage message)
        {
            if (!Globals.MonitoredChannels.Contains(message.Channel.Id)) return Task.CompletedTask; // ignore if channel isnt monitored

            var lockObject = new object();
            var total = 0;

            var messageContent = new string[1];
            messageContent[0] = message.Content.ToLower();

            if (message.Embeds is not null)
            {
                foreach (var embed in message.Embeds)
                {
                    messageContent[0] += embed.Title?.ToLower() + " " + embed.Description?.ToLower();
                }
            }

            //the chars/strings to look for. We use both because we're testing some string methods too.
            var keywords = Globals.LoadedKeywords.Keys.ToArray();
            //the count of each substring finding

            Parallel.For(0, messageContent.Length,
                () => 0,
                (x, loopState, subtotal) =>
                {
                    foreach (var keyword in keywords)
                    {
                        if (((messageContent[x].Length - messageContent[x].Replace(keyword, string.Empty).Length) / keyword.Length > 0 ? 1 : 0) !=
                            1) continue;

                        Console.WriteLine($"Found match! {keyword}");

                        AlertDiscord(message, keyword); 

                        total++;
                    }

                    return subtotal;
                },
                (s) =>
                {
                    lock (lockObject)
                    {
                        total += s;
                    }
                }
            );
            return Task.CompletedTask;
        }

        public static Task AlertDiscord(SocketMessage message, string keyword)
        {
            var channel = message.Channel;

            var keywordInfo = Globals.LoadedKeywords[keyword];

            if (keywordInfo.LastPinged > DateTime.Now.AddMinutes(-1)) return Task.CompletedTask;

            if (keywordInfo.UserId.Count > 0)
            {
                var userMessage = keywordInfo.UserId.Aggregate("", (current, userId) => current + $"<@{userId}> ");

                channel.SendMessageAsync(userMessage);
            }

            if (keywordInfo.RoleId.Count > 0)
            {
                var userMessage = keywordInfo.RoleId.Aggregate("", (current, roleId) => current + $"<@&{roleId}> ");

                channel.SendMessageAsync(userMessage);
            }

            if (keywordInfo.ChannelId.Count > 0)
            {
                foreach (var channelId in keywordInfo.ChannelId)
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
                        .WithTitle($"Keyword Detected! \"{keyword}\"")
                        .WithDescription($"**[Click here to go to the message]({message.GetJumpUrl()})**")
                        .WithFooter($"Keyword Pinger")
                        .WithCurrentTimestamp()
                        .Build();

                    var pingChannel = (IMessageChannel)_client.GetChannel(channelId);

                    pingChannel.SendMessageAsync("", false, embed);
                }
            }

            Globals.LoadedKeywords[keyword].LastPinged = DateTime.Now;

            return Task.CompletedTask;
        }
    }
}

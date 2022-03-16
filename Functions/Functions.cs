using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Keyword_Pinger.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Keyword_Pinger.Functions
{
    public static class Functions
    {
        private static object writeLocker = new Object();

        public static async Task SetBotStatusAsync(  DiscordSocketClient client)
        {
            var config = GetConfig();

            var currently = config["currently"]?.Value<string>().ToLower();
            var statusText = config["playing_status"]?.Value<string>();
            var onlineStatus = config["status"]?.Value<string>().ToLower();

            // Set the online status
            if (!string.IsNullOrEmpty(onlineStatus))
            {
                var userStatus = onlineStatus switch
                {
                    "dnd" => UserStatus.DoNotDisturb,
                    "idle" => UserStatus.Idle,
                    "offline" => UserStatus.Invisible,
                    _ => UserStatus.Online
                };

                await client.SetStatusAsync(userStatus);
                Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} | Online status set | {userStatus}");
            }

            // Set the playing status
            if (!string.IsNullOrEmpty(currently) && !string.IsNullOrEmpty(statusText))
            {
                var activity = currently switch
                {
                    "listening" => ActivityType.Listening,
                    "watching" => ActivityType.Watching,
                    "streaming" => ActivityType.Streaming,
                    _ => ActivityType.Playing
                };

                await client.SetGameAsync(statusText, type: activity);
                Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} | Playing status set | {activity}: {statusText}");
            }            
        }

        public static JObject GetConfig()
        {
            // Get the config file.
            using var configJson = new StreamReader(Directory.GetCurrentDirectory() + @"/config.json");
                return (JObject)JsonConvert.DeserializeObject(configJson.ReadToEnd());
        }

        public static void LoadKeywordsFromFile()
        {
            try
            {
                var keywordsJson = new StreamReader(Directory.GetCurrentDirectory() + @"/keywords.json");
                var keywordObject = (JObject) JsonConvert.DeserializeObject(keywordsJson.ReadToEnd());

                foreach (var (key, value) in keywordObject)
                {
                    var userIdList = value["UserId"].Values<ulong>().ToList();
                    var roleIdList = value["RoleId"].Values<ulong>().ToList();
                    var channelIdList = value["ChannelId"].Values<ulong>().ToList();

                    Globals.LoadedKeywords.Add(key, new KeywordInfo
                    {
                        ChannelId = channelIdList,
                        UserId = userIdList,
                        RoleId = roleIdList,
                        LastPinged = DateTime.Now
                    });
                }

                var channelsJson = new StreamReader(Directory.GetCurrentDirectory() + @"/channels.json");
                var channelsObject = (JArray) JsonConvert.DeserializeObject(channelsJson.ReadToEnd());

                foreach (var channelId in channelsObject.Values<ulong>())
                {
                    Globals.MonitoredChannels.Add(channelId);
                }

                Console.WriteLine("Loaded Keywords and Channels from file!");
            }
            catch
            {
                // do nothing
            }
        }

        public static void SaveKeywordsToFile()
        {
            var keywordsJson = JsonConvert.SerializeObject(Globals.LoadedKeywords);
            var channelsJson = JsonConvert.SerializeObject(Globals.MonitoredChannels);

            try
            {
                lock (writeLocker)
                {
                    File.WriteAllText(Directory.GetCurrentDirectory() + @"/keywords.json", keywordsJson);
                    File.WriteAllText(Directory.GetCurrentDirectory() + @"/channels.json", channelsJson);
                }
            }
            catch
            {
                // File busy so not writing. 
                // Doesn't matter as its just saving total progress. May lose one or two if fails.
            }
        }
    }
}

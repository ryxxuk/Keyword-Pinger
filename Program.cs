using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Keyword_Pinger.Functions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.Threading;
using Discord.Net;
using Newtonsoft.Json;
using SlickReship_Addresses.Modules;

namespace Keyword_Pinger
{
    public class Program
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        static Task Main(string[] args)
        {
            return new Program().MainAsync();
        }

        private Program()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                MessageCacheSize = 250,
                AlwaysDownloadUsers = true
            });

            _commands = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Info,
                CaseSensitiveCommands = false,
            });

            _client.Log += Log;
            _commands.Log += Log;
        }

        private static Task Log(LogMessage message)
        {
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }
            Console.WriteLine($"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message} {message.Exception}");
            Console.ResetColor();

            return Task.CompletedTask;
        }

        private async Task MainAsync()
        {
            var config = Functions.Functions.GetConfig();
            var token = config["token"].Value<string>();

            await InitCommands();

            Functions.Functions.LoadKeywordsFromFile();

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            //SlashCommands._client = _client;
            //Functions._client = _client;

            await Task.Delay(Timeout.Infinite);
        }

        private async Task InitCommands()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            _client.Ready += ClientReadyAsync;
            _client.MessageReceived += CheckMessageForKeyword;
            _client.SlashCommandExecuted += SlashCommandHandler;
        }

        private async Task ClientReadyAsync()
            => await Functions.Functions.SetBotStatusAsync(_client);


        private async Task CheckMessageForKeyword(SocketMessage rawMessage)
        {
            if (rawMessage.Author.Id != _client.CurrentUser.Id)
            {
                _ = KeywordChecker.CheckForKeyword(rawMessage);
            }
        }

        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            switch (command.Data.Name)
            {
                case "address":
                    switch (command.Data.Options.FirstOrDefault()?.Name)
                    {
                        case "set":
                            Console.WriteLine($"{DateTime.Now} [Command] {command.User.Username} Executed /address set");
                            await SlashCommands.SetAddress(command);
                            break;
                        case "edit":
                            Console.WriteLine($"{DateTime.Now} [Command] {command.User.Username} Executed /address edit");
                            await SlashCommands.EditAddress(command);
                            break;
                        case "delete":
                            Console.WriteLine($"{DateTime.Now} [Command] {command.User.Username} Executed /address delete");
                            await SlashCommands.DeleteAddress(command);
                            break;
                        case "view":
                            Console.WriteLine($"{DateTime.Now} [Command] {command.User.Username} Executed /address view");
                            await SlashCommands.ViewAddress(command);
                            break;
                        case "send":
                            Console.WriteLine($"{DateTime.Now} [Command] {command.User.Username} Executed /address send");
                            await SlashCommands.SendAddress(command);
                            break;
                    }
                    break;
                case "vacation":
                    Console.WriteLine($"{DateTime.Now} [Command] {command.User.Username} Executed /vacation");
                    break;
                case "create-embeds":
                    Console.WriteLine($"{DateTime.Now} [Command] {command.User.Username} Executed /create-embeds");
                    await SlashCommands.ClaimEmbeds(command);
                    break;
                case "create-premium-embeds":
                    Console.WriteLine($"{DateTime.Now} [Command] {command.User.Username} Executed /create-premium-embeds");
                    await SlashCommands.ClaimPremiumEmbeds(command);
                    break;
            }
        }

        public async Task CreateSlashCommands()
        {
            const ulong guildId = 893087963582464030;


            var slashCommandBuilders = new List<SlashCommandBuilder>
            {
                new SlashCommandBuilder()
                    .WithName("address")
                    .WithDescription("Manage the address of a user")
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("set")
                        .WithDescription("Sets the address of a user")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption(countryOptions)
                        .AddOption("firstname", ApplicationCommandOptionType.String, "Enter [Your Discord Name] for automatic placeholder", isRequired: true)
                        .AddOption("lastname", ApplicationCommandOptionType.String, "Enter [Your Discord Name] for automatic placeholder", isRequired: true)
                        .AddOption("housenumber", ApplicationCommandOptionType.String, "House Number excluding Street Address", isRequired: true)
                        .AddOption("address1", ApplicationCommandOptionType.String, "First line of your address, minus house number.", isRequired: true)
                        .AddOption("address2", ApplicationCommandOptionType.String, "Second line of your address (Optional)", isRequired: false)
                        .AddOption("zipcode", ApplicationCommandOptionType.String, "Your zip or post code", isRequired: true)
                        .AddOption("city", ApplicationCommandOptionType.String, "Your city", isRequired: true)
                        .AddOption("state", ApplicationCommandOptionType.String, "State or County", isRequired: true)
                        .AddOption("phoneprefix", ApplicationCommandOptionType.String, "Your phone country code e.g. +44", isRequired: true)
                        .AddOption("phonenumber", ApplicationCommandOptionType.String, "Your phone number ignoring leading zeros", isRequired: true))
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("edit")
                        .WithDescription("Edits the address of a user")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption(countryOptions)
                        .AddOption("firstname", ApplicationCommandOptionType.String, "Enter '[Your Discord Name]' for automatic placeholder", isRequired: false)
                        .AddOption("lastname", ApplicationCommandOptionType.String, "Enter '[Your Discord Name]' for automatic placeholder", isRequired: false)
                        .AddOption("housenumber", ApplicationCommandOptionType.String, "House Number excluding Street Address", isRequired: false)
                        .AddOption("address1", ApplicationCommandOptionType.String, "First line of your address, minus house number.", isRequired: false)
                        .AddOption("address2", ApplicationCommandOptionType.String, "Second line of your address (Optional)", isRequired: false)
                        .AddOption("zipcode", ApplicationCommandOptionType.String, "Your zip or post code", isRequired: false)
                        .AddOption("city", ApplicationCommandOptionType.String, "Your city", isRequired: false)
                        .AddOption("state", ApplicationCommandOptionType.String, "State or County", isRequired: false)
                        .AddOption("phoneprefix", ApplicationCommandOptionType.String, "Your phone country code e.g. +44", isRequired: false)
                        .AddOption("phonenumber", ApplicationCommandOptionType.String, "Your phone number ignoring leading zeros", isRequired: false))
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("delete")
                        .WithDescription("Deletes the address of a user")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption(countryOptions))
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("view")
                        .WithDescription("See all your addresses")
                        .AddOption("user", ApplicationCommandOptionType.User, "The user you want to see addresses", isRequired: false)
                        .WithType(ApplicationCommandOptionType.SubCommand))
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("send")
                        .WithDescription("Sets an address of a user to another user")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("reshipper", ApplicationCommandOptionType.User, "The reshipper you want an address to be sent of", isRequired: true)
                        .AddOption(countryOptions)
                        .AddOption("customer", ApplicationCommandOptionType.User, "The user who you want to send the address to", isRequired: true)),
                     new SlashCommandBuilder()
                    .WithName("vacation")
                    .WithDescription("Manage the address of a user")
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("status")
                        .WithDescription("What is the user vacation status")
                        .WithRequired(true)
                        .AddChoice("Away", "away")
                        .AddChoice("Home", "home")
                        .WithType(ApplicationCommandOptionType.String)),
                new SlashCommandBuilder()
                    .WithName("create-embeds")
                    .WithDescription("Create the claim address embeds in this channel"),
                new SlashCommandBuilder()
                    .WithName("create-premium-embeds")
                    .WithDescription("Create the claim address embeds in this channel for premium addresses")
            };
            try
            {
                foreach (var slashCommand in slashCommandBuilders)
                {
                    await _client.Rest.CreateGuildCommand(slashCommand.Build(), guildId);
                }
            }
            catch (HttpException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Reason, Formatting.Indented);
                Console.WriteLine(json);
            }
        }
    }
}
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.Reflection;

public class CSharpBot
{
    public static Task Main(string[] args) => new CSharpBot().MainAsync();

    public async Task MainAsync()
    {
        var _config = new DiscordSocketConfig { MessageCacheSize = 100 };
        _client = new DiscordSocketClient(_config);

        _client.Log += Log;
        _client.Ready += Client_Ready;
        _client.SlashCommandExecuted += SlashCommandHandler;

        string? token = Environment.GetEnvironmentVariable("DiscordToken");

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        // Block this task until the program is closed.
        await Task.Delay(-1);
    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    private DiscordSocketClient? _client;

    public async Task Client_Ready()
    {
        // Let's build a guild command! We're going to need a guild so lets just put that in a variable.
        var guild = _client!.GetGuild(718253204147798047);

        // Next, lets create our slash command builder. This is like the embed builder but for slash commands.
        var guildCommand = new SlashCommandBuilder();

        // Note: Names have to be all lowercase and match the regular expression ^[\w-]{3,32}$
        guildCommand.WithName("first-command");

        // Descriptions can have a max length of 100.
        guildCommand.WithDescription("This is my first guild slash command!");

        // Let's do our global command
        var globalCommand = new SlashCommandBuilder();
        globalCommand.WithName("first-global-command");
        globalCommand.WithDescription("This is my first global slash command");

        var guildCommand2 = new SlashCommandBuilder()
        .WithName("list-roles")
        .WithDescription("Lists all roles of a user.")
        .AddOption("user", ApplicationCommandOptionType.User, "The users whos roles you want to be listed", isRequired: true);

        try
        {
            await _client.Rest.CreateGuildCommand(guildCommand.Build(), 718253204147798047);
        }
        catch (HttpException exception)
        {
            var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
            Console.WriteLine(json);
        }

        try
        {
            // Now that we have our builder, we can call the CreateApplicationCommandAsync method to make our slash command.
            await guild.CreateApplicationCommandAsync(guildCommand.Build());

            // With global commands we don't need the guild.
            await _client.CreateGlobalApplicationCommandAsync(globalCommand.Build());
            // Using the ready event is a simple implementation for the sake of the example. Suitable for testing and development.
            // For a production bot, it is recommended to only run the CreateGlobalApplicationCommandAsync() once for each command.
        }
        catch (HttpException exception)
        {
            // If our command was invalid, we should catch an ApplicationCommandException. This exception contains the path of the error as well as the error message. You can serialize the Error field in the exception to get a visual of where your error is.
            var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);

            // You can send this error somewhere or just print it to the console, for this example we're just going to print it.
            Console.WriteLine(json);
        }


    }
    private async Task SlashCommandHandler(SocketSlashCommand command)
    {
        await command.RespondAsync($"You executed {command.Data.Name}");

        switch (command.Data.Name)
        {
            case "list-roles":
                await HandleListRoleCommand(command);
                break;
        }
    }

    private async Task HandleListRoleCommand(SocketSlashCommand command)
    {
        // We need to extract the user parameter from the command. since we only have one option and it's required, we can just use the first option.
        var guildUser = (SocketGuildUser)command.Data.Options.First().Value;

        // We remove the everyone role and select the mention of each role.
        var roleList = string.Join(",\n", guildUser.Roles.Where(x => !x.IsEveryone).Select(x => x.Mention));

        var embedBuiler = new EmbedBuilder()
            .WithAuthor(guildUser.ToString(), guildUser.GetAvatarUrl() ?? guildUser.GetDefaultAvatarUrl())
            .WithTitle("Roles")
            .WithDescription(roleList)
            .WithColor(Color.Green)
            .WithCurrentTimestamp();

        // Now, Let's respond with the embed.
        await command.RespondAsync(embed: embedBuiler.Build());
    }


}



internal class AConfigurationClass
{
    public string? Token;
}
using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

namespace Kekkonen.Services
{
    public class LoggingService
    {
        private readonly CommandService _commandService;
        private readonly DiscordSocketClient _discordSocket;

        private readonly string _folder = Path.Combine(AppContext.BaseDirectory, "logs");

        public LoggingService(CommandService commandService, DiscordSocketClient discordSocket)
        {
            _discordSocket = discordSocket;
            _commandService = commandService;

            _discordSocket.Log += LogAsync;
            _commandService.Log += LogAsync;
        }

        private string LogFile => Path.Combine(_folder, $"{DateTime.Now:yyyy-MM-dd}.log");

        private Task LogAsync(LogMessage logMessage)
        {
            if (Directory.Exists(_folder) == false)
            {
                Directory.CreateDirectory(_folder);
            }

            if (File.Exists(LogFile) == false)
            {
                File.Create(LogFile).Dispose();
            }

            var text =
                $"{DateTime.Now:HH:mm:ss} [{logMessage.Severity}] {logMessage.Source}: {logMessage.Exception?.ToString() ?? logMessage.Message}";
            File.AppendAllText(LogFile, text + "\r\n");

            return Console.Out.WriteLineAsync(text);
        }
    }
}
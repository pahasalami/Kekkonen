using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kekkonen.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kekkonen
{
    public class Runner
    {
        public Runner()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("configuration.json");

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        public static async Task RunAsync(string[] args)
        {
            var runner = new Runner();
            await runner.StartAsync(args);
        }

        public async Task StartAsync(string[] args)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);

            var provider = services.BuildServiceProvider();
            provider.GetRequiredService<LoggingService>();
            provider.GetRequiredService<CommandHandler>();
            provider.GetRequiredService<MuteService>();

            await provider.GetRequiredService<LoginService>().StartAsync();
            await Task.Delay(-1);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Verbose,
                    MessageCacheSize = 2048
                }))
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    LogLevel = LogSeverity.Verbose,
                    DefaultRunMode = RunMode.Async,
                    CaseSensitiveCommands = false
                }))
                .AddSingleton<CommandHandler>()
                .AddSingleton<LoginService>()
                .AddSingleton<LoggingService>()
                .AddSingleton<MuteService>()
                .AddSingleton(Configuration);
        }
    }
}
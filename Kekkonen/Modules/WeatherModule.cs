using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;

namespace Kekkonen.Modules
{
    [Name("Weather")]
    [RequireContext(ContextType.Guild)]
    public class WeatherModule : ModuleBase<SocketCommandContext>
    {
        private readonly IConfigurationRoot _configuration;

        public WeatherModule(IConfigurationRoot configuration)
        {
            _configuration = configuration;
        }

        [Command("sää")]
        [RequireUserPermission(GuildPermission.SendMessages)]
        public async Task GetWeather([Remainder] string input)
        {
            if (Context.Channel.Name != "bot-spam") return;

            var message = await ReplyAsync(":thinking:");

            var client = new RestClient("http://api.openweathermap.org/");
            var request = new RestRequest("data/2.5/weather", Method.GET)
            {
                RequestFormat = DataFormat.Json,
                UseDefaultCredentials = false
            };

            var api = _configuration.GetSection("api").GetChildren().Where(n => n.Key == "weather")
                .Select(x => x.Value).FirstOrDefault();

            request.AddParameter("appid", api);
            request.AddParameter("q", input);
            request.AddParameter("units", "metric");
            request.AddHeader("User-Agent", "kekkonen+github.com/pahasalami/Kekkonen");

            var response = client.Execute<WeatherApiResponse>(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                await message.ModifyAsync(x => { x.Content = $":lennu: ({response.StatusDescription})"; });
                return;
            }

            await message.ModifyAsync(x =>
            {
                x.Content = "";
                x.Embed = new EmbedBuilder()
                    .WithColor(new Color(255, 80, 80))
                    .WithTitle($"{response.Data.Name}, {response.Data.Sys.Country}")
                    .WithDescription(response.Data.GetHuman())
                    .WithThumbnailUrl($"https://deadbeef.sexy/static/weather-icons/{response.Data.Weather[0].Icon}.png")
                    .WithCurrentTimestamp()
                    .Build();
            });
        }
    }

    public class WeatherApiResponse
    {
        [JsonProperty("coord")] public Coord Coordinates { get; set; }

        [JsonProperty("weather")] public List<Weather> Weather { get; set; }

        [JsonProperty("base")] public string Base { get; set; }

        [JsonProperty("main")] public Main Main { get; set; }

        [JsonProperty("visibility")] public long Visibility { get; set; }

        [JsonProperty("wind")] public Wind Wind { get; set; }

        [JsonProperty("clouds")] public Clouds Clouds { get; set; }

        [JsonProperty("dt")] public long Dt { get; set; }

        [JsonProperty("sys")] public Sys Sys { get; set; }

        [JsonProperty("id")] public long Id { get; set; }

        [JsonProperty("Name")] public string Name { get; set; }

        [JsonProperty("cod")] public long Cod { get; set; }

        public string GetHuman()
        {
            return
                $"Temperature: {Main.Temp}°C\n" +
                $"Pressure: {Main.Pressure} hPa\n" +
                $"Humidity: {Main.Humidity}%\n" +
                $"Wind: {Wind.Speed} km/h\n" +
                $"Minimum: {Main.TempMin}°C, Maximum: {Main.TempMax}°C\n";
        }
    }

    public class Clouds
    {
        [JsonProperty("all")] public long All { get; set; }
    }

    public class Coord
    {
        [JsonProperty("lon")] public double Lon { get; set; }

        [JsonProperty("lat")] public double Lat { get; set; }
    }

    public class Main
    {
        [JsonProperty("temp")] public double Temp { get; set; }

        [JsonProperty("pressure")] public long Pressure { get; set; }

        [JsonProperty("humidity")] public long Humidity { get; set; }

        [JsonProperty("temp_min")] public double TempMin { get; set; }

        [JsonProperty("temp_max")] public double TempMax { get; set; }
    }

    public class Sys
    {
        [JsonProperty("type")] public long Type { get; set; }

        [JsonProperty("id")] public long Id { get; set; }

        [JsonProperty("message")] public double Message { get; set; }

        [JsonProperty("country")] public string Country { get; set; }

        [JsonProperty("sunrise")] public long Sunrise { get; set; }

        [JsonProperty("sunset")] public long Sunset { get; set; }
    }

    public class Weather
    {
        [JsonProperty("id")] public long Id { get; set; }

        [JsonProperty("main")] public string Main { get; set; }

        [JsonProperty("description")] public string Description { get; set; }

        [JsonProperty("icon")] public string Icon { get; set; }
    }

    public class Wind
    {
        [JsonProperty("speed")] public double Speed { get; set; }

        [JsonProperty("deg")] public long Degree { get; set; }
    }
}
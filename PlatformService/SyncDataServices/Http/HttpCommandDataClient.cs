using System.Text;
using System.Text.Json;
using PlatformService.Dtos;

namespace PlatformService.SyncDataServices.Http
{
    public class HttpCommandDataClient : ICommandDataClient
    {
        private readonly HttpClient client;
        private readonly IConfiguration configuration;
        private readonly IHostEnvironment hostEnvironment;

        public HttpCommandDataClient(HttpClient client, IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            this.client = client;
            this.configuration = configuration;
            this.hostEnvironment = hostEnvironment;
        }
        public async Task SendPlatformToCommand(PlatformReadDto platform)
        {
            var httpContent = new StringContent(
                JsonSerializer.Serialize(platform),
                Encoding.UTF8,
                "application/json");

            if (hostEnvironment.IsProduction())
            {
                var response = await client.PostAsync($"{configuration["CommandService"]}", httpContent);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("--> Sync POST to CommandService was OK!");
                }
                else
                {
                    Console.WriteLine("--> Sync POST to CommandService was not OK!");

                }
            }
            else
            {
                var response = await client.PostAsync($"{configuration["CommandServiceLocal"]}", httpContent);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("--> Sync POST to CommandService was OK!");
                }
                else
                {
                    Console.WriteLine("--> Sync POST to CommandService was not OK!");

                }
            }
        }
    }
}
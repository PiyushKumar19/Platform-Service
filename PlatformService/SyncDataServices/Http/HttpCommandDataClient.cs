using System.Text;
using System.Text.Json;
using PlatformService.Dtos;

namespace PlatformService.SyncDataServices.Http
{
    public class HttpCommandDataClient : ICommandDataClient
    {
        private readonly HttpClient client;
        private readonly IConfiguration configuration;

        public HttpCommandDataClient(HttpClient client,  IConfiguration configuration)
        {
            this.client = client;
            this.configuration = configuration;
        }
        public async Task SendPlatformToCommand(PlatformReadDto platform)
        {
            var httpContent = new StringContent(
                JsonSerializer.Serialize(platform),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync($"{configuration.GetSection("CommandServiceHttp")}", httpContent);
            
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("--> Sync POST to CommandService was OK!");
            }
            else{
                Console.WriteLine("--> Sync POST to CommandService was not OK!");

            }
        }
    }
}
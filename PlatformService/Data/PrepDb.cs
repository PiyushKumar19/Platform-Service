namespace PlatformService.Data
{
    public static class PrepDb
    {
        public static void PrepPopulation(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                SendData(serviceScope.ServiceProvider.GetService<AppDbContext>());
            }
        }

        private static void SendData(AppDbContext context)
        {
            if (!context.Platform.Any())
            {
                Console.WriteLine("--> Seeding Data...");
            }
            else
            {
                Console.WriteLine("--> We already have data");
            }
        }
    }
}

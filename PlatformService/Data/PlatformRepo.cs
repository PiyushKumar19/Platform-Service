﻿using PlatformService.Models;

namespace PlatformService.Data
{
    public class PlatformRepo : IPlatformRepo
    {
        private readonly AppDbContext dbContext;

        public PlatformRepo(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public void CreatePlatform(Platform platform)
        {
            if (platform == null)
            {
                throw new ArgumentNullException(nameof(platform));
            }
            dbContext.Platform.Add(platform);
        }

        public IEnumerable<Platform> GetAllPlatforms()
        {
            return dbContext.Platform.ToList();
        }

        public Platform GetPlatfomrById(int id)
        {
            var platform = dbContext.Platform.FirstOrDefault(p=>p.Id == id);
            return (platform  == null) ? null : platform;
        }

        public bool SaveChanges()
        {
            return (dbContext.SaveChanges() >= 0);
        }
    }
}

﻿using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(ForumSystem.Web.Areas.Identity.IdentityHostingStartup))]

namespace ForumSystem.Web.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
            });
        }
    }
}

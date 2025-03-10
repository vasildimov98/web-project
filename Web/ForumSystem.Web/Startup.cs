﻿namespace ForumSystem.Web
{
    using System.Reflection;

    using ForumSystem.Data;
    using ForumSystem.Data.Common;
    using ForumSystem.Data.Common.Repositories;
    using ForumSystem.Data.Models;
    using ForumSystem.Data.Repositories;
    using ForumSystem.Data.Seeding;
    using ForumSystem.Services.Data;
    using ForumSystem.Services.Mapping;
    using ForumSystem.Services.Messaging;
    using ForumSystem.Web.Hub;
    using ForumSystem.Web.ViewModels;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlServer(this.configuration.GetConnectionString("DefaultConnection")));

            services
                .AddDefaultIdentity<ApplicationUser>(IdentityOptionsProvider.GetIdentityOptions)
                .AddRoles<ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.Configure<CookiePolicyOptions>(
                options =>
                    {
                        options.CheckConsentNeeded = context => true;
                        options.MinimumSameSitePolicy = SameSiteMode.None;
                    });

            services.AddControllersWithViews(
                options =>
                    {
                        options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                    }).AddRazorRuntimeCompilation();

            services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-CSRF-TOKEN";
            });

            services.AddSignalR();

            services.AddRazorPages();
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddSingleton(this.configuration);

            services.AddMemoryCache();

            // Data repositories
            services.AddScoped(typeof(IDeletableEntityRepository<>), typeof(EfDeletableEntityRepository<>));
            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
            services.AddScoped<IDbQueryRunner, DbQueryRunner>();

            // Application services
            services.AddTransient<IEmailSender, NullMessageSender>();
            services.AddTransient<ICategoriesService, CategoriesService>();
            services.AddTransient<IPostsService, PostsService>();
            services.AddTransient<IVotesService, VotesService>();
            services.AddTransient<ICommentsService, CommentsService>();
            services.AddTransient<IUsersService, UsersService>();
            services.AddTransient<IChatsService, ChatsService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            AutoMapperConfig.RegisterMappings(typeof(ErrorViewModel).GetTypeInfo().Assembly);

            // Seed data on application startup
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.Migrate();
                new ApplicationDbContextSeeder(this.configuration)
                    .SeedAsync(dbContext, serviceScope.ServiceProvider)
                    .GetAwaiter()
                    .GetResult();
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(
                endpoints =>
                    {
                        endpoints
                          .MapControllerRoute(
                              name: "user-username-page",
                              pattern: "User/{username:required}/{id?}",
                              defaults: new
                              {
                                  controller = "Profiles",
                                  action = "ByUsername",
                              });

                        endpoints
                           .MapControllerRoute(
                               name: "post-id-title",
                               pattern: "Post/{id:min(1)}/{title:required}",
                               defaults: new
                               {
                                   controller = "Posts",
                                   action = "ById",
                               });

                        endpoints
                           .MapControllerRoute(
                               name: "post-edit-page",
                               pattern: "Post/Edit/{id:min(1)}",
                               defaults: new
                               {
                                   controller = "Posts",
                                   action = "Edit",
                               });

                        endpoints
                         .MapControllerRoute(
                             name: "post-delete-page",
                             pattern: "Post/Delete/{id:min(1)}",
                             defaults: new
                             {
                                 controller = "Posts",
                                 action = "Delete",
                             });

                        endpoints
                          .MapControllerRoute(
                              name: "category-create-page",
                              pattern: "Category/Create/{id?}",
                              defaults: new
                              {
                                  controller = "Categories",
                                  action = "Create",
                              });

                        endpoints
                          .MapControllerRoute(
                              name: "category-edit-page",
                              pattern: "Category/Edit/{id:min(1)}",
                              defaults: new
                              {
                                  controller = "Categories",
                                  action = "Edit",
                              });

                        endpoints
                         .MapControllerRoute(
                             name: "category-delete-page",
                             pattern: "Category/Delete/{id:min(1)}",
                             defaults: new
                             {
                                 controller = "Categories",
                                 action = "Delete",
                             });

                        endpoints
                           .MapControllerRoute(
                               name: "category-name-page",
                               pattern: "Category/{name:required}/{id?}",
                               defaults: new
                               {
                                   controller = "Categories",
                                   action = "ByName",
                               });

                        endpoints
                          .MapControllerRoute(
                              name: "categories-username-page",
                              pattern: "Categories/ByOwner/{username:required}/{id?}",
                              defaults: new
                              {
                                  controller = "Categories",
                                  action = "ByOwner",
                              });

                        endpoints
                         .MapControllerRoute(
                             name: "category-live-chat",
                             pattern: "Chat/{name:required}",
                             defaults: new
                             {
                                 controller = "Chats",
                                 action = "LiveChat",
                             });

                        endpoints
                            .MapHub<ChatHub>("/category-chat");

                        endpoints.MapControllerRoute("areaRoute", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                        endpoints.MapControllerRoute("default", "{controller=Posts}/{action=All}/{id?}");
                        endpoints.MapRazorPages();
                    });
        }
    }
}

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediaBin.Data;
using Microsoft.Extensions.FileProviders;
using System.IO;

namespace MediaBin
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        private FileStore fileStore;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            fileStore = new FileStore(Configuration.GetValue<string>("FilesPath"));

            services.AddControllersWithViews();

            services.AddSingleton(fileStore);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.Map("/{file}", async (context) =>
                {
                    var file = fileStore.Files.FirstOrDefault(m => m.FileName == context.Request.RouteValues["file"] as string);
                    if (file != null)
                    {
                        file.Content = await fileStore.ReadAsync(file);
                        context.Response.StatusCode = 200;
                        context.Response.ContentType = file.Type;
                        await context.Response.Body.WriteAsync(file.Content, 0, file.Content.Length);
                        context.Response.Body.Close();
                    }
                });
            });
        }
    }
}

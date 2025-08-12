using AppProject.Controllers;
using AppProject.Modules.FileExplorer;

namespace AppProject
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped<FileExplorerController>();
            builder.Services.AddScoped<IFileExplorerService, FileExplorerService>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.MapControllers();

            app.MapFallback(async context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = 404;
                    return;
                }

                if (context.Request.Path.StartsWithSegments("/swagger"))
                {
                    context.Response.StatusCode = 404;
                    return;
                }

                context.Response.ContentType = "text/html";
                await context.Response.SendFileAsync("wwwroot/index.html");
            });

            app.Run();
        }
    }
}
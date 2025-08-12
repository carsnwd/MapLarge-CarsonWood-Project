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

            // Configure file upload limits
            builder.Services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseDefaultFiles(new DefaultFilesOptions
            {
                DefaultFileNames = new List<string> { "index.html" }
            });
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthorization();
            app.MapControllers();

            app.MapFallback(async context =>
            {
                if (context.Request.Path.StartsWithSegments("/api") ||
                    context.Request.Path.StartsWithSegments("/swagger"))
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
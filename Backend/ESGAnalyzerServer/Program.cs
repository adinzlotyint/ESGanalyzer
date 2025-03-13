using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Server.IIS;
using Microsoft.OpenApi.Models;

namespace ESGAnalyzerServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddHttpLogging(opts =>
                opts.LoggingFields = HttpLoggingFields.RequestProperties);
            builder.Services.AddSwaggerGen();
            builder.Services.AddControllers();
            builder.Services.AddScoped<UploadService>();

            builder.Logging.AddFilter("Microsoft.AspNetCore.HttpLogging", LogLevel.Information);

            var app = builder.Build();

            if (app.Environment.IsDevelopment()) {
                app.UseHttpLogging();
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                    options.RoutePrefix = string.Empty;
                });
            }
            app.UseRouting();
            app.MapControllers();
            app.Run();
        }
    }
}

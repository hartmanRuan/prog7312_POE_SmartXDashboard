
using Microsoft.EntityFrameworkCore;
using SmartX.Api.Data;

namespace SmartX.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // Configure Swagger to handle generic types properly
            builder.Services.AddSwaggerGen(c =>
            {
                c.CustomSchemaIds(type => type.ToString());
            });


            builder.Services.AddDbContext<SmartXDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Combine all controller and JSON options into ONE block:
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                });
            var app = builder.Build();

            // Enable Swagger UI in Development
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartX API V1");
                    c.RoutePrefix = "swagger"; // Opens Swagger at http://localhost:5000/swagger
                });
            }

            app.UseRouting();
            app.UseAuthorization();
            app.MapControllers();

            app.Run("http://localhost:5000");
        }
    }
}

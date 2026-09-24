
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Purchase_Sales_API.ErrorHandlingMiddleware;
using Purchase_Sales_Core.CoreDIContainer;
using Purchase_Sales_Core.Validators;
using Purchase_Sales_Infrastructure.Context;
using Purchase_Sales_Infrastructure.InfrastructureDIContainer;

namespace Purchase_Sales_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            // FluentValidation — validators live in the Core project
            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddValidatorsFromAssemblyContaining<PurchaseFileDTOValidator>();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //AddInfrastructureServices
            builder.Services.AddInfrastructureServices(builder.Configuration);
            //AddCoreServices
            builder.Services.AddCoreServices(builder.Configuration);
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = 300_000_000 ; 
            });
            builder.Services.Configure<FormOptions>(options =>
            {
                options.ValueLengthLimit = 300_000_000;
                options.MultipartBodyLengthLimit = 300_000_000;
            });

            // CORS — allow the React client (via Nginx on :3000) and direct browser access
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins("http://localhost:3000")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            var app = builder.Build();

            // Auto-apply EF Core migrations on startup (with retry for Docker cold-start)
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var retries = 5;
                for (int i = 0; i < retries; i++)
                {
                    try
                    {
                        db.Database.Migrate();
                        Console.WriteLine("Database migrated successfully.");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Migration attempt {i + 1}/{retries} failed: {ex.Message}");
                        if (i == retries - 1) throw;
                        Thread.Sleep(5000);
                    }
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseCustomErrorHandlingMiddleware();
            app.UseCors();
            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}

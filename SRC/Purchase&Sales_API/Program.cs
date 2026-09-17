
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Http.Features;
using Purchase_Sales_API.ErrorHandlingMiddleware;
using Purchase_Sales_Core.CoreDIContainer;
using Purchase_Sales_Core.Validators;
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

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseCustomErrorHandlingMiddleware();
            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}

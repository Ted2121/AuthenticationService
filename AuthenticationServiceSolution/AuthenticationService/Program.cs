using AuthenticationService.Data;
using AuthenticationService.Extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace AuthenticationService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddDbContext<AppDbContext>(options => options.UseCosmos(
            builder.Configuration["Users:AccountEndpoint"],
            builder.Configuration["Users:AccountKey"],
            builder.Configuration["Users:Database"]
            ));
        builder.Services.AddTransient<IUserRepository, UserRepository>();

        builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration));

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.ConfigureExceptionHandler(app.Logger);

        app.UseSerilogRequestLogging();

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}

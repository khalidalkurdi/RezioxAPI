using CloudinaryDotNet;
using DataAccess.ExternalcCloud;
using DataAccess.PublicClasses;
using Microsoft.EntityFrameworkCore;
using Reziox.DataAccess;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("SomeeConnections")));

//inject nececcry interfaces services
builder.Services.AddScoped<ICloudImag, CloudImage>();
builder.Services.AddScoped<INotificationService, NotificationService>();

//config cloudinary
builder.Services.AddSingleton(x =>
{
    var config = builder.Configuration.GetSection("Cloudinary");
    return new Cloudinary(new Account(
        //as array
        config["CloudName"],
        config["ApiKey"],
        config["ApiSecret"]
    ));
}
);
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Global API v1");
        c.RoutePrefix = string.Empty; //Swagger UI to be the root URL
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

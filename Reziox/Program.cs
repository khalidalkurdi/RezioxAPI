using CloudinaryDotNet;
using DataAccess.ExternalcCloud;
using DataAccess.PublicClasses;
using DataAccess.Services;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Model.Configuration;
using Reziox.DataAccess;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("SomeeConnections")));

// service interfaces 
builder.Services.AddScoped<ICloudImag, CloudImage>();
builder.Services.AddScoped<INotificationService, NotificationService>();

//smtp configuration
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

//config cloudinary
builder.Services.AddSingleton(x =>
{
    //it is array
    var config = builder.Configuration.GetSection("Cloudinary");
    return new Cloudinary(new Account(
        
        config["CloudName"],
        config["ApiKey"],
        config["ApiSecret"]
    ));
}
);
builder.Services.AddControllers();

//bush notifaction
FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromFile("FirebaseKey.json")
});

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

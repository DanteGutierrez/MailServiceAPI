using MailServiceAPI.Data;
using MailServiceAPI;
using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore.Extensions;
using MailServiceAPI.MessageQueue;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("Secret"));

//builder.Services.AddAuthentication();
//builder.Services.AddAuthorization();

builder.Services.AddScoped<IMailNotificationProducer, MailNotificationProducer>();
builder.Services.AddHostedService<MailNotificationConsumer>();
builder.Services.AddFluentEmail("dgutierrez@student.neumont.edu").AddSmtpSender("smtp.sendgrid.net", 2525, "apikey", "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");

builder.Services.AddEntityFrameworkMySQL().AddDbContext<MailDBContext>(options => { options.UseMySQL(builder.Configuration.GetConnectionString("mySQL")); });

builder.Services.AddStackExchangeRedisCache(options => { options.Configuration = "localhost:6379"; });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//app.UseAuthorization();

app.UseMiddleware<JwtMiddleware>();

app.MapControllers();

app.Run();

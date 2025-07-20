using Microsoft.EntityFrameworkCore;
using Quartz;
using Telegram.Bot;
using TelegramBot.Application.Jobs;
using TelegramBot.Infrastructure.Contexts;
using TelegramBot.WebAPI.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddPostgreConfiguration(builder.Configuration);
builder.Services.AddServiceConfiguration();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddQuartz(q => 
{
    q.UseMicrosoftDependencyInjectionJobFactory();
    q.AddJob<PeriodicMessageJob>(cfg => cfg
        .StoreDurably()
        .WithIdentity("PeriodicMessageJob"));
});

builder.Services.AddQuartzHostedService(opt =>
{
    opt.WaitForJobsToComplete = true;
});
builder.Services.AddHttpClient("telegram_bot_client")
    .AddTypedClient<ITelegramBotClient>(httpClient =>
    {
        var botToken = Environment.GetEnvironmentVariable("TELEGRAM_TOKEN");
        if (string.IsNullOrEmpty(botToken))
        {
            throw new InvalidOperationException("Переменная окружения TELEGRAM_TOKEN не установлена");
        }
        return new TelegramBotClient(botToken, httpClient);
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}


app.UseAuthorization();
app.MapControllers();

app.Run();


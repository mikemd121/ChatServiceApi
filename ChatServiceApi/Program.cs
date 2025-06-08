using Application.Implementation;
using Application.Interfaces;
using ChatServiceApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ISupportAgentProvider, SupportAgentProvider>();
builder.Services.AddSingleton<IChatQueueService,InMemoryChatQueueService>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

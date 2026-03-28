using Soteo.MasterServer;
using Soteo.MasterServer.GameState.Repositories;
using Soteo.MasterServer.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Host.ConfigureHostOptions(options => options.ShutdownTimeout = TimeSpan.FromSeconds(5));

if (builder.Environment.IsDevelopment())
{
    builder.Services
        .AddCors(options => options.AddDefaultPolicy(policy => policy.SetIsOriginAllowed(o => new Uri(o).IsLoopback)));
}

builder.Services.AddSingleton<IWebSocketRepository, WebSocketRepository>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<ICharacterRepository, CharacterRepository>();
builder.Services.AddSingleton<IMessageSender, WebSocketMessageSender>();
builder.Services.AddSingleton<IDispatcher, Dispatcher>();

foreach (Type type in TypeLocator.MessageHandlerTypes.Values) builder.Services.AddTransient(type);

var app = builder.Build();

app.UseCors();
app.UseWebSockets();

if (app.Environment.IsDevelopment()) app.MapOpenApi();
app.MapControllers();

var dispatcher = (Dispatcher)app.Services.GetRequiredService<IDispatcher>();
app.Run();
dispatcher.ShutDown();
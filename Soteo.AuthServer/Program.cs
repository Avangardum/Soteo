using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Soteo.AuthServer.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration["Soteo:AuthServerConnectionString"] ??
    throw new InvalidOperationException("Soteo:AuthServerConnectionString configuration value is not set");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();
builder.Services.AddControllers();

if (builder.Environment.IsDevelopment())
{
    builder.Services
        .AddCors(options => options.AddDefaultPolicy(policy => policy.SetIsOriginAllowed(o => new Uri(o).IsLoopback)));
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();
app.MapControllers();

app.Run();
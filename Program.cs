using PokemonBinder.Data;
using PokemonBinder.Repositories;
using PokemonBinder.Services;

var builder = WebApplication.CreateBuilder(args);

// Database path
string dbPath = Path.Combine(AppContext.BaseDirectory, "pokemon_binder.db");

// Add services to the container
builder.Services.AddControllersWithViews();

// Register data access
builder.Services.AddSingleton(new ConnectionManager(dbPath));
builder.Services.AddScoped<ICardRepository, CardRepository>();
builder.Services.AddScoped<IBinderRepository, BinderRepository>();
builder.Services.AddScoped<CardService>();
builder.Services.AddScoped<BinderService>();

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var initializer = new DatabaseInitializer(dbPath);
    await initializer.InitializeAsync();
    Console.WriteLine($"Database initialized at: {dbPath}");
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
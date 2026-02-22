using CSTune.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// PostgreSQL Connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Add PostgreSQL support
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Helpful developer exceptions for DB
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Identity configuration
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();

var app = builder.Build();

// Retry loop for DB connection + migrations
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();

    const int maxRetries = 3;
    int retryCount = 0;
    bool dbReady = false;

    while (!dbReady && retryCount < maxRetries)
    {
        try
        {
            Console.WriteLine($"Attempting DB migrate (try {retryCount + 1}/{maxRetries})...");
            db.Database.Migrate();
            dbReady = true;
        }
        catch (Exception ex)
        {
            retryCount++;
            Console.WriteLine($"DB not ready yet: {ex.Message}");
            Thread.Sleep(5000); // 5s pause before retry
        }
    }

    if (!dbReady)
    {
        throw new Exception("Could not connect to the database after multiple retries.");
    }
}

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Map Razor Pages
app.MapRazorPages();

app.Run();
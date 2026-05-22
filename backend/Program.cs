using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Npgsql;

const string FrontendPolicyName = "FrontendPolicy";

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no está configurada.");

var dataSource = BuildDataSource(connectionString);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(dataSource));

AddApplicationDependencies(builder.Services);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options => options.AddPolicy(FrontendPolicyName, policy => ConfigureFrontendCors(policy)));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(FrontendPolicyName);

app.UseAuthorization();
app.MapControllers();

app.Run();

static NpgsqlDataSource BuildDataSource(string connectionString)
{
    var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
    dataSourceBuilder.MapEnum<BookingStatus>("booking_status");
    return dataSourceBuilder.Build();
}

static void AddApplicationDependencies(IServiceCollection services)
{
    services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

    services.AddScoped<GuestRepository>();
    services.AddScoped<GuestService>();
    
    services.AddScoped<BookingRepository>();
    services.AddScoped<BookingService>();
    
    services.AddScoped<RoomRepository>();
    services.AddScoped<RoomService>();
    
    services.AddScoped<RoomTypeRepository>();
    services.AddScoped<RoomTypeService>();
    
    services.AddScoped<ServiceContactRepository>();
    services.AddScoped<ServiceContactService>();
}

static void ConfigureFrontendCors(CorsPolicyBuilder policy)
{
    policy
        .WithOrigins("http://127.0.0.1:5500", "http://localhost:5500")
        .AllowAnyHeader()
        .AllowAnyMethod();
}
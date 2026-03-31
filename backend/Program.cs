using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Npgsql;

const string FrontendPolicyName = "FrontendPolicy";

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no está configurada.");

var dataSource = BuildDataSource(connectionString);

// Configurar la conexión a PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(dataSource));

// Capas de la aplicación (inyección de dependencia)
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
    services.AddSingleton<IRoomTypePresetCreator, SimpleRoomTypePresetCreator>();
    services.AddSingleton<IRoomTypePresetCreator, SuiteRoomTypePresetCreator>();
    services.AddSingleton<IRoomTypePresetCreator, DoubleTwinRoomTypePresetCreator>();
    services.AddSingleton<IRoomTypePresetCreator, DoubleMatrimonialRoomTypePresetCreator>();
    services.AddScoped<IGuestRepository, GuestRepository>();
    services.AddScoped<IGuestService, GuestService>();
    services.AddScoped<IBookingRepository, BookingRepository>();
    services.AddScoped<IBookingService, BookingService>();
    services.AddScoped<IRoomRepository, RoomRepository>();
    services.AddScoped<IRoomService, RoomService>();
    services.AddScoped<IRoomTypeRepository, RoomTypeRepository>();
    services.AddScoped<IRoomTypeService, RoomTypeService>();
    services.AddScoped<IServiceContactRepository, ServiceContactRepository>();
    services.AddScoped<IServiceContactService, ServiceContactService>();
}

static void ConfigureFrontendCors(CorsPolicyBuilder policy)
{
    policy
        .WithOrigins("http://127.0.0.1:5500", "http://localhost:5500")
        .AllowAnyHeader()
        .AllowAnyMethod();
}
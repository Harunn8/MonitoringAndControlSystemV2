using McsAPI.Controllers;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MongoDB.Driver;
using Services;
using Serilog;
using MQTTnet;
using MQTTnet.Client;
using EventBusMqtt.Connection.Base;
using EventBusMqtt.Connection;
using EventBusMqtt.Producer;
using MQTTnet.Client.Options;
using McsApplication.Mapper;
using McsCore.Entities;
using McsInfrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using McsApplication.Services.Base;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
Log.Information("Logger started");
builder.Host.UseSerilog();

//Log.Information("Mqtt connection is preparing...");
//var mqttOptions = new MqttClientOptionsBuilder()
//    .WithClientId("telemetry")
//    .WithTcpServer("127.0.0.1", 1883)
//    .WithCleanSession()
//    .Build();

//builder.Services.AddSingleton<IMqttClientOptions>(mqttOptions);
//builder.Services.AddSingleton<IMqttClient>(_ => new MqttFactory().CreateMqttClient());

//builder.Services.AddSingleton<IMqttConnection>(sp =>
//{
//    var clientOptions = sp.GetRequiredService<IMqttClientOptions>();
//    var client = sp.GetRequiredService<IMqttClient>();
//    return new MqttConnection(clientOptions, client);
//});

//builder.Services.AddSingleton<MqttProducer>();

builder.Services.AddSingleton<IMqttClient>(_ => new MqttFactory().CreateMqttClient());
builder.Services.AddSingleton<IMqttClientOptions>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    return new MqttClientOptionsBuilder()
        .WithClientId("telemetry")
        .WithTcpServer(configuration["EventBusMqtt:Server"], int.Parse(configuration["EventBusMqtt:Port"]))
        .WithCleanSession()
        .Build();
});

builder.Services.AddScoped<IMqttConnection, MqttConnection>();
builder.Services.AddScoped<MqttProducer>();




Log.Information("Mqtt connection was establish");

Log.Information("MongoDB connection is preparing...");
var mongoDbSettings = configuration.GetSection("MongoDbSettings").Get<MongoDBSettings>();
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = configuration["ConnectionStrings:MongoDb"];
    return new MongoClient(settings);
});
builder.Services.AddSingleton(sp =>
{
    var mongoClient = sp.GetRequiredService<IMongoClient>();
    return mongoClient.GetDatabase(mongoDbSettings.DatabaseName);
});

var seedService = builder.Services.AddScoped<McsContextSeed>();

Log.Information("MongoDB connection was establish");

builder.Services.AddScoped<SnmpService>();
builder.Services.AddScoped<TcpService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ISnmpDeviceService, SnmpDeviceService>();
builder.Services.AddScoped<SnmpDeviceService>();
builder.Services.AddScoped<WebSocketHandlerSnmp>();
builder.Services.AddScoped<WebSocketHandlerTcp>();
builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<CancellationTokenSource>();


//builder.Services.AddScoped<DeviceDataService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"])),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Snmp Communication",
        Version = "v1",
        Description = "A simple example ASP.NET Core Web API"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header. Example: \"Bearer {token}\""
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(McsMappingProfile));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Snmp Communication");
        c.RoutePrefix = string.Empty;
    });
}

app.UseStaticFiles();

app.UseWebSockets();

app.UseRouting();

app.UseCors("AllowAllOrigins");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Map("/ws/snmp", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var webSocketHandlerSnmp = app.Services.GetRequiredService<WebSocketHandlerSnmp>();
        await webSocketHandlerSnmp.HandleAsyncSnmp(context, webSocket);
        Log.Information("Snmp socket started");
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

app.Map("/ws/tcp", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var webSocketHandlerTcp = app.Services.GetRequiredService<WebSocketHandlerTcp>();
        await webSocketHandlerTcp.HandleAsyncTcp(context, webSocket);
        Log.Information("Tcp socket started");
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

app.Run();

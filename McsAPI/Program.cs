using McsAPI.Controllers;
using McsApplication.Services;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MongoDB.Driver;
using Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using Serilog;
using MQTTnet.Client;
using EventBusMqtt.Connection.Base;
using EventBusMqtt.Connection;
using EventBusMqtt.Producer;
using MQTTnet;
using McsApplication.Models;
using AutoMapper;
using McsApplication.Mapper;
using EventBusMqtt.Connection.Base;
using EventBusMqtt.Producer;
using McsAPI.Controllers;
using McsCore.Entities;
using MQTTnet.Client.Options;
using Services.Base;
using McsInfrastructure.Data;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

Log.Information("Logger is configured and started");

Log.Information("Mqtt Connection preparing...");

var mqttOptions = new MqttClientOptionsBuilder()
    .WithClientId("telemetry")
    .WithTcpServer("127.0.0.1", 1883)
    .WithCleanSession()
    .Build();

builder.Services.AddSingleton<IMqttClient>(_ => new MqttFactory().CreateMqttClient());
builder.Services.AddSingleton<IMqttConnection>(_ => new MqttConnection(mqttOptions, new MqttFactory().CreateMqttClient()));
builder.Services.AddSingleton<MqttProducer>();

builder.Services.AddSingleton<ISnmpService, SnmpService>();
builder.Services.AddTransient<WebSocketHandlerSnmp>();
builder.Services.AddTransient<WebSocketHandlerTcp>();

Log.Information("MongoDB connection preparing is started");
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

builder.Services.AddScoped<McsContextSeed>();
builder.Services.AddScoped<SnmpParserService>();
builder.Services.AddScoped<TcpService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<SnmpDeviceService>();
builder.Services.AddScoped<TcpService>();
builder.Services.AddScoped<SnmpService>();

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

using (var scope = app.Services.CreateScope())
{
    var seedService = scope.ServiceProvider.GetRequiredService<McsContextSeed>();
    await seedService.UserSeedAsync();
    await seedService.DeviceSeedAsync();
}

app.MapControllers();

app.Map("/ws/snmp", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var webSocketHandlerSnmp = app.Services.GetRequiredService<WebSocketHandlerSnmp>();
        await webSocketHandlerSnmp.HandleAsyncSnmp(context, webSocket);
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
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

app.Run();

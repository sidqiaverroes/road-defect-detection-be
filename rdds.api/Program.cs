using System.Security.Claims;
using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using rdds.api.Data;
using rdds.api.Interfaces;
using rdds.api.Models;
using rdds.api.Repositories;
using rdds.api.Services;
using rdds.api.Services.Calculation;
using rdds.api.Services.MQTT;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "RDDS API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options => {
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultPostgresConnection"));
});

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// AUTH SECTION START
builder.Services.AddIdentity<AppUser, IdentityRole>(options => {
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;

})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
    options.DefaultChallengeScheme =
    options.DefaultForbidScheme =
    options.DefaultScheme =
    options.DefaultSignInScheme =
    options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"])
        )
    };
});

builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("Admin", policy =>
        {
            policy.RequireRole("Admin");
        });
    });
// AUTH SECTION END

builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
builder.Services.AddScoped<IAttemptRepository, AttemptRepository>();
builder.Services.AddScoped<IRoadDataRepository, RoadDataRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
// builder.Services.AddScoped<IAccessTypeRepository, AccessTypeRepository>();
builder.Services.AddScoped<IRoadCategoryRepository, RoadCategoryRepository>();
builder.Services.AddScoped<ICalculatedDataRepository, CalculatedDataRepository>();
builder.Services.AddSingleton<CalculationService>();
builder.Services.AddSingleton(sp =>
{
    var options = new HiveMQClientOptions
    {
        Host = builder.Configuration["MQTT:Host"],
        Port = int.Parse(builder.Configuration["MQTT:Port"]),
        UserName = builder.Configuration["MQTT:Username"],
        Password = builder.Configuration["MQTT:Password"]
    };
    return new HiveMQClient(options);
});
builder.Services.AddSingleton<MqttService>();
builder.Services.AddHostedService<MqttService>();


var app = builder.Build();

// Initialize database method
DbInitializer.InitializeDb(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.UseWebSockets();

app.MapControllers();


app.Run();



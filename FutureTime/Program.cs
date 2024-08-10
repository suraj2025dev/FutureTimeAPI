
using AegisService.Lib.Filters;
using FutureTime;
using FutureTime.Filters;
using Auth;
using DbUp;
using Library.Data;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Npgsql;
using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;
using static Org.BouncyCastle.Math.EC.ECCurve;
using FutureTime.MongoDB;
using Microsoft.Extensions.Options;
using FutureTime.StaticData;

var isService = false;

//when the service start we need to pass the --service parameter while running the .exe
if (Debugger.IsAttached == false && args.Contains("--service"))
{
    isService = true;
}
if (isService)
{
    System.IO.Directory.SetCurrentDirectory(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
}

var builder = WebApplication.CreateBuilder(args);

#region Migration
var path = Path.Combine(Directory.GetCurrentDirectory(), "Configuration.json");

if (File.Exists(path))
{
    // Read entire text file content in one string    
    string text = File.ReadAllText(path);
    
    AppStatic.CONFIG = JsonConvert.DeserializeObject<Configuration>(text);
}


//var settings = builder.Configuration.GetSection("App:Database:HOST");//.Get<Configuration>();
//AppStatic.CONFIG=settings; 

var connectionString = "Server=" + AppStatic.CONFIG.App.Database.HOST + ";Port=" + AppStatic.CONFIG.App.Database.PORT + ";Database=" + AppStatic.CONFIG.App.Database.NAME + ";User Id=postgres;Password=" + AppStatic.CONFIG.App.Database.PASSWORD + ";Timeout=1024;";
AppStatic.DB_CONN = "Server=" + AppStatic.CONFIG.App.Database.HOST + ";Port=" + AppStatic.CONFIG.App.Database.PORT + ";Database=" + AppStatic.CONFIG.App.Database.NAME + ";User Id=postgres;Password=" + AppStatic.CONFIG.App.Database.PASSWORD;
if (true)
{
    try
    {
        using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
        {
            conn.Open();
            // Define a query
            using (NpgsqlCommand command = new NpgsqlCommand(@"
SELECT pg_terminate_backend(pg_stat_activity.pid)
FROM pg_stat_activity
WHERE pg_stat_activity.datname = '" + AppStatic.CONFIG.App.Database.NAME + @"' -- ? change this to your DB
 AND pid <> pg_backend_pid();
", conn))
            {
                NpgsqlDataReader dr = command.ExecuteReader();

            }
        }
    }
    catch (Exception ex)
    {

    }
}


//try
//{
//    using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
//    {
//        conn.Open();
//        // Define a query
//        using (NpgsqlCommand command = new NpgsqlCommand(@"
//--view & fn to delete 
//", conn))
//        {
//            NpgsqlDataReader dr = command.ExecuteReader();

//        }
//    }
//}
//catch (Exception ex)
//{

//}

EnsureDatabase.For.PostgresqlDatabase(connectionString);

var upgrader =
    DeployChanges.To
        .PostgresqlDatabase(connectionString)
        .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
        .WithVariablesDisabled()
        .WithExecutionTimeout(TimeSpan.FromSeconds(1800))
        .LogToConsole()
        .Build();

var result = upgrader.PerformUpgrade();


if (!result.Successful)
{
    Console.WriteLine("Error in migration");
    return;
}

#endregion


// Add services to the container.
builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});


//builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDB"));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "1.0.0",
        Title = "Aegis Pulse",
        Description = "Aegis Pulse for multi property connectivity",
        Contact = new OpenApiContact
        {
            Name = "Aegis Software",
            Email = "info.aegissoftware@gmail.com"
        }
    });



    //First we define the security scheme
    c.AddSecurityDefinition("Bearer", //Name the security scheme
        new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme.",
            Type = SecuritySchemeType.Http, //We set the scheme type to http since we're using bearer authentication
            Scheme = "bearer" //The name of the HTTP Authorization scheme to be used in the Authorization header. In this case "bearer".
        });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement{
                    {
                        new OpenApiSecurityScheme{
                            Reference = new OpenApiReference{
                                Id = "Bearer", //The name of the previously defined security scheme.
                                Type = ReferenceType.SecurityScheme
                            }
                        },new List<string>()
                    }
                });

    // Set the comments path for the Swagger JSON and UI.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

});

builder.Services.Inject();

builder.Services.AddMvc(option => {
    option.Filters.Add(typeof(ApplicationAuthorizationFilter));//Adding authorization to all controllers.
    option.Filters.Add(typeof(RequestResponseFilterAttribute));//Adding authorization to all controllers.
    
    option.EnableEndpointRouting = false;
});//.AddNewtonsoftJson();

var app = builder.Build();

app.UseCors("AllowAll");


// Configure the HTTP request pipeline.
if (true)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

FeedStaticData.Feed();



app.MapControllers();

app.Run();


using AegisService.Lib.Filters;
using FutureTime;
using FutureTime.Filters;
using FutureTime.Service;
using FutureTime.StaticData;
using Library.Data;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Reflection;

var isService = false;

//when the service start we need to pass the --service parameter while running the .exe
if (Debugger.IsAttached == false && args.Contains("--service"))
{
    isService = true;
}
if (isService)
{
    System.IO.Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
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
#endregion

builder.Services.AddSingleton<FirebaseService>();
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
var firebase = app.Services.GetRequiredService<FirebaseService>();

// Configure the HTTP request pipeline.
if (true)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

FeedStaticData.Feed();

app.UseMiddleware<ExceptionHandlingMiddleware>(); // Register the middleware

app.MapControllers();

app.Run();

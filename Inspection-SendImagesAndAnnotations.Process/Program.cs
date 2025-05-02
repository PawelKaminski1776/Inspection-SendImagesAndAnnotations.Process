using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.Globalization;
using InspectionSendImagesAndAnnotations.Handlers;
using InspectionSendImagesAndAnnotations.Controllers.DtoFactory;
using InspectionSendImagesAndAnnotations.Process;
using InspectionSendImagesAndAnnotations.Channel.Services;
using InspectionSendImagesAndAnnotations.Messages.Dtos;
using InspectionSendImagesAndAnnotations.Channel;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<AppConfiguration>(_ => AppConfiguration.Instance);
builder.Services.AddSingleton<IDtoFactory, DtoFactory>();

var appConfig = AppConfiguration.Instance;

builder.Services.AddScoped<MongoConnect>(provider =>
{
    var connectionString = appConfig.GetSetting("ConnectionStrings:DefaultConnection");
    return new MongoConnect(connectionString);
});
builder.Services.AddScoped<InspectionService>(provider =>
{
    var connectionString = appConfig.GetSetting("ConnectionStrings:DefaultConnection");
    return new InspectionService(connectionString);
});

builder.Services.AddScoped<PythonAPI>(provider =>
{
    var pythonApi = appConfig.GetSetting("PythonAPI");
    var username = appConfig.GetSetting("Username");
    var password = appConfig.GetSetting("Password");
    return new PythonAPI(pythonApi, username, password);
});

builder.Services.AddScoped<MyHandler>();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var endpointConfiguration = new EndpointConfiguration("NServiceBusHandlers");
string instanceId = Environment.MachineName;
endpointConfiguration.MakeInstanceUniquelyAddressable(instanceId);
endpointConfiguration.EnableCallbacks();

var settings = new JsonSerializerSettings
{
    TypeNameHandling = TypeNameHandling.Auto,
    Converters =
    {
        new IsoDateTimeConverter
        {
            DateTimeStyles = DateTimeStyles.RoundtripKind
        }
    }
};
var serialization = endpointConfiguration.UseSerialization<NewtonsoftJsonSerializer>();
serialization.Settings(settings);

var transport = endpointConfiguration.UseTransport<LearningTransport>();
var persistence = endpointConfiguration.UsePersistence<LearningPersistence>();

if (builder.Environment.IsDevelopment())
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(5013);
        options.ListenAnyIP(5014, listenOptions =>
        {
            listenOptions.UseHttps();
        });
        transport.StorageDirectory("/app/.learningtransport");
    });
}
else
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(5013);
    });
    transport.StorageDirectory("/home/ubuntu/storage");

}

var routing = transport.Routing();
routing.RouteToEndpoint(typeof(InspectionRequest), "NServiceBusHandlers");

var scanner = endpointConfiguration.AssemblyScanner().ScanFileSystemAssemblies = true;

builder.UseNServiceBus(endpointConfiguration);

var app = builder.Build();
app.UseRouting();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = string.Empty;
    });
}
app.UseCors("AllowAll");
app.UseMiddleware<LoggingMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();

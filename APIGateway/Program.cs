using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using MMLib.SwaggerForOcelot.DependencyInjection;
using MMLib.SwaggerForOcelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Load BOTH configs
builder.Configuration
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddJsonFile("ocelot.SwaggerEndPoints.json", optional: false, reloadOnChange: true);

// Swagger base (required)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//  Ocelot
builder.Services.AddOcelot(builder.Configuration);

//  Swagger aggregation
builder.Services.AddSwaggerForOcelot(builder.Configuration);

var app = builder.Build();

//  Swagger UI for Gateway
app.UseSwaggerForOcelotUI(opt =>
{
    opt.PathToSwaggerGenerator = "/swagger/docs";
    
});

//  Ocelot middleware
await app.UseOcelot();

app.Run();
using Company.Webshop.Main.Modules;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddModules(builder.Configuration);

WebApplication app = builder.Build();
app.UseExceptionHandler();
app.UseDefaultFiles();
app.UseStaticFiles();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.MapModules();
app.MapHealthChecks("/health");

await app.Services.InitializeModuleDatabases();
await app.RunAsync();

public partial class Program;

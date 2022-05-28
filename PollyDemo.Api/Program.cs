var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
services.AddControllers();

var app = builder.Build();
app.UseRouting();
app.UseEndpoints(config => { config.MapControllers(); });
app.Run();

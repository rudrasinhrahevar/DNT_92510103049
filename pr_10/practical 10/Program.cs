using StudentManagementApi.Data;
using StudentManagementApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<StudentRepository>();

var app = builder.Build();

app.UseMiddleware<RequestIdMiddleware>();
app.UseMiddleware<ApiExceptionMiddleware>();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

await app.Services.GetRequiredService<StudentRepository>().InitializeAsync();

app.Run();

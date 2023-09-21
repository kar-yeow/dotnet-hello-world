var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var what = System.Environment.GetEnvironmentVariable("HELLO_VAR");
app.MapGet("/", () => $"Hello World {what}!");
app.MapGet("/hello/{name}", (string name) => new 
{
    Name = name
});

app.Run();
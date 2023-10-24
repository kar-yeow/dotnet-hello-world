var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var what = System.Environment.GetEnvironmentVariable("HELLO_VAR");
app.MapGet("/", () => $"Hello World {what}!");
app.MapGet("/hello/{name}", (string name) => new 
{
    Name = name
});
var path = Directory.GetCurrentDirectory();
Console.WriteLine(path);
var files = Directory.GetFiles(".");
foreach (var file in files)
{
    Console.WriteLine(file);
}


app.Run();
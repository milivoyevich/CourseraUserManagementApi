using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLogging();
var app = builder.Build();

// Middleware for logging requests
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path}");
    await next();
});

// Middleware for error handling
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError($"Unhandled Exception: {ex.Message}");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = ex.Message }));
    }
});

// In-memory user storage
var users = new List<User>();

//Get all
app.MapGet("/users", (HttpContext context) =>
{
    return users;
});
// Create User
app.MapPost("/users", (HttpContext context, User user) =>
{
    if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Email))
    {
        context.Response.StatusCode = 400;
        return context.Response.WriteAsync(JsonSerializer.Serialize(new { error = "Username and email are required" }));
    }

    user.Id = users.Count + 1;
    users.Add(user);
    context.Response.StatusCode = 201;
    return context.Response.WriteAsync(JsonSerializer.Serialize(user));
});

// Get User by ID
app.MapGet("/users/{id:int}", (HttpContext context, int id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    if (user == null)
    {
        context.Response.StatusCode = 404;
        return context.Response.WriteAsync(JsonSerializer.Serialize(new { error = "User not found" }));
    }
    return context.Response.WriteAsync(JsonSerializer.Serialize(user));
});

// Delete User
app.MapDelete("/users/{id:int}", (HttpContext context, int id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    if (user == null)
    {
        context.Response.StatusCode = 404;
        return context.Response.WriteAsync(JsonSerializer.Serialize(new { error = "User not found" }));
    }

    users.Remove(user);
    return context.Response.WriteAsync(JsonSerializer.Serialize(new { message = "User deleted" }));
});

app.Run();

record User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
}
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
using CourseraUserManagementApi.Models;
using CourseraUserManagementApi.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLogging();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});
var app = builder.Build();

app.UseCors("AllowAll");
app.UseMiddleware<LoggingMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();

// In-memory user storage
var users = MockData.Users; // new List<User>();

//Get all
app.MapGet("/users", (HttpContext context) =>
{
    return context.Response.WriteAsync(JsonSerializer.Serialize(users));
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

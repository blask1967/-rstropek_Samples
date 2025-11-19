using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.WebSockets;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.AddServiceDefaults();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services.AddWebSockets(options => { });

var app = builder.Build();

app.UseCors();
app.UseHttpsRedirection();
app.UseWebSockets();
app.MapOpenApi();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "v1"));

app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();

        // Note the use of WebSocketStream. The stream abstraction on top of WebSocket
        // is brand new in .NET 10/C# 14 and makes working with WebSockets much easier.
        // It allows you to use StreamReader/StreamWriter or any other stream-based API
        // on top of WebSockets.
        using var stream = WebSocketStream.Create(webSocket, WebSocketMessageType.Text, true);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        string? message;
        while ((message = await reader.ReadLineAsync()) != null)
        {
            // Here we are using System.Text.Json to serialize a JSON object.
            // We make use of the new WebSocketStream to write directly to the WebSocket.
            await JsonSerializer.SerializeAsync(stream, new { echo = message });
            if (message.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }   
        }
    }
    else
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
    }
});

app.Run();

using GrpcServer.Services;
using GrpcServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using System.Net;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Any, 5001, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
    options.Listen(IPAddress.Any, 5002, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1;
    });
});
builder.Services.AddSingleton<MathAlgorithms>();
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddCors(o => o.AddPolicy("AllowAll", builder =>
{
    builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
}));

var app = builder.Build();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

IWebHostEnvironment env = app.Environment;
if (env.IsDevelopment()) { app.MapGrpcReflectionService(); }

app.UseGrpcWeb(new GrpcWebOptions {  DefaultEnabled = true,  });
app.UseCors();

app.MapGrpcService<GreeterService>().RequireCors("AllowAll");
app.MapGrpcService<MathGuruService>().RequireCors("AllowAll");

app.MapGet("/", async context =>
{
    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
});

app.Run();

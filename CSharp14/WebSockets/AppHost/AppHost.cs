var builder = DistributedApplication.CreateBuilder(args);

var webapi = builder.AddProject<Projects.WebApi>("webapi");

// Aspire 13 has been fundamentally redesigned in the context of .NET 10.
// Example: Integration of JavaScript is now more complete than before.
// Here we use the new Vite integration to add a frontend project based
// on Vite (https://vitejs.dev/). Other new integrations include
// e.g. Python-based projects.
var frontend = builder.AddViteApp("frontend", "../Frontend")
    .WithReference(webapi)
    .WithExternalHttpEndpoints();

builder.Build().Run();

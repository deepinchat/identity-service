using Deepin.Identity.Server.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationService();

var app = builder.Build();

app.ConfigureApplicationService();

app.Run();
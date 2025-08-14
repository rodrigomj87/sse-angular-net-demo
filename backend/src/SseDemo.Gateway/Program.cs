// Copyright (c) SseDemo. All rights reserved.

using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) => cfg
	.ReadFrom.Configuration(ctx.Configuration)
	.Enrich.FromLogContext()
	.WriteTo.Console());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.MapHealthChecks("/health");
app.MapHealthChecks("/ready");

app.MapGet("/", () => Results.Ok(new { ok = true, service = "gateway", ts = DateTimeOffset.UtcNow }));

app.Run();

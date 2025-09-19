using LoopstationCompanionApi.Data;
using LoopstationCompanionApi.Repositories;
using LoopstationCompanionApi.Services;
using LoopstationCompanionApi.Services.LoopstationCompanionApi.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

bool useEf = builder.Configuration.GetValue("UseEfRepository", true);

if (useEf)
    builder.Services.AddScoped<IPresetRepository, EfPresetRepository>();
else
    builder.Services.AddScoped<IPresetRepository, JsonPresetRepository>();

builder.Services.AddScoped<IRc0Validator, Rc0Validator>();
builder.Services.AddScoped<IRc0Importer, Rc0Importer>();
builder.Services.AddScoped<IPresetService, PresetService>();

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
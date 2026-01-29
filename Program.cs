var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger Add
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddSerilogLogging();

builder.Services.AddSqliteDatabase(builder.Configuration, builder.Environment);

builder.Services.AddCorsPolicies(builder.Configuration);

builder.Services.AddScoped<LocalFileStorage>();
builder.Services.AddScoped<ProcessoImobiliarioService>();
builder.Services.AddScoped<ProcessoImobiliarioRepository>();

var app = builder.Build();

// Swagger Use
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<TraceIdMiddleware>();
app.UseSerilogRequestLogging();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<TimeMiddleware>();

app.UseCorsByEnvironment(app.Environment);

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
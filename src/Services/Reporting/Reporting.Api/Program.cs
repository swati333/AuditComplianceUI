using Ehs.Observability.Security;

var builder = WebApplication.CreateBuilder(args);

// --- Building blocks (Ehs.Observability) ---
builder.Services.AddEhsCors(builder.Configuration);
builder.Services.AddEhsEntraIdAuthentication(builder.Configuration);

// --- Web / API ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "EHS Reporting Service",
        Version = "v1",
        Description = "Asynchronous Dapper/stored-procedure reporting and compliance-score aggregation for the EHS Audit & Compliance platform.",
    });
    options.AddEhsEntraIdOAuth(builder.Configuration);
});

var app = builder.Build();

app.UseEhsCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "EHS Reporting Service v1");
        options.UseEhsEntraIdOAuth(builder.Configuration);
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

/// <summary>Exposed so WebApplicationFactory&lt;Program&gt; can bootstrap this app in integration tests.</summary>
public partial class Program
{
}

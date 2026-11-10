using StreamWriter.Web;
using StreamWriter.Web.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddOptions<DailyChallengeTextGeneratorOptions>()
    .Bind(builder.Configuration.GetSection(DailyChallengeTextGeneratorOptions.SectionName))
    .Validate(options =>
    {
        try
        {
            _ = DailyChallengeTextGeneratorFactory.Create(options.Generator);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }, "The DailyChallengeTextGenerator configuration is invalid.")
    .ValidateOnStart();

builder.Services.AddSingleton<IDailyChallengeTextGenerator>(serviceProvider =>
{
    var options = serviceProvider
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<DailyChallengeTextGeneratorOptions>>()
        .Value;

    return DailyChallengeTextGeneratorFactory.Create(options.Generator);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();

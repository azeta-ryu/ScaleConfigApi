using System.Text.Json.Serialization;
using ScaleConfigApi;
using ScaleConfigApi.Models;
using ScaleConfigApi.Services;

var builder = WebApplication.CreateSlimBuilder(args);

// 1. Configure AOT-safe JSON serialization
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

// 2. Register the service for generating scale files
builder.Services.AddSingleton<ScaleFileGenerator>();

var app = builder.Build();

// 3. Define the API endpoint
app.MapPost("/generate-scale-configs", 
    (
        ProductTagLink[] products, 
        ScaleFileGenerator generator, 
        ILogger<Program> logger
    ) =>
{
    Log.ApiRequestReceived(logger, products.Length);

    try
    {
        // Call the generator service to create the binary files
        var result = generator.GenerateFiles(products);
        
        // Return the files as a JSON object
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        Log.ApiError(logger, ex.Message, ex);
        return Results.Problem(ex.Message, statusCode: 500);
    }
});

app.Run();
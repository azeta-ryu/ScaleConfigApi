using Scalar.AspNetCore;
using ScaleConfigApi;
using ScaleConfigApi.Models;
using ScaleConfigApi.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

// 1. Configure AOT-safe JSON serialization
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

// 2. Register the service for generating scale files
builder.Services.AddOpenApi();
builder.Services.AddSingleton<ScaleFileGenerator>();
builder.Services.AddSingleton<ScaleUploaderService>();

var app = builder.Build();
app.MapOpenApi();
app.MapScalarApiReference();

app.MapGet("/", () => Results.Redirect("/scalar"));

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
            var result = generator.GenerateFilesForApiResponse(products);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            Log.ApiError(logger, ex.Message, ex);
            return Results.Problem(ex.Message, statusCode: 500);
        }
    });

app.MapPost("/upload-to-scale",
    async (
        ScaleUploadRequest request,
        ScaleFileGenerator generator,
        ScaleUploaderService uploader,
        ILogger<Program> logger
    ) =>
    {
        Log.UploadRequestReceived(logger, request.Products.Length, request.ScaleIpAddress);
        try
        {
            // 1. Generate the raw binary files
            var files = generator.GenerateFilesRaw(request.Products);

            // 2. Pass them to the uploader service
            var result = await uploader.UploadFilesAsync(request.ScaleIpAddress, files);

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            Log.ApiError(logger, ex.Message, ex);
            return Results.Problem(ex.Message, statusCode: 500);
        }
    });

app.Run();
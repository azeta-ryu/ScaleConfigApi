using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using ScaleConfigApi.Models;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ProductTagLink[]))]
[JsonSerializable(typeof(ScaleFileGenerationResult))]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(ScaleUploadRequest))] // <-- Add this line
[JsonSerializable(typeof(ScaleUploadResult))]  // <-- Add this line
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
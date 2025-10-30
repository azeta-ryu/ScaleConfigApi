using Microsoft.AspNetCore.Mvc;
using ScaleConfigApi.Models;
using System.Text.Json.Serialization;

// This context pre-generates serializers for these types,
// which is required for AOT compilation.
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ProductTagLink[]))]
[JsonSerializable(typeof(ScaleFileGenerationResult))]
[JsonSerializable(typeof(ProblemDetails))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
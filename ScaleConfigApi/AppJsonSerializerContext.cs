using System.Text.Json.Serialization;
using ScaleConfigApi.Models;

// This context pre-generates serializers for these types,
// which is required for AOT compilation.
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ProductTagLink[]))]
[JsonSerializable(typeof(ScaleFileGenerationResult))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
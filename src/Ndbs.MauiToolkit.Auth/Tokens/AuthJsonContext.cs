using System.Text.Json.Serialization;
using Ndbs.MauiToolkit.Auth.Profile;

namespace Ndbs.MauiToolkit.Auth.Tokens
{
    /// <summary>
    /// Source-generated <see cref="System.Text.Json.Serialization.JsonSerializerContext"/>
    /// for the library's persisted types. Using a source generator keeps JSON
    /// serialization free of runtime reflection so it works under trimming and
    /// AOT, including iOS (N1).
    /// </summary>
    [JsonSourceGenerationOptions(WriteIndented = false)]
    [JsonSerializable(typeof(OidcTokenSet))]
    [JsonSerializable(typeof(UserProfile))]
    internal sealed partial class AuthJsonContext : JsonSerializerContext
    {
    }
}

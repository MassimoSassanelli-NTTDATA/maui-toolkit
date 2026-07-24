using System.Text.Json;

namespace Ndbs.MauiToolkit.Xia.MicroApps.Manifest
{
    /// <summary>
    /// Parses a micro app manifest (<c>project.json</c>) into a
    /// <see cref="MicroAppManifest"/>. The parser is deliberately tolerant: undeclared
    /// root/form fields are ignored, the <c>texts</c> array (undeclared in the
    /// 2021-11-23 schema but present in real packages) is processed, and scalar values
    /// (null/number/string/boolean) in <c>customproperties</c> and <c>settings</c> are
    /// serialized to their string representation (§7.4).
    /// </summary>
    public static class MicroAppManifestParser
    {
        private static readonly JsonDocumentOptions DocumentOptions = new()
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip,
        };

        /// <summary>Parses the manifest from its JSON representation.</summary>
        /// <param name="json">The raw <c>project.json</c> content.</param>
        /// <returns>The parsed manifest.</returns>
        /// <exception cref="ArgumentException">The JSON is empty.</exception>
        /// <exception cref="MicroAppManifestException">The JSON is invalid or misses required fields.</exception>
        public static MicroAppManifest Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentException("The manifest JSON must not be empty.", nameof(json));
            }

            JsonDocument document;
            try
            {
                document = JsonDocument.Parse(json, DocumentOptions);
            }
            catch (JsonException ex)
            {
                throw new MicroAppManifestException("The manifest is not valid JSON.", ex);
            }

            using (document)
            {
                var root = document.RootElement;
                if (root.ValueKind != JsonValueKind.Object)
                {
                    throw new MicroAppManifestException("The manifest root must be a JSON object.");
                }

                var name = ReadRequiredString(root, "name");

                return new MicroAppManifest
                {
                    Name = name,
                    DisplayName = ReadOptionalString(root, "displayname"),
                    Description = ReadOptionalString(root, "description"),
                    CustomProperties = ReadScalarMap(root, "customproperties"),
                    Forms = ReadForms(root),
                };
            }
        }

        private static IReadOnlyList<MicroAppManifestForm> ReadForms(JsonElement root)
        {
            if (!root.TryGetProperty("forms", out var forms) || forms.ValueKind != JsonValueKind.Array)
            {
                throw new MicroAppManifestException("The manifest must declare a 'forms' array.");
            }

            var result = new List<MicroAppManifestForm>();
            foreach (var form in forms.EnumerateArray())
            {
                if (form.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                result.Add(new MicroAppManifestForm
                {
                    Name = ReadRequiredString(form, "name"),
                    DisplayName = ReadOptionalString(form, "displayname"),
                    Description = ReadOptionalString(form, "description"),
                    Path = ReadRequiredString(form, "path"),
                    Completions = ReadStringArray(form, "completions"),
                    Texts = ReadStringArray(form, "texts"),
                    Settings = ReadScalarMap(form, "settings"),
                });
            }

            return result;
        }

        private static string ReadRequiredString(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var value) ||
                value.ValueKind != JsonValueKind.String ||
                string.IsNullOrWhiteSpace(value.GetString()))
            {
                throw new MicroAppManifestException($"The manifest is missing the required '{propertyName}' field.");
            }

            return value.GetString()!;
        }

        private static string? ReadOptionalString(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String)
            {
                return value.GetString();
            }

            return null;
        }

        private static IReadOnlyList<string> ReadStringArray(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var array) || array.ValueKind != JsonValueKind.Array)
            {
                return Array.Empty<string>();
            }

            var result = new List<string>();
            foreach (var item in array.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    var text = item.GetString();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        result.Add(text!);
                    }
                }
            }

            return result;
        }

        private static IReadOnlyDictionary<string, string?> ReadScalarMap(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var map) || map.ValueKind != JsonValueKind.Object)
            {
                return new Dictionary<string, string?>();
            }

            var result = new Dictionary<string, string?>(StringComparer.Ordinal);
            foreach (var property in map.EnumerateObject())
            {
                result[property.Name] = SerializeScalar(property.Value);
            }

            return result;
        }

        private static string? SerializeScalar(JsonElement value) => value.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.String => value.GetString(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Number => value.GetRawText(),
            // Objects / arrays are not expected for scalar maps; keep the raw JSON so
            // no information is lost and parsing stays tolerant.
            _ => value.GetRawText(),
        };
    }
}

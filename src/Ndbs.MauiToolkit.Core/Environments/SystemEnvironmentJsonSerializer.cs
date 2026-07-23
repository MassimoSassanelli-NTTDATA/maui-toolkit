using System.Text;
using System.Text.Json;

namespace Ndbs.MauiToolkit.Environments
{
    /// <summary>
    /// Serializes and deserializes the schema-less list of
    /// <see cref="SystemEnvironment"/> values to and from JSON. The on-disk format is
    /// a JSON array of objects, each carrying a <c>Name</c> plus arbitrary section
    /// objects. The implementation uses <see cref="JsonDocument"/> and
    /// <see cref="Utf8JsonWriter"/> directly so it stays reflection-free and safe for
    /// iOS trimming / AOT, even though the section types are unknown.
    /// </summary>
    public static class SystemEnvironmentJsonSerializer
    {
        private static readonly JsonWriterOptions WriterOptions = new() { Indented = true };

        /// <summary>Parses a JSON array of environments.</summary>
        /// <param name="json">The JSON payload.</param>
        /// <returns>The parsed environments (sections detached from the document).</returns>
        /// <exception cref="FormatException">The payload is not a JSON array of environment objects.</exception>
        public static IReadOnlyList<SystemEnvironment> Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return Array.Empty<SystemEnvironment>();
            }

            JsonDocument document;
            try
            {
                document = JsonDocument.Parse(json);
            }
            catch (JsonException ex)
            {
                throw new FormatException("Die Konfigurationsdaten sind kein gültiges JSON.", ex);
            }

            using (document)
            {
                var root = document.RootElement;
                if (root.ValueKind != JsonValueKind.Array)
                {
                    throw new FormatException("Die Konfigurationsdaten müssen eine Liste von Systemumgebungen sein.");
                }

                var result = new List<SystemEnvironment>();
                foreach (var element in root.EnumerateArray())
                {
                    result.Add(ReadEnvironment(element));
                }

                return result;
            }
        }

        /// <summary>Serializes the environments to an indented JSON array.</summary>
        /// <param name="environments">The environments to serialize.</param>
        /// <returns>The JSON payload.</returns>
        public static string Serialize(IReadOnlyList<SystemEnvironment> environments)
        {
            ArgumentNullException.ThrowIfNull(environments);

            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream, WriterOptions))
            {
                writer.WriteStartArray();
                foreach (var environment in environments)
                {
                    writer.WriteStartObject();
                    writer.WriteString(SystemEnvironment.NameProperty, environment.Name);
                    foreach (var (key, value) in environment.Sections)
                    {
                        writer.WritePropertyName(key);
                        value.WriteTo(writer);
                    }

                    writer.WriteEndObject();
                }

                writer.WriteEndArray();
            }

            return Encoding.UTF8.GetString(stream.ToArray());
        }

        private static SystemEnvironment ReadEnvironment(JsonElement element)
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                throw new FormatException("Jede Systemumgebung muss ein JSON-Objekt sein.");
            }

            if (!element.TryGetProperty(SystemEnvironment.NameProperty, out var nameElement) ||
                nameElement.ValueKind != JsonValueKind.String ||
                string.IsNullOrWhiteSpace(nameElement.GetString()))
            {
                throw new FormatException("Jede Systemumgebung benötigt einen eindeutigen Namen.");
            }

            var name = nameElement.GetString()!;
            var sections = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, SystemEnvironment.NameProperty, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Clone detaches the value from the document so it stays valid after dispose.
                sections[property.Name] = property.Value.Clone();
            }

            return new SystemEnvironment(name, sections);
        }
    }
}

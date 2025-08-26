using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace LoopstationCompanionApi.Services
{
    namespace LoopstationCompanionApi.Services
    {
        public class Rc0Importer : IRc0Importer
        {
            private readonly IRc0Validator _validator;

            public Rc0Importer(IRc0Validator validator) => _validator = validator;

            public async Task<string> ImportAndSanitizeAsync(Stream rc0Stream, CancellationToken ct = default)
            {
                using var reader = new StreamReader(rc0Stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 64 * 1024, leaveOpen: true);
                var raw = await reader.ReadToEndAsync();

                var cleaned = CleanXmlContent(raw);
                var errors = await _validator.ValidateAsync(cleaned, ct);
                if (errors.Count > 0)
                    throw new InvalidDataException("RC0 validation failed: " + string.Join("; ", errors));

                var payload = BuildSanitizedPayload(cleaned);

                return JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = false });
            }

            // ======= CLEANING =======
            private string CleanXmlContent(string xml)
            {
                // Replace <0>, </0> ... → <NUM_0>, </NUM_0>
                string cleaned = Regex.Replace(xml, "<([0-9])>", "<NUM_$1>");
                cleaned = Regex.Replace(cleaned, "</([0-9])>", "</NUM_$1>");

                // Replace single symbol tags <#> → <SYMBOL_HASH>
                cleaned = Regex.Replace(cleaned, "<([^\\w\\.-:])>", m => $"<SYMBOL_{SymbolToName(m.Groups[1].Value)}>");
                cleaned = Regex.Replace(cleaned, "</([^\\w\\.-:])>", m => $"</SYMBOL_{SymbolToName(m.Groups[1].Value)}>");

                // Drop <count>...</count>
                cleaned = Regex.Replace(cleaned, "<count>.*?</count>", "", RegexOptions.Singleline);

                return cleaned.Trim();
            }

            private static string SymbolToName(string symbol) => symbol switch
            {
                "#" => "HASH",
                "$" => "DOLLAR",
                "@" => "AT",
                "%" => "PERCENT",
                "&" => "AMPERSAND",
                "*" => "ASTERISK",
                _ => Uri.EscapeDataString(symbol)
            };

            // ======= BUILD FE-READY PAYLOAD =======
            private object BuildSanitizedPayload(string cleanedXml)
            {
                var xdoc = XDocument.Parse(cleanedXml, LoadOptions.PreserveWhitespace);
                if (xdoc.Root is null) throw new InvalidDataException("No root element.");

                // Find <database>
                XElement? database = string.Equals(xdoc.Root.Name.LocalName, "database", StringComparison.OrdinalIgnoreCase)
                    ? xdoc.Root
                    : xdoc.Root.Element("database") ?? xdoc.Root.Elements().FirstOrDefault(e =>
                        string.Equals(e.Name.LocalName, "database", StringComparison.OrdinalIgnoreCase));

                if (database is null) throw new InvalidDataException("Missing <database>.");

                // Find <ifx> under <database>
                XElement? ifx = database.Element("ifx") ?? database.Elements().FirstOrDefault(e =>
                    string.Equals(e.Name.LocalName, "ifx", StringComparison.OrdinalIgnoreCase));
                if (ifx is null) throw new InvalidDataException("Missing <ifx>.");

                var effects = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                foreach (var eff in ifx.Elements())
                {
                    if (string.Equals(eff.Name.LocalName, "_attributes", StringComparison.OrdinalIgnoreCase)) continue;

                    var effectKey = eff.Name.LocalName; // e.g. AA_LPF
                    var mapped = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                    foreach (var param in eff.Elements())
                    {
                        var key = param.Name.LocalName;           // A, B, NUM_0, SYMBOL_HASH, etc.
                        var raw = (param.Value ?? string.Empty).Trim();

                        var meta = EffectMeta.GetParamMeta(effectKey, key);
                        var clamped = ClampToRange(raw, meta.Min, meta.Max);

                        mapped[meta.Label] = new Dictionary<string, string> { ["_text"] = clamped };
                    }

                    effects[effectKey] = mapped;
                }

                var completed = DefaultPayloadFactory.ApplyDefaults(effects);

                return new
                {
                    database = new
                    {
                        ifx = completed
                    }
                };
            }

            private static string ClampToRange(string raw, int min, int max)
            {
                if (!int.TryParse(raw, out var n)) n = min;
                if (n < min) n = min;
                if (n > max) n = max;
                return n.ToString();
            }
        }
    }
}
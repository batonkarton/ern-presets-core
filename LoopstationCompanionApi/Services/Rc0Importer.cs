using LoopstationCompanionApi.Constants;
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

            public async Task<string> ImportAndSanitizeAsync(IFormFile file, CancellationToken ct = default)
            {
                if (file is null) throw new ArgumentNullException(nameof(file));
                if (file.Length == 0) throw new InvalidDataException("Uploaded file is empty.");

                using var stream = file.OpenReadStream();
                using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 64 * 1024, leaveOpen: false);
                var raw = await reader.ReadToEndAsync();

                var cleaned = CleanXmlContent(raw);
                var errors = await _validator.ValidateAsync(cleaned, ct);
                if (errors.Count > 0)
                    throw new InvalidDataException("RC0 validation failed: " + string.Join("; ", errors));

                var payload = BuildSanitizedPayload(cleaned);
                return JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = false });
            }

            private string CleanXmlContent(string xml)
            {
                // Replace <0>, </0> ... → <NUM_0>, </NUM_0>
                string cleaned = Regex.Replace(xml, XmlRegexPatterns.OpenNumericTag, XmlRegexPatterns.OpenNumericReplacement);
                cleaned = Regex.Replace(cleaned, XmlRegexPatterns.CloseNumericTag, XmlRegexPatterns.CloseNumericReplacement);

                // Replace single symbol tags <#> → <SYMBOL_HASH>, etc.
                cleaned = Regex.Replace(xml, XmlRegexPatterns.OpenSymbolTag,
                    m => string.Format(XmlRegexPatterns.OpenSymbolReplacement, SymbolToName(m.Groups[1].Value)));

                cleaned = Regex.Replace(cleaned, XmlRegexPatterns.CloseSymbolTag,
                    m => string.Format(XmlRegexPatterns.CloseSymbolReplacement, SymbolToName(m.Groups[1].Value)));

                // Drop <count>...</count>
                cleaned = Regex.Replace(cleaned, XmlRegexPatterns.CountTagPattern, "", RegexOptions.Singleline);

                return cleaned.Trim();
            }

            private static string SymbolToName(string symbol) => symbol switch
            {
                "#" => XmlConstants.SymbolHash,
                "$" => XmlConstants.SymbolDollar,
                "@" => XmlConstants.SymbolAt,
                "%" => XmlConstants.SymbolPercent,
                "&" => XmlConstants.SymbolAmpersand,
                "*" => XmlConstants.SymbolAsterisk,
                _ => Uri.EscapeDataString(symbol)
            };

            // BUILD FE-READY PAYLOAD
            private object BuildSanitizedPayload(string cleanedXml)
            {
                var xdoc = XDocument.Parse(cleanedXml, LoadOptions.PreserveWhitespace);
                if (xdoc.Root is null) throw new InvalidDataException("No root element.");

                // Find <database>
                XElement? database = string.Equals(xdoc.Root.Name.LocalName, XmlConstants.DatabaseTag, StringComparison.OrdinalIgnoreCase)
                    ? xdoc.Root
                    : xdoc.Root.Element(XmlConstants.DatabaseTag) ?? xdoc.Root.Elements().FirstOrDefault(e =>
                        string.Equals(e.Name.LocalName, XmlConstants.DatabaseTag, StringComparison.OrdinalIgnoreCase));

                if (database is null) throw new InvalidDataException($"Missing <{XmlConstants.DatabaseTag}>.");

                // Find <ifx> under <database>
                XElement? ifx = database.Element(XmlConstants.IfxTag) ?? database.Elements().FirstOrDefault(e =>
                    string.Equals(e.Name.LocalName, XmlConstants.IfxTag, StringComparison.OrdinalIgnoreCase));
                if (ifx is null) throw new InvalidDataException($"Missing <{XmlConstants.IfxTag}>.");

                var effects = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                foreach (var eff in ifx.Elements())
                {
                    if (string.Equals(eff.Name.LocalName, XmlConstants.AttributesTag, StringComparison.OrdinalIgnoreCase)) continue;

                    var effectKey = eff.Name.LocalName; // e.g. AA_LPF
                    var mapped = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                    foreach (var param in eff.Elements())
                    {
                        var key = param.Name.LocalName;
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
using LoopstationCompanionApi.Constants;
using System.Xml.Linq;

namespace LoopstationCompanionApi.Services
{
    public class Rc0Validator : IRc0Validator
    {
        public Task<IReadOnlyList<string>> ValidateAsync(string cleanedXml, CancellationToken ct = default)
        {
            var errors = new List<string>();
            XDocument? doc;

            try
            {
                doc = XDocument.Parse(cleanedXml, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
            }
            catch (Exception ex)
            {
                errors.Add($"XML parse error: {ex.Message}");
                return Task.FromResult<IReadOnlyList<string>>(errors);
            }

            if (doc.Root is null)
            {
                errors.Add("Missing root element.");
                return Task.FromResult<IReadOnlyList<string>>(errors);
            }

            // Accept either: root == <database> OR a descendant <database>
            var rootName = doc.Root.Name.LocalName;
            XElement? database = null;

            if (string.Equals(rootName, XmlConstants.DatabaseTag, StringComparison.OrdinalIgnoreCase))
            {
                database = doc.Root;
            }
            else
            {
                database = doc.Root.Element(XmlConstants.DatabaseTag)
                           ?? doc.Root.Elements().FirstOrDefault(e =>
                                string.Equals(e.Name.LocalName, XmlConstants.DatabaseTag, StringComparison.OrdinalIgnoreCase));
            }

            if (database is null)
            {
                errors.Add($"Missing <{XmlConstants.DatabaseTag}> element.");
                return Task.FromResult<IReadOnlyList<string>>(errors);
            }

            // <ifx> can be a child somewhere under <database> (sibling to <mem>, etc.)
            var ifx = database.Element(XmlConstants.IfxTag)
                      ?? database.Elements().FirstOrDefault(e =>
                           string.Equals(e.Name.LocalName, XmlConstants.IfxTag, StringComparison.OrdinalIgnoreCase));

            if (ifx is null)
            {
                errors.Add($"Missing <{XmlConstants.IfxTag}> element under <{XmlConstants.DatabaseTag}>.");
                return Task.FromResult<IReadOnlyList<string>>(errors);
            }

            var effects = ifx.Elements()
                .Where(e => !string.Equals(e.Name.LocalName, XmlConstants.AttributesTag, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (effects.Count == 0)
                errors.Add($"No effects found in <{XmlConstants.IfxTag}>.");

            // Basic param sanity
            foreach (var eff in effects)
            {
                foreach (var p in eff.Elements())
                {
                    if (string.IsNullOrWhiteSpace(p.Value))
                        errors.Add($"Effect '{eff.Name.LocalName}' has empty param '{p.Name.LocalName}'.");
                }
            }

            return Task.FromResult<IReadOnlyList<string>>(errors);
        }
    }
}

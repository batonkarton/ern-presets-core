using System.Text.Json;

namespace LoopstationCompanionApi.Services
{
    public static class DefaultPayloadFactory
    {
        public static object DefaultPayloadObject()
        {
            var ifx = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            foreach (var effect in EffectMeta.EffectKeys())
            {
                var paramDict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                foreach (var kv in EffectMeta.Params(effect))
                {
                    var meta = kv.Value;
                    paramDict[meta.Label] = new Dictionary<string, string>
                    {
                        ["_text"] = meta.Default.ToString()
                    };
                }

                ifx[effect] = paramDict;
            }

            return new { database = new { ifx } };
        }

        public static string DefaultPayloadJson() =>
            JsonSerializer.Serialize(DefaultPayloadObject());

        /// <summary>
        /// Merge a partially built IFX dictionary (effect -> { label -> {"_text": "..."} })
        /// with defaults from EffectMeta. Ensures every known effect/param exists with a value.
        /// Keeps unknown extra params and unknown effects if present.
        /// </summary>
        public static IDictionary<string, object> ApplyDefaults(IDictionary<string, object> partial)
        {
            var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            foreach (var effect in EffectMeta.EffectKeys())
            {
                // Provided effect block from partial (may be null)
                var providedEffect = GetChildDict(partial, effect);

                // Build merged param dict
                var mergedParams = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                // Ensure all known params exist (take provided value if present, else meta default)
                foreach (var kv in EffectMeta.Params(effect))
                {
                    var meta = kv.Value;
                    var label = meta.Label;

                    var providedParam = GetChildDict(providedEffect, label);
                    var providedText = ExtractText(providedParam);

                    var valueStr = !string.IsNullOrWhiteSpace(providedText)
                        ? providedText!
                        : meta.Default.ToString();

                    mergedParams[label] = new Dictionary<string, string>
                    {
                        ["_text"] = valueStr
                    };
                }

                // Keep any extra/unknown params present in the payload
                if (providedEffect is not null)
                {
                    foreach (var kv in providedEffect)
                    {
                        if (!mergedParams.ContainsKey(kv.Key))
                        {
                            mergedParams[kv.Key] = kv.Value;
                        }
                    }
                }

                result[effect] = mergedParams;
            }

            // Preserve any unknown effects that were present in the payload
            foreach (var kv in partial)
            {
                if (!result.ContainsKey(kv.Key))
                {
                    result[kv.Key] = kv.Value;
                }
            }

            return result;
        }

        // Cast an object to Dictionary<string, object> if possible
        private static IDictionary<string, object>? AsDict(object? obj)
            => obj as IDictionary<string, object>;

        // Safely fetch a child dictionary by key from a parent dictionary
        private static IDictionary<string, object>? GetChildDict(
            IDictionary<string, object>? dict,
            string key)
        {
            if (dict is null) return null;
            return dict.TryGetValue(key, out var obj) ? AsDict(obj) : null;
        }

        // Extract "_text" value from a param object that could be
        // Dictionary<string, object> OR Dictionary<string, string>
        private static string? ExtractText(object? paramObj)
        {
            if (paramObj is IDictionary<string, object> d1)
            {
                if (d1.TryGetValue("_text", out var v))
                    return v?.ToString();
            }
            else if (paramObj is IDictionary<string, string> d2)
            {
                if (d2.TryGetValue("_text", out var v))
                    return v;
            }
            return null;
        }
    }
}

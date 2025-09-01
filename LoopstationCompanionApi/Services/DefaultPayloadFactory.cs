using System.Text.Json;

namespace LoopstationCompanionApi.Services
{
    public static class DefaultPayloadFactory
    {
        public static string GetDefaultPayloadJson() =>
            JsonSerializer.Serialize(BuildDefaultPayloadObject());

        private static object BuildDefaultPayloadObject()
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
                var providedEffect = GetChildDict(partial, effect);
                var mergedParams = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                foreach (var kv in EffectMeta.Params(effect))
                {
                    var meta = kv.Value;
                    var label = meta.Label;

                    object? providedParamObj = GetChildRaw(providedEffect, label);
                    var providedText = ExtractText(providedParamObj);

                    var valueStr = !string.IsNullOrWhiteSpace(providedText)
                        ? providedText!
                        : meta.Default.ToString();

                    mergedParams[label] = new Dictionary<string, string>
                    {
                        ["_text"] = valueStr
                    };
                }

                if (providedEffect is not null)
                {
                    foreach (var kv in providedEffect)
                    {
                        if (!mergedParams.ContainsKey(kv.Key))
                            mergedParams[kv.Key] = kv.Value;
                    }
                }

                result[effect] = mergedParams;
            }

            foreach (var kv in partial)
                if (!result.ContainsKey(kv.Key))
                    result[kv.Key] = kv.Value;

            return result;
        }

        private static object? GetChildRaw(IDictionary<string, object>? dict, string key)
            => dict is null ? null : (dict.TryGetValue(key, out var obj) ? obj : null);

        private static IDictionary<string, object>? GetChildDict(
            IDictionary<string, object>? dict,
            string key)
            => dict is null ? null : (dict.TryGetValue(key, out var obj) ? AsDict(obj) : null);

        private static IDictionary<string, object>? AsDict(object? obj)
            => obj as IDictionary<string, object>;

        // Extract "_text" value from a param object that could be
        // Dictionary<string, object> OR Dictionary<string, string>
        private static string? ExtractText(object? paramObj)
        {
            if (paramObj is IDictionary<string, object> d1)
            {
                if (d1.TryGetValue("_text", out var v)) return v?.ToString();
            }
            else if (paramObj is IDictionary<string, string> d2)
            {
                if (d2.TryGetValue("_text", out var v)) return v;
            }
            return null;
        }
    }
}

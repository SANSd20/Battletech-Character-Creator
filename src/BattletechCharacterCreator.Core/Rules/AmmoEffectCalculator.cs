using System.Globalization;

namespace BattletechCharacterCreator.Core.Rules;

public static class AmmoEffectCalculator
{
    public static string CalculateDamage(string baseDamage, string modifier)
    {
        if (string.IsNullOrWhiteSpace(baseDamage) ||
            string.IsNullOrWhiteSpace(modifier) ||
            modifier.Trim() == "-")
        {
            return "";
        }

        var baseParts = baseDamage.Split('/',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var modifierParts = modifier.Split('/',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (baseParts.Length != 2 || modifierParts.Length != 2)
        {
            return modifier;
        }

        return TryApplyDamagePart(baseParts[0], modifierParts[0], out var ap) &&
            TryApplyDamagePart(baseParts[1], modifierParts[1], out var bd)
            ? $"{ap}/{bd}"
            : modifier;
    }

    public static string CalculateRange(string baseRange, string modifier)
    {
        if (string.IsNullOrWhiteSpace(baseRange) ||
            string.IsNullOrWhiteSpace(modifier) ||
            modifier.Trim() == "-")
        {
            return "";
        }

        if (!modifier.Equals("Half range", StringComparison.OrdinalIgnoreCase))
        {
            return modifier;
        }

        var parts = baseRange.Split('/',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
        {
            return modifier;
        }

        var ranges = new List<string>();
        foreach (var part in parts)
        {
            if (!decimal.TryParse(
                    part,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out var value))
            {
                return modifier;
            }

            ranges.Add((value / 2m).ToString("0.##", CultureInfo.InvariantCulture));
        }

        return string.Join("/", ranges);
    }

    private static bool TryApplyDamagePart(
        string basePart,
        string modifierPart,
        out string result)
    {
        result = "";
        if (!TryReadBaseDamage(basePart, out var baseValue, out var baseSuffix) ||
            !TryReadDamageModifier(modifierPart, out var modifierValue, out var modifierSuffix))
        {
            return false;
        }

        var adjusted = Math.Max(0, baseValue + modifierValue);
        var suffix = modifierSuffix.Length > 0 ? modifierSuffix : baseSuffix;
        result = $"{adjusted}{suffix}";
        return true;
    }

    private static bool TryReadBaseDamage(
        string value,
        out int numeric,
        out string suffix)
    {
        numeric = 0;
        suffix = "";
        var trimmed = value.Trim();
        var index = 0;
        while (index < trimmed.Length && char.IsDigit(trimmed[index]))
        {
            index++;
        }

        if (index == 0 ||
            !int.TryParse(
                trimmed[..index],
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out numeric))
        {
            return false;
        }

        suffix = trimmed[index..];
        return suffix.All(char.IsLetter);
    }

    private static bool TryReadDamageModifier(
        string value,
        out int numeric,
        out string suffix)
    {
        numeric = 0;
        suffix = "";
        var trimmed = value.Trim();
        if (trimmed.Length < 2 || (trimmed[0] != '+' && trimmed[0] != '-'))
        {
            return false;
        }

        var index = 1;
        while (index < trimmed.Length && char.IsDigit(trimmed[index]))
        {
            index++;
        }

        if (index == 1 ||
            !int.TryParse(
                trimmed[..index],
                NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture,
                out numeric))
        {
            return false;
        }

        suffix = trimmed[index..];
        return suffix.All(char.IsLetter);
    }
}

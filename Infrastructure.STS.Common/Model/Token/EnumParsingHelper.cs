using System;

namespace Kombit.InfrastructureSamples.Token;

internal static class EnumParsingHelper
{
    internal static TEnum? ParseOptionalEnum<TEnum>(string? value, string parameterName)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        if (Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed))
            return parsed;
        throw new ArgumentException(
            $"Invalid value '{value}' for {parameterName}. Expected one of: {string.Join(", ", Enum.GetNames(typeof(TEnum)))}",
            parameterName);
    }
}

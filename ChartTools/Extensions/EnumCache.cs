﻿namespace ChartTools.Extensions;

internal static class EnumCache<T> where T : struct, Enum
{
    public static T[] Values => _values ??= [.. Enum.GetValues<T>()];
    private static T[]? _values;

    public static void Clear() => _values = null;
}

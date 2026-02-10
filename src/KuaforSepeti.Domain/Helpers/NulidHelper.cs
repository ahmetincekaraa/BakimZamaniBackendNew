namespace KuaforSepeti.Domain.Helpers;

using NUlid;

/// <summary>
/// NULID (Universally Unique Lexicographically Sortable Identifier) helper methods.
/// </summary>
public static class NulidHelper
{
    /// <summary>
    /// Generates a new NULID and returns it as a string.
    /// </summary>
    /// <returns>A new NULID as a 26-character string.</returns>
    public static string NewNulidToString()
    {
        return Ulid.NewUlid().ToString();
    }

    /// <summary>
    /// Generates a new NULID with a specific timestamp and returns it as a string.
    /// </summary>
    /// <param name="timestamp">The timestamp to use for the NULID.</param>
    /// <returns>A new NULID as a 26-character string.</returns>
    public static string NewNulidToString(DateTimeOffset timestamp)
    {
        return Ulid.NewUlid(timestamp).ToString();
    }

    /// <summary>
    /// Parses a NULID string and returns the Ulid object.
    /// </summary>
    /// <param name="nulidString">The NULID string to parse.</param>
    /// <returns>The parsed Ulid object.</returns>
    public static Ulid Parse(string nulidString)
    {
        return Ulid.Parse(nulidString);
    }

    /// <summary>
    /// Tries to parse a NULID string.
    /// </summary>
    /// <param name="nulidString">The NULID string to parse.</param>
    /// <param name="result">The parsed Ulid if successful.</param>
    /// <returns>True if parsing was successful, false otherwise.</returns>
    public static bool TryParse(string nulidString, out Ulid result)
    {
        return Ulid.TryParse(nulidString, out result);
    }

    /// <summary>
    /// Validates if a string is a valid NULID format.
    /// </summary>
    /// <param name="nulidString">The string to validate.</param>
    /// <returns>True if valid NULID format, false otherwise.</returns>
    public static bool IsValid(string nulidString)
    {
        if (string.IsNullOrWhiteSpace(nulidString))
            return false;

        return Ulid.TryParse(nulidString, out _);
    }

    /// <summary>
    /// Extracts the timestamp from a NULID string.
    /// </summary>
    /// <param name="nulidString">The NULID string.</param>
    /// <returns>The timestamp embedded in the NULID.</returns>
    public static DateTimeOffset GetTimestamp(string nulidString)
    {
        var ulid = Ulid.Parse(nulidString);
        return ulid.Time;
    }
}

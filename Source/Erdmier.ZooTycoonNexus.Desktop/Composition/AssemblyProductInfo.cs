namespace Erdmier.ZooTycoonNexus.Desktop.Composition;

/// <summary>Reads the product information from the attributes the build stamps into an assembly, falling back to neutral values when they are missing.</summary>
public sealed class AssemblyProductInfo : IProductInfo
{
    private const string BuildDateMetadataKey = "BuildDateUtc";
    private const string BuildDateFormat = "yyyy-MM-dd";
    private const string FallbackVersion = "0.0.0";

    /// <summary>Initialises a new instance of the <see cref="AssemblyProductInfo" /> class, reading all three values once.</summary>
    /// <param name="assembly">The assembly carrying the stamped attributes.</param>
    /// <param name="logger">The logger that receives a warning for each missing attribute.</param>
    public AssemblyProductInfo(Assembly assembly, ILogger<AssemblyProductInfo> logger)
    {
        var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        if (string.IsNullOrWhiteSpace(informationalVersion))
        {
            logger.LogWarning("The assembly has no AssemblyInformationalVersion attribute; reporting version {Version}.", FallbackVersion);
            Version = FallbackVersion;
        }
        else
        {
            Version = NormaliseVersion(informationalVersion);
        }

        var buildDate = assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
                                .FirstOrDefault(attribute => attribute.Key == BuildDateMetadataKey)?.Value;

        if (DateOnly.TryParseExact(buildDate, BuildDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            BuildDateUtc = parsed;
        }
        else
        {
            logger.LogWarning("The assembly has no valid {Key} metadata (found {Value}); reporting no build date.", BuildDateMetadataKey, buildDate);
            BuildDateUtc = DateOnly.MinValue;
        }

        var copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;

        if (string.IsNullOrWhiteSpace(copyright))
        {
            logger.LogWarning("The assembly has no AssemblyCopyright attribute; reporting an empty copyright.");
            Copyright = string.Empty;
        }
        else
        {
            Copyright = copyright;
        }
    }

    /// <inheritdoc />
    public string Version { get; }

    /// <inheritdoc />
    public DateOnly BuildDateUtc { get; }

    /// <inheritdoc />
    public string Copyright { get; }

    /// <summary>Removes the build metadata, everything from the first <c>+</c>, that the .NET SDK appends to an informational version.</summary>
    /// <param name="informationalVersion">The informational version, for example <c>1.0.0+65e5c3d</c>.</param>
    /// <returns>The version without build metadata, for example <c>1.0.0</c>.</returns>
    public static string NormaliseVersion(string informationalVersion)
    {
        var plus = informationalVersion.IndexOf('+');

        return plus < 0 ? informationalVersion : informationalVersion[..plus];
    }
}

namespace Erdmier.ZooTycoonNexus.Desktop.Composition;

/// <summary>Describes the running product: its release number, build date and copyright, as stamped into the Desktop assembly at build time.</summary>
public interface IProductInfo
{
    /// <summary>Gets the release number, free of any build metadata (for example <c>1.0.0</c>).</summary>
    string Version { get; }

    /// <summary>Gets the UTC date on which the assembly was built, or <see cref="DateOnly.MinValue" /> when it is not stamped.</summary>
    DateOnly BuildDateUtc { get; }

    /// <summary>Gets the copyright notice, or an empty string when it is not stamped.</summary>
    string Copyright { get; }
}

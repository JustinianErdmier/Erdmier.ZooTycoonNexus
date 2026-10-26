namespace Erdmier.ZooTycoonNexus.Desktop.ViewModels.Dialogs;

/// <summary>The view model for the modal About dialogue.</summary>
public sealed class AboutDialogViewModel : ViewModelBase
{
    /// <summary>Initialises a new instance.</summary>
    /// <param name="productInfo">The build-stamped product information.</param>
    public AboutDialogViewModel(IProductInfo productInfo)
    {
        VersionLine = $"Version {productInfo.Version} (build {productInfo.BuildDateUtc.ToString(format: "yyyy.MM.dd", CultureInfo.InvariantCulture)})";
        Copyright   = productInfo.Copyright;
    }

    /// <summary>Initialises a new instance for the XAML designer.</summary>
    public AboutDialogViewModel()
        : this(new DesignerProductInfo())
    { }

    /// <summary>The product name.</summary>
    public string ProductName => "Zoo Tycoon Nexus";

    /// <summary>The version and build date line, with the date formatted independently of the current culture.</summary>
    public string VersionLine { get; }

    /// <summary>A one-line description of the product.</summary>
    public string Description => "A native Windows launcher and configuration tool for Zoo Tycoon (2001).";

    /// <summary>The technology attribution line.</summary>
    public string Attribution => "Built with .NET 10, Avalonia 11.3, Classic.Avalonia, SQLite via EF Core and Serilog.";

    /// <summary>The copyright line.</summary>
    public string Copyright { get; }
}

/// <summary>Designer-only product information; the version here is sample data, not the app's version.</summary>
file sealed class DesignerProductInfo : IProductInfo
{
    public string Version => "1.0.0";

    public DateOnly BuildDateUtc => new(year: 2026, month: 10, day: 25);

    public string Copyright => "© 2026 Justinian Erdmier";
}

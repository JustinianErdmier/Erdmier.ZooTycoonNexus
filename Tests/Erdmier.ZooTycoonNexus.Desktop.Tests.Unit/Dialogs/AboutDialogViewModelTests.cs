namespace Erdmier.ZooTycoonNexus.Desktop.Tests.Unit.Dialogs;

public sealed class AboutDialogViewModelTests
{
    private static IProductInfo ProductInfo(string version = "2.4.6", string copyright = "© 2027 Someone Else")
    {
        IProductInfo productInfo = Substitute.For<IProductInfo>();

        productInfo.Version.Returns(version);
        productInfo.BuildDateUtc.Returns(new DateOnly(2027, 1, 9));
        productInfo.Copyright.Returns(copyright);

        return productInfo;
    }

    [ Fact ]
    public void VersionLine_FormatsVersionAndBuildDate()
    {
        AboutDialogViewModel viewModel = new(ProductInfo());

        viewModel.VersionLine.ShouldBe("Version 2.4.6 (build 2027.01.09)");
    }

    [ Theory ]
    [ InlineData("th-TH") ]
    [ InlineData("ar-SA") ]
    public void VersionLine_IgnoresCurrentCulture(string cultureName)
    {
        CultureInfo original = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);

            AboutDialogViewModel viewModel = new(ProductInfo());

            viewModel.VersionLine.ShouldBe("Version 2.4.6 (build 2027.01.09)");
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [ Fact ]
    public void Copyright_PassesThrough()
    {
        AboutDialogViewModel viewModel = new(ProductInfo(copyright: "© 2027 Someone Else"));

        viewModel.Copyright.ShouldBe("© 2027 Someone Else");
    }

    [ Fact ]
    public void ProductName_Description_Attribution_AreTheSpecCopy()
    {
        AboutDialogViewModel viewModel = new(ProductInfo());

        viewModel.ProductName.ShouldBe("Zoo Tycoon Nexus");
        viewModel.Description.ShouldBe("A native Windows launcher and configuration tool for Zoo Tycoon (2001).");
        viewModel.Attribution.ShouldBe("Built with .NET 10, Avalonia 11.3, Classic.Avalonia, SQLite via EF Core and Serilog.");
    }
}

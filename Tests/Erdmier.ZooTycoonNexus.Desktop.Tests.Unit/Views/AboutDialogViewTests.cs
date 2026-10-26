using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

using Erdmier.ZooTycoonNexus.Desktop.Views.Dialogs;

namespace Erdmier.ZooTycoonNexus.Desktop.Tests.Unit.Views;

/// <summary>Verifies the About dialogue's keyboard, wrapping and logo behaviour.</summary>
public sealed class AboutDialogViewTests
{
    private static AboutDialogView Shown(string copyright = "© 2027 Someone Else")
    {
        IProductInfo productInfo = Substitute.For<IProductInfo>();

        productInfo.Version.Returns("2.4.6");
        productInfo.BuildDateUtc.Returns(new DateOnly(year: 2027, month: 1, day: 9));
        productInfo.Copyright.Returns(copyright);

        AboutDialogView view = new() { DataContext = new AboutDialogViewModel(productInfo) };

        view.Show();

        Dispatcher.UIThread.RunJobs();

        return view;
    }

    [ AvaloniaFact ]
    public void OkButton_IsDefaultAndCancel()
    {
        AboutDialogView view = Shown();

        Button ok = view.GetVisualDescendants()
                        .OfType<Button>()
                        .Single(button => Equals(button.Content, "OK"));

        ok.IsDefault.ShouldBeTrue();
        ok.IsCancel.ShouldBeTrue();
    }

    [ AvaloniaFact ]
    public void LongCopy_WrapsWithinWindowWidth()
    {
        string copyright = string.Concat(Enumerable.Repeat(element: "word ", count: 60))
                                 .TrimEnd();

        copyright.Length.ShouldBe(expected: 299);

        copyright += "!";

        AboutDialogView view = Shown(copyright);

        TextBlock textBlock = view.GetVisualDescendants()
                                  .OfType<TextBlock>()
                                  .Single(block => block.Text == copyright);

        textBlock.TextWrapping.ShouldBe(expected: TextWrapping.Wrap);
        textBlock.Bounds.Height.ShouldBeGreaterThan(textBlock.FontSize * 2);
        view.Bounds.Width.ShouldBe(expected: 430);
    }

    [ AvaloniaFact ]
    public void Logo_IsPrimaryIconAt64()
    {
        AboutDialogView view = Shown();

        Image logo = view.GetVisualDescendants()
                         .OfType<Image>()
                         .Single(image => image.Width == 64);

        logo.Source.ShouldNotBeNull();
        logo.Width.ShouldBe(expected: 64);
        logo.Height.ShouldBe(expected: 64);
    }
}

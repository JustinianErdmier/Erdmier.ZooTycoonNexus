using System.Reflection.Emit;
using System.Xml.Linq;

namespace Erdmier.ZooTycoonNexus.Desktop.Tests.Unit.Composition;

/// <summary>Verifies how <see cref="AssemblyProductInfo"/> reads, normalises and falls back on the assembly's version, build date and copyright.</summary>
public sealed class AssemblyProductInfoTests
{
    [ Theory ]
    [ InlineData("1.2.3+abc123", "1.2.3") ]
    [ InlineData("1.2.3-beta.1+abc", "1.2.3-beta.1") ]
    [ InlineData("1.2.3", "1.2.3") ]
    public void NormaliseVersion_StripsBuildMetadata(string informationalVersion, string expected)
        => AssemblyProductInfo.NormaliseVersion(informationalVersion).ShouldBe(expected: expected);

    [ Fact ]
    public void DesktopAssembly_VersionMatchesDirectoryBuildProps()
    {
        var propsVersion = ReadPropsVersion();

        var info = new AssemblyProductInfo(typeof(App).Assembly, NullLogger<AssemblyProductInfo>.Instance);

        info.Version.ShouldBe(expected: propsVersion);
        info.Version.ShouldNotContain(expected: "+");
    }

    [ Fact ]
    public void DesktopAssembly_BuildDateIsRecent()
    {
        var info = new AssemblyProductInfo(typeof(App).Assembly, NullLogger<AssemblyProductInfo>.Instance);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        info.BuildDateUtc.ShouldNotBe(DateOnly.MinValue);
        info.BuildDateUtc.ShouldBeInRange(today.AddDays(-1), today);
    }

    [ Fact ]
    public void DesktopAssembly_CopyrightIsStamped()
    {
        var info = new AssemblyProductInfo(typeof(App).Assembly, NullLogger<AssemblyProductInfo>.Instance);

        info.Copyright.ShouldStartWith(expected: "©");
    }

    [ Fact ]
    public void MissingAttributes_FallBackAndWarn()
    {
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName: "NoProductInfo"), AssemblyBuilderAccess.Run);
        var logger = new RecordingLogger<AssemblyProductInfo>();

        var info = new AssemblyProductInfo(assembly, logger);

        info.Version.ShouldBe(expected: "0.0.0");
        info.BuildDateUtc.ShouldBe(expected: DateOnly.MinValue);
        info.Copyright.ShouldBe(expected: "");
        logger.Entries.Count.ShouldBe(expected: 3);
        logger.Entries.ShouldAllBe(entry => entry.Level == LogLevel.Warning);
    }

    private static string ReadPropsVersion()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Directory.Build.props")))
            directory = directory.Parent;

        directory.ShouldNotBeNull();

        var document = XDocument.Load(Path.Combine(directory.FullName, "Directory.Build.props"));

        return document.Descendants("Version").Single().Value;
    }
}

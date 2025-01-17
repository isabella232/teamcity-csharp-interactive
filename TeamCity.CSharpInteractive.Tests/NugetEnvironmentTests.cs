namespace TeamCity.CSharpInteractive.Tests;

public class NuGetEnvironmentTests
{
    private readonly Mock<IHostEnvironment> _hostEnvironment;
    private readonly Mock<IEnvironment> _environment;
    private readonly Mock<IUniqueNameGenerator> _uniqueNameGenerator;
    private readonly Mock<ICleaner> _cleaner;
    private readonly Mock<ISettings> _settings;

    public NuGetEnvironmentTests()
    {
        _hostEnvironment = new Mock<IHostEnvironment>();
        _environment = new Mock<IEnvironment>();
        _uniqueNameGenerator = new Mock<IUniqueNameGenerator>();
        _cleaner = new Mock<ICleaner>();
        _settings = new Mock<ISettings>();
    }

    [Fact]
    public void ShouldProvideEmptyFallbackFoldersByDefault()
    {
        // Given
        var instance = CreateInstance();

        // When
        var actualFallbackFolders = instance.FallbackFolders.ToArray();

        // Then
        actualFallbackFolders.ShouldBe(Array.Empty<string>());
    }

    [Fact]
    public void ShouldProvideFallbackFoldersWhenEnvVarNUGET_FALLBACK_PACKAGES()
    {
        // Given
        var instance = CreateInstance();
        _hostEnvironment.Setup(i => i.GetEnvironmentVariable("NUGET_FALLBACK_PACKAGES")).Returns(" path1; Path2");

        // When
        var actualFallbackFolders = instance.FallbackFolders.ToArray();

        // Then
        actualFallbackFolders.ShouldBe(new[] {"path1", "Path2"});
    }

    [Theory]
    [InlineData(null, @"tmp\abc")]
    [InlineData(" ", @"tmp\abc")]
    [InlineData("", @"tmp\abc")]
    [InlineData("Abc", "Abc")]
    [InlineData("  Abc ", "Abc")]
    public void ShouldProvidePackagesPath(string? envVarValue, string expectedPackagesPath)
    {
        // Given
        expectedPackagesPath = expectedPackagesPath.Replace('\\', Path.DirectorySeparatorChar);
        var instance = CreateInstance();
        _environment.Setup(i => i.GetPath(SpecialFolder.Temp)).Returns("tmp");
        _uniqueNameGenerator.Setup(i => i.Generate()).Returns("abc");
        _hostEnvironment.Setup(i => i.GetEnvironmentVariable("NUGET_PACKAGES")).Returns(envVarValue);
        var trackToken = new Mock<IDisposable>();
        var tracking = false;
        _cleaner.Setup(i => i.Track(expectedPackagesPath)).Callback(() => tracking = true).Returns(trackToken.Object);

        // When
        var actualPackagesPath = instance.PackagesPath;
        instance.Dispose();

        // Then
        actualPackagesPath.ShouldBe(expectedPackagesPath);
        if (tracking)
        {
            trackToken.Verify(i => i.Dispose());
        }
    }

    [Fact]
    public void ShouldProvideSources()
    {
        // Given
        var instance = CreateInstance();
        var sources = new[] {"Src1", "Src2"};

        // When
        _settings.SetupGet(i => i.NuGetSources).Returns(sources);
        var actualSources = instance.Sources.ToArray();

        // Then
        actualSources.ShouldBe(new[] {"Src1", "Src2", @"https://api.nuget.org/v3/index.json"});
    }

    private NuGetEnvironment CreateInstance() =>
        new(
            _environment.Object,
            _hostEnvironment.Object,
            _uniqueNameGenerator.Object,
            _cleaner.Object,
            _settings.Object);
}
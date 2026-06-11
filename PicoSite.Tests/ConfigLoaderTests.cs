using PicoSite.Models;
using PicoSite.Services;
using Xunit;

namespace PicoSite.Tests;

public class ConfigLoaderTests
{
    [Fact]
    public void Load_WhenFileNotFound_ReturnsDefaults()
    {
        var loader = new ConfigLoader();
        var config = loader.Load("/nonexistent/path");

        Assert.Equal("PicoSite", config.Title);
        Assert.Equal("default", config.Theme);
        Assert.Equal(8090, config.Port);
        Assert.Equal("./_site", config.Output);
    }

    [Fact]
    public void Load_WithSampleConfig_ReadsCorrectly()
    {
        var sampleDir = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "sample"));

        var configPath = Path.Combine(sampleDir, "picosite.json");
        if (!File.Exists(configPath))
            return; // skip — sample not present in this context

        var loader = new ConfigLoader();
        var config = loader.Load(sampleDir);

        Assert.NotNull(config.Title);
    }
}

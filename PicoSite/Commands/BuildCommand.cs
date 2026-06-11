using System.CommandLine;
using PicoSite.Models;
using PicoSite.Services;

namespace PicoSite.Commands;

public class BuildCommand : Command
{
    public BuildCommand(ConfigLoader configLoader, MarkdownParser markdownParser)
        : base("build", "生成静态文件")
    {
        var outputOption = new Option<string>("--output")
        {
            Description = "输出目录",
        };

        var themeOption = new Option<string>("--theme")
        {
            Description = "指定主题",
        };

        var themeDirOption = new Option<string>("--theme-dir")
        {
            Description = "主题目录路径（默认 exe 同目录下的 Themes/）",
        };

        AddOption(outputOption);
        AddOption(themeOption);
        AddOption(themeDirOption);

        this.SetHandler((string output, string theme, string themeDir) =>
        {
            var config = configLoader.Load(Directory.GetCurrentDirectory());
            if (!string.IsNullOrEmpty(theme)) config.Theme = theme;
            if (!string.IsNullOrEmpty(output)) config.Output = output;
            RunBuild(config, markdownParser, themeDir);
        }, outputOption, themeOption, themeDirOption);
    }

    private static void RunBuild(SiteConfig config, MarkdownParser parser, string? themeDirOverride = null)
    {
        var workingDir = Directory.GetCurrentDirectory();
        var sourceDir = SiteGenerator.FindSourceDir(workingDir);
        var outputDir = Path.GetFullPath(config.Output ?? "./_site");
        var themeDir = !string.IsNullOrEmpty(themeDirOverride)
            ? Path.GetFullPath(themeDirOverride)
            : Path.Combine(AppContext.BaseDirectory, "Themes", config.Theme ?? "default");

        Console.WriteLine("PicoSite 构建");
        Console.WriteLine($"源目录: {sourceDir}");
        Console.WriteLine($"输出目录: {outputDir}");
        Console.WriteLine($"主题: {config.Theme}");
        Console.WriteLine();

        var templates = new TemplateEngine(themeDir);
        var generator = new SiteGenerator(parser, templates, config);

        generator.Build(sourceDir, outputDir);

        var count = Directory.GetFiles(outputDir, "*.html", SearchOption.AllDirectories).Length;
        Console.WriteLine($"完成！共生成 {count} 个页面");
        if (count == 0)
        {
            Console.WriteLine($"提示: 当前源目录 {sourceDir} 下未找到 .md 文件。");
            Console.WriteLine($"      请切换到包含 content/ 的目录（如 sample/），");
            Console.WriteLine($"      或参考 README 创建内容。");
        }
    }
}

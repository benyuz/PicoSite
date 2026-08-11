using PicoSite.Commands;
using PicoSite.Services;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

var configLoader = new ConfigLoader();
var markdownParser = new MarkdownParser();
var serveCommand = new ServeCommand(configLoader, markdownParser);

var root = new RootCommand("PicoSite - 简单轻量的静态站点生成器").VersionCommand();
root.AddCommand(serveCommand);
root.AddCommand(new BuildCommand(configLoader, markdownParser));

// 无参数时默认启动本地预览服务器（与 hugo 等主流静态站点生成器一致），
// 跨平台行为相同：Windows/Linux/macOS 下 `picosite` 均等于 `picosite serve`
root.SetHandler(async (System.CommandLine.Invocation.InvocationContext ctx) =>
{
    if (ctx.ParseResult.CommandResult.Command is RootCommand)
    {
        if (Console.IsOutputRedirected)
            Console.WriteLine("未指定命令，默认启动预览服务器。可用命令: serve / build");
        await serveCommand.InvokeAsync(Array.Empty<string>());
    }
});

var builder = new CommandLineBuilder(root)
    //.UseLocalizationResources(new ChineseLocalization())//汉化
    .UseHelp().Build();

return await builder.InvokeAsync(args);

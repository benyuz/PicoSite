using Markdig;
using Markdig.Extensions.EmphasisExtras;

namespace PicoSite.Services;

public class MarkdownParser
{
    private readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder()
        .UsePipeTables()
        .UseAutoLinks()
        .UseMathematics()   // 公式: $x^2$ / $$...$$（前端由 KaTeX 渲染）
        .UseTaskLists()     // GFM 任务列表: - [x] 已完成
        .UseEmphasisExtras(EmphasisExtraOptions.Strikethrough) // 删除线: ~~文本~~
        .UseEmojiAndSmiley() // emoji: :smile:
        .Build();

    public (Dictionary<string, object>? FrontMatter, string Html) Parse(string markdown)
    {
        Dictionary<string, object>? frontMatter = null;
        var body = markdown;

        // Front Matter: --- 包裹的 YAML 块，兼容 CRLF / LF
        const string sep = "---";
        if (body.StartsWith(sep + "\n") || body.StartsWith(sep + "\r\n"))
        {
            // 前导分隔符长度: "---\n" = 4, "---\r\n" = 5
            int headerLen = body[3] == '\r' ? 5 : 4;

            // 在后继内容中找结東分隔符: \n---\n 或 \n---\r\n 或 \r\n---\n 或 \r\n---\r\n
            int searchLimit = Math.Min(body.Length, 4096);
            int closePos = -1;

            for (int i = headerLen; i < searchLimit - 4; i++)
            {
                if (body[i] == '\r' && body[i + 1] == '\n' &&
                    body[i + 2] == '-' && body[i + 3] == '-' && body[i + 4] == '-')
                {
                    closePos = i;
                    break;
                }
                if (body[i] == '\n' &&
                    body[i + 1] == '-' && body[i + 2] == '-' && body[i + 3] == '-')
                {
                    closePos = i;
                    break;
                }
            }

            if (closePos > 0)
            {
                var yamlBlock = body[headerLen..closePos];
                // 跳过关闭分隔符: \n--- (4) 或 \r\n--- (5)
                int closeLen = body[closePos] == '\r' ? 5 : 4;
                body = body[(closePos + closeLen)..];
                // 剥离 body 开头可能残留的 \r\n 或 \n
                if (body.StartsWith("\r\n")) body = body[2..];
                else if (body.StartsWith("\n")) body = body[1..];

                frontMatter = ParseSimpleYaml(yamlBlock);
            }
        }

        var html = Markdown.ToHtml(body, _pipeline);
        return (frontMatter, html);
    }

    /// <summary>
    /// 简单的 YAML key: value 解析器，AOT 兼容，无需 YamlDotNet。
    /// 支持基本 Front Matter 格式：key: value、key: "quoted"、布尔、数字。
    /// 不支持嵌套结构、数组、多行值等复杂 YAML。
    /// </summary>
    private static Dictionary<string, object>? ParseSimpleYaml(string yaml)
    {
        var result = new Dictionary<string, object>();

        foreach (var line in yaml.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#'))
                continue;

            var colonIndex = trimmed.IndexOf(':');
            if (colonIndex <= 0)
                continue;

            var key = trimmed[..colonIndex].Trim();
            if (string.IsNullOrEmpty(key))
                continue;

            var rawValue = trimmed[(colonIndex + 1)..].Trim();

            // 跳过复杂的 YAML 结构（嵌套、多行）
            if (string.IsNullOrEmpty(rawValue) || rawValue.StartsWith('>') || rawValue.StartsWith('|'))
                continue;

            // 去引号
            if ((rawValue.StartsWith('"') && rawValue.EndsWith('"')) ||
                (rawValue.StartsWith('\'') && rawValue.EndsWith('\'')))
            {
                rawValue = rawValue[1..^1];
                result[key] = rawValue;
                continue;
            }

            // 布尔值
            if (rawValue is "true" or "True" or "TRUE")
            {
                result[key] = true;
                continue;
            }
            if (rawValue is "false" or "False" or "FALSE")
            {
                result[key] = false;
                continue;
            }

            // 数字
            if (long.TryParse(rawValue, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var longVal))
            {
                result[key] = longVal;
                continue;
            }
            if (double.TryParse(rawValue, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var doubleVal))
            {
                result[key] = doubleVal;
                continue;
            }

            // 字符串
            result[key] = rawValue;
        }

        return result.Count > 0 ? result : null;
    }
}

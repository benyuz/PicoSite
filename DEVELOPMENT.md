# PicoSite 开发笔记

> 供后续开发续接使用。保持更新，改动相关章节后同步日期。

- 最后更新：2026-08-07
- 仓库：https://github.com/benyuz/PicoSite
- 文档站：https://benyuz.github.io/PicoSite/

---

## 1. 项目一句话

**PicoSite** — 基于 .NET 10 的零配置静态站点生成器：写 Markdown，运行两条命令（`serve` / `build`），得到一个网站。AOT 编译为 ~10MB 单文件，无运行时依赖。

## 2. 当前状态（v1.3 已发布）

| 里程碑 | 状态 |
|--------|------|
| v1.0 | ✅ 已发布（Release `v1.0.0`，4 平台 AOT 产物） |
| v1.2 | ✅ 已发布（标题搜索、首页摘要、favicon、--baseUrl 等） |
| v1.3 | ✅ 已发布（发布时间/更新时间显示、构建前清空输出目录、主题 UI 优化等） |
| v2.0 | 📋 规划中：插件系统 |

**v1.0 完成的功能：**

- `serve` 热重载预览 / `build` 静态生成
- Markdown + YAML Front Matter
- Liquid 模板引擎（Fluid.Core），主题系统
- 多语言（ISO 语言目录自动检测）
- 导航渲染树 `site.nav`（主题自行决定形态）
- 主题大改：首页聚合 + 文章页双模板 + 暗色模式
- 语言切换器（原语言名显示 + 同页互切）
- 语言级 `site.json`（title/description 按语言区分）
- `baseUrl`（GitHub Pages 子路径部署）
- 404 页、`--version`、`--theme-dir`
- header 社交链接区（github/email 配置驱动）

## 3. 仓库结构

```
PicoSite/                  ← 仓库根（含 PicoSite.sln）
├── PicoSite/              ← 主项目（exe，AssemblyName=picosite）
│   ├── Commands/          ← ServeCommand / BuildCommand / GetVersionCommand
│   ├── Models/            ← SiteConfig / SiteModel / PageModel / NavNode
│   ├── Services/          ← SiteGenerator / TemplateEngine / MarkdownParser / ConfigLoader / HotReloadService
│   └── Themes/default/    ← 默认主题（index/page/header/sidebar/footer/404.html + style.css）
├── PicoSite.Tests/        ← xUnit 测试（21 个）
├── docs-src/              ← 文档站源（content/zh + content/en + picosite.json）
├── sample/                ← 示例站点
└── .github/workflows/     ← ci.yml / release.yml / docs.yml
```

## 4. 架构与数据流

```
Markdown 文件 → Markdig 解析 → Front Matter(YamlDotNet→自研) + HTML
     ↓
SiteGenerator.LoadPages(sourceDir, language) → PageModel 列表
     ↓
SiteModel（Title/Pages/Nav/Language/Languages/BaseUrl/Github/Email）
     ↓
TemplateEngine.RenderForPage(site, page, content)
     ├── 首页(/) → index.html（聚合 site.nav 栏目）
     └── 其他页 → page.html（缺失回退 index）
     └── 注入 site.* / page.* / content / theme.assets
     ↓
Build: 每页写 HTML；Serve: 中间件按需渲染 + WebSocket 热重载
```

**多语言路由：**
- `content/zh/` 默认语言 → URL 无前缀，build 输出到站点根
- `content/en/` → URL `/en/`，build 输出到 `_site/en/`
- 非 ISO 代码目录（blog 等）→ 非语言页面，不受影响
- `defaultLanguage` 配置优先，否则取字母序第一个

## 5. GitHub 资产

| 资产 | 地址 |
|------|------|
| Release | https://github.com/benyuz/PicoSite/releases （v1.0.0 / v1.2.0 / v1.3.0 + 4 平台产物） |
| 文档站 | https://benyuz.github.io/PicoSite/ |
| Actions | ci（build+test）/ release（AOT 4 平台）/ docs（Pages 部署） |
| Issues | 8 个（v1.0 已关；#24 插件挂起） |
| Milestones | v1.0 closed / v2.0 open |

## 6. 关键设计决策

1. **多语言输出**：默认语言输出到站点根（`_site/index.html`），其他语言到子目录——符合静态站点惯例，部署即用。别改回"每语言一子目录"。
2. **baseUrl**：GitHub Pages 项目页部署必须配 `"baseUrl": "/PicoSite/"`，否则绝对链接全 404。注入时 `TrimEnd('/')` 防双斜杠。
3. **导航渲染树 site.nav**：生成器只输出嵌套数据（目录节点 url=null），导航形态由主题决定。`site.pages` 平铺列表保留。
4. **AOT 优先**：`<PublishAot>true</PublishAot>` + `<JsonSerializerIsReflectionEnabledByDefault>false</...>`。JSON 一律用源码生成 context（`PicoSiteJsonContext`）。
5. **模板变量**：一律注入到 `site` 字典，模板用 `{{ site.xxx }}`。
6. **语言切换**：用 `site.current_path`（语言内路径）拼目标语言 URL，同页互切。

## 7. 工作流

| 触发 | 工作流 | 做什么 |
|------|--------|--------|
| push/PR main | ci.yml | restore → build → test（35 个） |
| push tag v* / Release published | release.yml | 自动创建 Release（含发布说明）→ AOT 编译 4 平台（win/linux/macos-x64/arm64）→ 上传产物 |
| push main（docs-src 变更） | docs.yml | build docs-src → 部署 GitHub Pages |

**Pages 设置**：Source = GitHub Actions（不是 branch）。

## 8. 本地开发验证

```bash
dotnet build PicoSite/PicoSite.csproj --configuration Release
dotnet run verify.cs            # 单文件验证（#:project 引用主项目）
dotnet test                     # 某些会话环境可能被 hook 拦截，用 verify.cs 兜底
```

> 开发约定：批量改含中文的文件用 C# 单文件脚本或 write_file，**别用 PowerShell**（5.1 默认 ANSI 会破坏 UTF-8）。

## 9. 开发坑位（详细版在项目记忆 picosite-dev-pitfalls）

1. **AOT 禁用 JSON 反射** → 用源码生成 context；探针/独立项目会掩盖此问题
2. **PowerShell 改文件破坏中文** → 用 write_file / C# 脚本
3. **Fluid 变量作用域** → `{{ site.xxx }}`；空字符串是 truthy，判空用 `!= ""`
4. **GitHub Pages 子路径** → 必须配 baseUrl
5. **AOT 跨平台编译** → 各 OS 用自己的 runner

## 10. 路线图

| 版本 | 内容 | 备注 |
|------|------|------|
| v1.2 | ✅ 已发布：标题搜索、首页卡片摘要（Excerpt/Date/Description）、内联 SVG favicon、主题缺失友好提示、内容宽度对齐 Docusaurus、serve 兼容 .html、build --baseUrl | 已发布 Release `v1.2.0` |
| v1.3 | ✅ 已发布：页面发布时间/更新时间（front matter `date`/`updated`）、文档分页"上一篇/下一篇"修复、构建前自动清空输出目录、首页容器限宽 1320px、卡片标题链接化与 4 列弹性、侧边栏缩窄 260px 与箭头指示修复、分页左右位置固定 | 已发布 Release `v1.3.0` |
| v2.0 | 插件系统 | 接口设计需谨慎，独占版本 |

## 11. 遗留 / 待办

- [x] v1.2：标题搜索、首页摘要、favicon、--baseUrl、主题缺失提示
- [x] v1.3：发布时间/更新时间、分页修复、构建前清空输出目录、主题 UI 优化
- [ ] v2.0：插件系统（#24）
- [ ] docs-src 内容可继续扩充（命令/主题/多语言章节已有）
- [ ] 默认语言切换器在"非语言页面"（如 blog/）上的行为可再打磨

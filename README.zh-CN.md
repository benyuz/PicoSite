[**中文**](README.zh-CN.md) | [**English**](README.md)

---

# PicoSite — 零配置静态站点生成器

> 写 Markdown，运行两条命令，得到一个网站。

传统 SSG 多基于 Node.js 生态，依赖臃肿、构建缓慢。
Go、Rust 生态虽有轻量级工具，但大多沿用「配置驱动+固定主题结构」的模式，学习成本和扩展门槛依然不低。

**PicoSite 的理想是：简单，没负担，好用。**

**下载** 👉 https://github.com/benyuz/PicoSite/releases （选择对应平台的单文件，~10MB）

---

## 快速开始

1. 把 `picosite` 放到 Markdown 文件夹里
2. 运行 `picosite serve` 打开 http://localhost:8090 预览
3. 改内容保存后浏览器自动刷新

发布时运行 `picosite build`，静态文件输出到 `_site/`。

---

## 命令

| 命令 | 说明 |
|------|------|
| `picosite serve` | 启动预览服务器（支持热重载）|
| `picosite build` | 生成静态文件到 `_site/` |

| 选项 | 适用 | 默认 |
|------|------|------|
| `--port 3000` | serve | 8090 |
| `--theme dark` | serve, build | default |
| `--theme-dir ./Themes/default` | serve, build | exe 同目录下 Themes/ 内的主题 |
| `--output ./dist` | build | ./_site |

---

## 编写内容

Markdown 文件放在 `content/` 下，文件路径就是 URL：

```
content/index.md      → /
content/about.md      → /about
content/blog/post.md  → /blog/post
```

文件头部可加 YAML Front Matter：

```markdown
---
title: 文章标题
date: 2026-06-09
---

## 正文

支持 **Markdown** 语法。

### 多语言

`content/` 下目录名为 ISO 语言代码的子目录自动成为语言站点（零配置自动检测）：

```
content/
├── zh/
│   ├── index.md      → /          （默认语言，不带前缀）
│   └── about.md      → /about
├── en/
│   ├── index.md      → /en/
│   └── about.md      → /en/about
└── blog/
    └── post.md       → /blog/post  （非语言目录不受影响）
```

默认语言取字母序第一个检测到的（如 `en`），可在 `picosite.json` 里覆盖：

```json
{ "defaultLanguage": "zh" }
```

`build` 时每语言输出一个子目录：`_site/zh/...`、`_site/en/...`。
模板中可用 `site.language`、`site.languages`、`site.default_language` 渲染语言切换器。
```

---

## 配置（可选）

创建 `picosite.json`：

```json
{
  "title": "我的站点",
  "description": "站点描述",
  "theme": "default",
  "port": 8090,
  "output": "./_site",
  "defaultLanguage": "zh",
  "baseUrl": "/",
  "github": "https://github.com/you/your-site",
  "email": "you@example.com"
}
```

所有字段可选，不配置用默认值：

| 字段 | 默认 | 用途 |
|------|------|------|
| `title` | PicoSite | 站点标题 |
| `description` | — | 站点描述（meta + 首页用） |
| `theme` | default | 主题名 |
| `port` | 8090 | 预览端口 |
| `output` | ./_site | 构建输出目录 |
| `defaultLanguage` | 第一个语言目录 | 默认语言（URL 不带前缀） |
| `baseUrl` | — | 部署子路径，如 GitHub Pages 项目页用 `/PicoSite/` |
| `github` | — | header 显示 GitHub 链接 |
| `email` | — | header 显示邮箱链接 |

---

## 主题系统

主题放在 `Themes/<主题名>/`，结构如下：

```
index.html      # 首页（聚合 site.nav 栏目）
page.html       # 文章/普通页布局（缺失时回退 index.html）
header.html     # 头部片段（logo + 语言切换器）
sidebar.html    # 侧边栏片段（树形导航）
footer.html     # 页脚片段
assets/style.css # 样式
```

模板中可用的变量：

| 变量 | 说明 |
|------|------|
| `{{ site.title }}` | 站点标题 |
| `{{ site.pages }}` | 所有页面（平铺列表） |
| `{{ site.nav }}` | 导航渲染树（嵌套结构，目录节点 url 为 null） |
| `{{ site.language }}` | 当前语言代码 |
| `{{ site.languages }}` | 所有可用语言 |
| `{{ page.title }}` | 当前页面标题 |
| `{{ page.url }}` | 当前页面 URL |
| `{{ page.date }}` | 页面日期 |
| `{{ content }}` | Markdown 渲染后的 HTML |
| `{{ theme.assets }}` | 主题资源路径 |

> ⚠️ `{{ content }}` 需写成 `{{ content | raw }}` 才能渲染 HTML。  
> `{% include %}` 的文件名必须加引号：`{% include "header.html" %}`。

支持 Liquid 标签：`{% include %}` `{% for %}` `{% if %}`。
默认主题支持暗色模式（跟随系统 `prefers-color-scheme`，零 JS）。

---

## 技术栈

**Markdig** 解析 Markdown · **Fluid.Core** 渲染 Liquid 模板 · **PicoServer** 托管 + 热重载 · **System.CommandLine** CLI 框架 · **.NET 10** AOT 编译为 ~10MB 单文件

---

## 路线图

| 版本 | 状态 | 内容 |
|------|------|------|
| v1.0 | ✅ 已发布 | 多语言、树形导航（site.nav）、主题美化（首页聚合 + 暗色模式）、404 页、--version |
| v1.1 | 规划中 | XML 注释驱动的 API 文档生成 |
| v2.0 | 规划中 | 插件系统 |

---

## 开源协议

MIT

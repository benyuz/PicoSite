---
title: 主题开发
---

主题放在 `Themes/<主题名>/`，通过 `--theme` 或 `picosite.json` 的 `theme` 字段选择。

内置的 `default` 主题为 **Docusaurus 风格**：首页全宽展示站点标题、功能卡片与文档栏目聚合（无侧边栏）；文章页带侧边栏树形导航、面包屑与"编辑此页"链接。自带语言切换器、暗色模式、标题搜索（Ctrl+K，按当前语言全部页面标题即时过滤）、代码高亮、KaTeX 数学公式、mermaid 流程图、本页目录（TOC 滚动高亮）、上一篇/下一篇、内联 SVG favicon，并针对移动端自适应。

## 结构

```
index.html       # 首页布局（Hero + 功能卡片 + 文档栏目聚合，无侧边栏）
page.html        # 文章/普通页布局（侧边栏 + 面包屑，缺失时回退 index.html）
header.html      # 头部片段（logo + 语言切换器）
sidebar.html     # 侧边栏片段（树形导航）
footer.html      # 页脚片段
404.html         # 404 页面
assets/style.css # 样式
```

## 模板变量

| 变量 | 说明 |
|------|------|
| `{{ site.title }}` | 站点标题 |
| `{{ site.description }}` | 站点描述 |
| `{{ site.pages }}` | 所有页面（平铺列表，含 Title/Url/Excerpt/Date） |
| `{{ site.nav }}` | 导航渲染树（嵌套，目录节点 url 为空，页面节点含 Description，不含首页） |
| `{{ site.language }}` | 当前语言代码 |
| `{{ site.languages }}` | 所有可用语言（含名称映射） |
| `{{ site.base_url }}` | 部署子路径（如 `/PicoSite/`） |
| `{{ site.github }}` / `{{ site.email }}` | GitHub 仓库链接 / 联系邮箱 |
| `{{ site.current_path }}` | 当前页面的语言内路径（语言切换器用） |
| `{{ theme.assets }}` | 主题静态资源路径 |
| `{{ theme.i18n.xxx }}` | 主题界面文案（按当前语言自动切换中英文） |
| `{{ page.title }}` | 当前页面标题 |
| `{{ page.url }}` | 当前页面 URL |
| `{{ page.date }}` | 页面日期（front matter `date`） |
| `{{ page.excerpt }}` | 页面摘要 |
| `{{ content }}` | Markdown 渲染后的 HTML |

## 动态变量

配置文件与 Front Matter 中的**任意未声明字段**都会自动注入模板，无需修改代码：

- `picosite.json` 自定义字段 → `{{ site.字段名 }}`
- 语言目录 `site.json` 自定义字段 → `{{ site.字段名 }}`（语言级覆盖站点级）
- Front Matter 自定义字段 → `{{ page.字段名 }}`

例如 `picosite.json`：

```json
{ "title": "我的站点", "author": "张三" }
```

主题中即可直接使用 `{{ site.author }}`；同理 front matter：

```markdown
---
title: 文章
author: 张三
---
```

文章页模板中用 `{{ page.author }}` 访问。内置字段（如 `title`、`description`）不会被自定义字段覆盖。

## 注意

- `{{ content }}` 需写成 `{{ content | raw }}` 才能渲染 HTML
- `{% include %}` 的文件名必须加引号：`{% include "header.html" %}`

## 自定义主题路径

用 `--theme-dir` 指定主题目录，可放在站点目录内随站点一起管理：

```bash
picosite serve --theme-dir ./my-theme
```

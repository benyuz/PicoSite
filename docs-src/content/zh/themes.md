---
title: 主题开发
---

# 主题开发

主题放在 `Themes/<主题名>/`，通过 `--theme` 或 `picosite.json` 的 `theme` 字段选择。

## 结构

```
index.html      # 首页（聚合 site.nav 栏目）
page.html       # 文章/普通页布局（缺失时回退 index.html）
header.html     # 头部片段（logo + 语言切换器）
sidebar.html    # 侧边栏片段（树形导航）
footer.html     # 页脚片段
assets/style.css # 样式
```

## 模板变量

| 变量 | 说明 |
|------|------|
| `{{ site.title }}` | 站点标题 |
| `{{ site.pages }}` | 所有页面（平铺列表） |
| `{{ site.nav }}` | 导航渲染树（嵌套，目录节点 url 为 null） |
| `{{ site.language }}` | 当前语言代码 |
| `{{ site.languages }}` | 所有可用语言 |
| `{{ page.title }}` | 当前页面标题 |
| `{{ page.url }}` | 当前页面 URL |
| `{{ content }}` | Markdown 渲染后的 HTML |

## 注意

- `{{ content }}` 需写成 `{{ content | raw }}` 才能渲染 HTML
- `{% include %}` 的文件名必须加引号：`{% include "header.html" %}`

## 自定义主题路径

用 `--theme-dir` 指定主题目录，可放在站点目录内随站点一起管理：

```bash
picosite serve --theme-dir ./my-theme
```

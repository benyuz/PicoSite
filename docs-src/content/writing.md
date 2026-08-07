---
title: 编写内容
---

# 编写内容

## Markdown

Markdown 文件放在 `content/` 下，文件路径就是 URL：

```
content/index.md      → /
content/about.md      → /about
content/blog/post.md  → /blog/post
```

## Front Matter

文件头部可加 YAML Front Matter：

```markdown
---
title: 文章标题
date: 2026-06-09
---

## 正文

支持 **Markdown** 语法。
```

支持字段：`title`、`date`，以及任意自定义字段。

## 多语言

`content/` 下目录名为 ISO 语言代码的子目录自动成为语言站点：

```
content/
├── zh/            → 默认语言，URL 不带前缀
│   └── index.md   → /
├── en/
│   └── index.md   → /en/
└── blog/          → 非语言目录不受影响
```

默认语言取字母序第一个，可在 `picosite.json` 配置：

```json
{ "defaultLanguage": "zh" }
```

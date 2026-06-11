[**中文**](README.zh-CN.md) | [**English**](README.md)

---

# PicoSite — Zero-Config Static Site Generator

> Write Markdown, run two commands, get a website.

Traditional SSGs are built on Node.js — powerful but bloated and slow.
Go/Rust alternatives are lightweight but follow "config-driven + rigid theme" patterns.

**PicoSite aims to be: simple, no burden, just works.**

**Download** 👉 https://github.com/benyuz/PicoSite/releases (single-file ~10MB for your platform)

---

## Quick Start

1. Put `picosite` in your Markdown folder
2. Run `picosite serve` and open http://localhost:8090 to preview
3. Edit your Markdown — browser auto-reloads

To publish, run `picosite build` — static files go to `_site/`.

---

## Commands

| Command | Description |
|---------|-------------|
| `picosite serve` | Start dev server with hot reload |
| `picosite build` | Generate static files to `_site/` |

| Option | Applies to | Default |
|--------|-----------|---------|
| `--port 3000` | serve | 8090 |
| `--theme dark` | serve, build | default |
| `--theme-dir ./Themes/default` | serve, build | exe 同目录下 Themes/ 内的主题 |
| `--output ./dist` | build | ./_site |

---

## Writing Content

Place Markdown files under `content/`. File path = URL:

```
content/index.md      → /
content/about.md      → /about
content/blog/post.md  → /blog/post
```

Add YAML Front Matter at the top:

```markdown
---
title: My Article
date: 2026-06-09
---

## Body

Write **Markdown** here.
```

---

## Configuration (Optional)

Create `picosite.json`:

```json
{
  "title": "My Site",
  "theme": "default",
  "port": 8090
}
```

All fields are optional.

---

## Theme System

Themes live in `Themes/<name>/`:

```
index.html      # Main layout
header.html     # Header partial
sidebar.html    # Sidebar partial
assets/style.css # Styles
```

Available template variables:

| Variable | Description |
|----------|-------------|
| `{{ site.title }}` | Site title |
| `{{ site.pages }}` | All pages |
| `{{ page.title }}` | Current page title |
| `{{ page.url }}` | Current page URL |
| `{{ page.date }}` | Page date |
| `{{ content }}` | Rendered HTML |
| `{{ theme.assets }}` | Theme asset path |

> ⚠️ `{{ content }}` must be written as `{{ content | raw }}` to render HTML.  
> `{% include %}` file names need quotes: `{% include "header.html" %}`.

Supports Liquid tags: `{% include %}` `{% for %}` `{% if %}`.

---

## Tech Stack

**Markdig** parses Markdown · **Fluid.Core** renders Liquid templates · **PicoServer** serves + hot reload · **System.CommandLine** CLI framework · **.NET 10** AOT compiled to ~10MB single file

---

## Roadmap

| Version | What's coming |
|---------|---------------|
| v1.0 | Theme polish + auto sidebar + multi-language + 404 page |
| v1.1 | API docs generation |
| v2.0 | Plugin system |

---

## License

MIT

[**中文**](README.zh-CN.md) | [**English**](README.md)

---

# PicoSite — Zero-Config Static Site Generator

> Write Markdown, run two commands, get a website.

Traditional SSGs are built on Node.js — powerful but bloated and slow.
Go/Rust alternatives are lightweight but follow "config-driven + rigid theme" patterns.

**PicoSite aims to be: simple, no burden, just works.**

**Download** 👉 https://github.com/benyuz/PicoSite/releases (single-file ~10MB for your platform)
**Docs** 👉 https://benyuz.github.io/PicoSite/

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
| `--baseUrl /PicoSite/` | build | baseUrl from picosite.json |

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

### Multi-language

Subdirectories named with ISO language codes under `content/` become language sites
(detected automatically, no config needed):

```
content/
├── zh/
│   ├── index.md      → /          (default language, no prefix)
│   └── about.md      → /about
├── en/
│   ├── index.md      → /en/
│   └── about.md      → /en/about
└── blog/
    └── post.md       → /blog/post  (non-language pages unaffected)
```

The default language is the first one detected alphabetically (e.g. `en`).
Override it in `picosite.json`:

```json
{ "defaultLanguage": "zh" }
```

`build` outputs the default language to the site root (`_site/index.html`),
other languages to subdirectories: `_site/en/...`.
Templates can access `site.language`, `site.languages`, `site.default_language`
to render a language switcher.

---

## Configuration (Optional)

Create `picosite.json`:

```json
{
  "title": "My Site",
  "description": "My site description",
  "theme": "default",
  "port": 8090,
  "output": "./_site",
  "defaultLanguage": "zh",
  "baseUrl": "/",
  "github": "https://github.com/you/your-site",
  "email": "you@example.com"
}
```

All fields are optional:

| Field | Default | Purpose |
|-------|---------|---------|
| `title` | PicoSite | Site title |
| `description` | — | Site description (used in meta + homepage) |
| `theme` | default | Theme name |
| `port` | 8090 | Dev server port |
| `output` | ./_site | Build output dir |
| `defaultLanguage` | first lang dir | Default language (no URL prefix) |
| `baseUrl` | — | Deploy subpath, e.g. `/PicoSite/` for GitHub Pages project sites |
| `github` | — | GitHub link shown in header |
| `email` | — | Email link shown in header |

---

## Theme System

Themes live in `Themes/<name>/`:

```
index.html      # Home page (aggregates sections from site.nav)
page.html       # Article/page layout (falls back to index.html if missing)
header.html     # Header partial (logo + language switcher)
sidebar.html    # Sidebar partial (tree nav)
footer.html     # Footer partial
assets/style.css # Styles
```

Available template variables:

| Variable | Description |
|----------|-------------|
| `{{ site.title }}` | Site title |
| `{{ site.pages }}` | All pages (flat list, with Title/Url/Excerpt/Date) |
| `{{ site.nav }}` | Navigation render tree (nested, dirs have `url = null`, page nodes include Description) |
| `{{ site.language }}` | Current language code |
| `{{ site.languages }}` | All available languages |
| `{{ page.title }}` | Current page title |
| `{{ page.url }}` | Current page URL |
| `{{ page.date }}` | Page date |
| `{{ page.excerpt }}` | Page excerpt (from Front Matter or start of body) |
| `{{ content }}` | Rendered HTML |
| `{{ theme.assets }}` | Theme asset path |

> ⚠️ `{{ content }}` must be written as `{{ content | raw }}` to render HTML.  
> `{% include %}` file names need quotes: `{% include "header.html" %}`.

Supports Liquid tags: `{% include %}` `{% for %}` `{% if %}`.

The default theme (Docusaurus-style) ships with:

- Dark mode via `prefers-color-scheme` (no JS)
- Title search (focus with `Ctrl+K`, live filtering of the current language's pages, match highlighting)
- Code highlighting (highlight.js, loaded on demand) + KaTeX math + mermaid diagrams
- Per-page TOC with scroll-spy, prev/next page links, "Edit this page" (GitHub link)
- Single h1 per page (content no longer uses `# heading`; the template renders it once)
- Inline SVG favicon (no external files)

---

## Tech Stack

**Markdig** parses Markdown · **Fluid.Core** renders Liquid templates · **PicoServer** serves + hot reload · **System.CommandLine** CLI framework · **.NET 10** AOT compiled to ~10MB single file

---

## Roadmap

| Version | Status | What's coming |
|---------|--------|---------------|
| v1.0 | ✅ Released | Multi-language, auto tree nav (`site.nav`), theme polish (home aggregation + dark mode), 404 page, `--version` |
| v1.2 | ✅ Released | Title search, homepage card excerpts (site.pages/nav gain Excerpt/Date/Description), inline SVG favicon, friendly missing-theme error, content width aligned with Docusaurus, serve accepts `.html` URLs, `--baseUrl` override for build |
| ~~v1.1~~ | ❌ Cancelled | ~~API docs generation from XML comments~~ (PicoSite is a general-purpose docs site; XML doc generation is no longer on the roadmap) |
| v2.0 | Planned | Plugin system |

---

## License

MIT

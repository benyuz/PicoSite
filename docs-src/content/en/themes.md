---
title: Theme Development
---

Themes live in `Themes/<theme-name>/`, selected via `--theme` or the `theme` field in `picosite.json`.

The built-in `default` theme is **Docusaurus-style**: the home page is full-width with the site title, feature cards and aggregated doc sections (no sidebar); article pages get a tree sidebar, breadcrumbs and an "Edit this page" link. It ships with a language switcher, dark mode, title search (`Ctrl+K`, live filtering across the current language's pages), code highlighting, KaTeX math, mermaid diagrams, per-page TOC with scroll-spy, prev/next page links, an inline SVG favicon and a mobile-friendly layout.

## Structure

```
index.html       # Home layout (Hero + feature cards + doc sections, no sidebar)
page.html        # Article/page layout (sidebar + breadcrumbs, falls back to index.html if missing)
header.html      # Header partial (logo + language switcher)
sidebar.html     # Sidebar partial (tree nav)
footer.html      # Footer partial
404.html         # 404 page
assets/style.css # Styles
```

## Template variables

| Variable | Description |
|----------|-------------|
| `{{ site.title }}` | Site title |
| `{{ site.description }}` | Site description |
| `{{ site.pages }}` | All pages (flat list, with Title/Url/Excerpt/Date) |
| `{{ site.nav }}` | Navigation render tree (nested, dirs have `url = null`, page nodes include Description) |
| `{{ site.language }}` | Current language code |
| `{{ site.languages }}` | All available languages (with display names) |
| `{{ site.base_url }}` | Deploy subpath (e.g. `/PicoSite/`) |
| `{{ site.github }}` / `{{ site.email }}` | GitHub repo link / contact email |
| `{{ site.current_path }}` | Current page's language-inner path (used by the language switcher) |
| `{{ theme.assets }}` | Theme static assets path |
| `{{ theme.i18n.xxx }}` | Theme UI strings (auto-switches between Chinese and English) |
| `{{ page.title }}` | Current page title |
| `{{ page.url }}` | Current page URL |
| `{{ page.date }}` | Page date (from front matter `date`) |
| `{{ page.excerpt }}` | Page excerpt |
| `{{ content }}` | Rendered HTML |

## Dynamic variables

**Any undeclared fields** in config files and Front Matter are automatically injected into templates — no code changes needed:

- Custom fields in `picosite.json` → `{{ site.fieldName }}`
- Custom fields in a language's `site.json` → `{{ site.fieldName }}` (language-level overrides site-level)
- Custom fields in Front Matter → `{{ page.fieldName }}`

For example, `picosite.json`:

```json
{ "title": "My Site", "author": "John Doe" }
```

The theme can use `{{ site.author }}` directly. Likewise, front matter:

```markdown
---
title: My Article
author: John Doe
---
```

And `{{ page.author }}` is available in article templates. Built-in fields (such as `title`, `description`) are never overridden by custom fields.

## Notes

- `{{ content }}` must be written as `{{ content | raw }}` to render HTML
- `{% include %}` file names need quotes: `{% include "header.html" %}`

## Custom theme path

Use `--theme-dir` to point at a theme directory, e.g. inside your site folder:

```bash
picosite serve --theme-dir ./my-theme
```

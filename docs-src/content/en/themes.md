---
title: Theme Development
---

# Theme Development

Themes live in `Themes/<name>/`, selected via `--theme` or the `theme` field in `picosite.json`.

## Structure

```
index.html      # Home page (aggregates site.nav sections)
page.html       # Article/page layout (falls back to index.html if missing)
header.html     # Header partial (logo + language switcher)
sidebar.html    # Sidebar partial (tree nav)
footer.html     # Footer partial
assets/style.css # Styles
```

## Template variables

| Variable | Description |
|----------|-------------|
| `{{ site.title }}` | Site title |
| `{{ site.pages }}` | All pages (flat list) |
| `{{ site.nav }}` | Navigation render tree (nested, dirs have `url = null`) |
| `{{ site.language }}` | Current language code |
| `{{ site.languages }}` | All available languages |
| `{{ site.base_url }}` | Deploy subpath (e.g. `/PicoSite/`) |
| `{{ page.title }}` | Current page title |
| `{{ page.url }}` | Current page URL |
| `{{ content }}` | Rendered HTML |

## Notes

- `{{ content }}` must be written as `{{ content | raw }}` to render HTML
- `{% include %}` file names need quotes: `{% include "header.html" %}`

## Custom theme path

Use `--theme-dir` to point at a theme directory, e.g. inside your site folder:

```bash
picosite serve --theme-dir ./my-theme
```

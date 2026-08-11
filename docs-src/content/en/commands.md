---
title: Commands
---

## serve — Start the dev server

```bash
picosite serve [--port 8090] [--theme default] [--theme-dir <path>]
```

**Running `picosite` with no arguments is equivalent to `picosite serve`** (same behavior on Windows / Linux / macOS) — handy for a quick preview.

- `--port`: preview port, default 8090
- `--theme`: theme name
- `--theme-dir`: theme directory path (defaults to `Themes/` next to the executable)

## build — Generate static files

```bash
picosite build [--output ./_site] [--theme default] [--theme-dir <path>] [--baseUrl /]
```

- `--output`: output directory, default `./_site`
- `--baseUrl`: override the baseUrl from picosite.json (e.g. `/repo-name/` for GitHub Pages project sites)
- Multi-language sites output one subdirectory per language: `_site/zh/`, `_site/en/`

---
title: PicoSite Static Site Generator
---

**Zero-config static site generator** — Write Markdown, run two commands, get a website.

Why PicoSite:

- **Zero-dependency**: no Node.js / Ruby / Python required — a single file, ready to run
- **Lightweight**: ~10MB AOT binary, far lighter than similar tools
- **Zero-config**: no scaffolding or config files — two commands to launch
- **Fast**: near-instant builds with hot-reload live preview
- **Multi-language**: auto-detected language directories, built-in i18n
- **Rich docs**: syntax highlighting, math, diagrams and site search out of the box

## Quick Start

1. Download the [executable](https://github.com/benyuz/PicoSite/releases) for your platform
2. Put it in your Markdown folder, run `picosite serve`
3. Open http://localhost:8090 to preview

Run `picosite build` to generate static files into `_site/`.

---
title: PicoSite Docs
---

**Zero-config static site generator** — Write Markdown, run two commands, get a website.

- Lightweight, AOT-compiled to a ~10MB single file, no runtime dependencies
- Markdown + YAML Front Matter content
- Liquid template engine with a theme system
- Auto-detected language directories, built-in multi-language support
- Hot-reload dev experience, one-command static generation

## Quick Start

1. Download the [executable](https://github.com/benyuz/PicoSite/releases) for your platform
2. Put it in your Markdown folder, run `picosite serve`
3. Open http://localhost:8090 to preview

Run `picosite build` to generate static files into `_site/`.

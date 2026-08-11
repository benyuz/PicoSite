---
title: PicoSite Static Site Generator
---

**Zero-config static site generator** — Write Markdown, run two commands, get a website.

Why PicoSite:

- **Lightweight**: AOT-compiled to a ~10MB single file, zero runtime dependencies
- **Fast**: near-instant builds and hot-reload live preview
- **Zero-config**: no setup files, two commands to start
- **Multi-language**: auto-detected language directories, built-in i18n support
- **Themable**: Liquid template engine, fully customizable
- **Cross-platform**: single-file distribution for Windows / Linux / macOS

## Quick Start

1. Download the [executable](https://github.com/benyuz/PicoSite/releases) for your platform
2. Put it in your Markdown folder, run `picosite serve`
3. Open http://localhost:8090 to preview

Run `picosite build` to generate static files into `_site/`.

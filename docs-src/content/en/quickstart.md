---
title: Quick Start
---

# Quick Start

## 1. Download

Download the executable for your platform (Windows / Linux / macOS) from the [Releases](https://github.com/benyuz/PicoSite/releases) page.

## 2. Start the dev server

Put `picosite` in your Markdown folder and run:

```bash
picosite serve
```

Open http://localhost:8090 to preview. The browser auto-refreshes on file changes (hot reload).

## 3. Generate static files

```bash
picosite build
```

Static files are output to `_site/`. Deploy them to any static hosting.

## Directory layout

PicoSite auto-detects the source directory: `content/` first, then `docs/`, then the current directory.

```
my-site/
├── content/
│   ├── index.md
│   ├── about.md
│   └── blog/
│       └── post.md
└── picosite
```

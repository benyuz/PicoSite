---
title: Writing Content
---

# Writing Content

## Markdown

Place Markdown files under `content/`. File path = URL:

```
content/index.md      → /
content/about.md      → /about
content/blog/post.md  → /blog/post
```

## Front Matter

Add YAML Front Matter at the top:

```markdown
---
title: My Article
date: 2026-06-09
---

## Body

Write **Markdown** here.
```

Supported fields: `title`, `date`, plus any custom fields.

## Multi-language

Subdirectories named with ISO language codes under `content/` become language sites:

```
content/
├── zh/            → default language, no URL prefix
│   └── index.md   → /
├── en/
│   └── index.md   → /en/
└── blog/          → non-language directory, unaffected
```

The default language is the first one detected alphabetically. Override it in `picosite.json`:

```json
{ "defaultLanguage": "zh" }
```

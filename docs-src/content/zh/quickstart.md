---
title: 快速上手
---

## 1. 下载

从 [Releases](https://github.com/benyuz/PicoSite/releases) 下载对应平台的可执行文件（Windows / Linux / macOS）。

## 2. 启动预览

把 `picosite` 放到 Markdown 文件夹里，运行：

```bash
picosite serve
```

打开 http://localhost:8090 即可预览。修改内容保存后浏览器自动刷新（热重载）。

## 3. 生成静态文件

```bash
picosite build
```

静态文件输出到 `_site/`，部署到任意静态托管即可。

## 目录结构

PicoSite 自动检测源目录：优先 `content/`，其次 `docs/`，最后当前目录。

```
my-site/
├── content/
│   ├── index.md
│   ├── about.md
│   └── blog/
│       └── post.md
└── picosite
```

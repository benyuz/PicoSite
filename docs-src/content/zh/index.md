---
title: PicoSite 文档
---

# PicoSite

**零配置静态站点生成器** — 写 Markdown，运行两条命令，得到一个网站。

- 轻量级，AOT 编译为 ~10MB 单文件，无运行时依赖
- Markdown + YAML Front Matter 编写内容
- Liquid 模板引擎，主题系统
- 自动检测语言目录，内置多语言支持
- 热重载开发体验，一键生成静态文件

## 快速开始

1. 下载对应平台的 [可执行文件](https://github.com/benyuz/PicoSite/releases)
2. 放到 Markdown 文件夹里，运行 `picosite serve`
3. 打开 http://localhost:8090 预览

发布时运行 `picosite build`，静态文件输出到 `_site/`。

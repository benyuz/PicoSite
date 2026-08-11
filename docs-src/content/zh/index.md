---
title: PicoSite 静态站点生成器
---

**零配置静态站点生成器** — 写 Markdown，运行两条命令，得到一个网站。

PicoSite 的特长：

- **轻量**：AOT 编译为 ~10MB 单文件，无运行时依赖
- **高效**：毫秒级构建，热重载即时预览
- **零配置**：无需配置文件，两条命令起步
- **多语言**：自动检测语言目录，内置多语言支持
- **主题化**：Liquid 模板引擎，自由定制外观
- **全平台**：Windows / Linux / macOS 单文件分发

## 快速开始

1. 下载对应平台的 [可执行文件](https://github.com/benyuz/PicoSite/releases)
2. 放到 Markdown 文件夹里，运行 `picosite serve`
3. 打开 http://localhost:8090 预览

发布时运行 `picosite build`，静态文件输出到 `_site/`。

---
title: PicoSite 静态站点生成器
---

**零配置静态站点生成器** — 写 Markdown，运行两条命令，得到一个网站。

PicoSite 的特长：

- **零依赖**：无需 Node.js / Ruby / Python，单文件即装即用
- **轻量**：AOT 编译仅 ~10MB，远轻于同类工具
- **零配置**：没有脚手架和配置文件，两条命令出站
- **高效**：毫秒级构建 + 热重载即时预览
- **多语言**：语言目录自动检测，内置 i18n 支持
- **富文档**：代码高亮、数学公式、图表、站内搜索开箱即用

## 快速开始

1. 下载对应平台的 [可执行文件](https://github.com/benyuz/PicoSite/releases)
2. 放到 Markdown 文件夹里，运行 `picosite serve`
3. 打开 http://localhost:8090 预览

发布时运行 `picosite build`，静态文件输出到 `_site/`。

---
title: 命令参考
---

## serve — 启动预览服务器

```bash
picosite serve [--port 8090] [--theme default] [--theme-dir <path>]
```

**不带任何参数直接运行 `picosite` 等同于 `picosite serve`**（Windows / Linux / macOS 行为一致），方便快速预览。

- `--port`：预览端口，默认 8090
- `--theme`：主题名
- `--theme-dir`：主题目录路径（默认 exe 同目录下 `Themes/`）

## build — 生成静态文件

```bash
picosite build [--output ./_site] [--theme default] [--theme-dir <path>] [--baseUrl /]
```

- `--output`：输出目录，默认 `./_site`
- `--baseUrl`：覆盖 picosite.json 的 baseUrl（如 GitHub Pages 项目页部署传 `/仓库名/`）
- 多语言站点每语言输出一个子目录：`_site/zh/`、`_site/en/`

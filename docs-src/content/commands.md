---
title: 命令参考
---

# 命令参考

## serve — 启动预览服务器

```bash
picosite serve [--port 8090] [--theme default] [--theme-dir <path>]
```

- `--port`：预览端口，默认 8090
- `--theme`：主题名
- `--theme-dir`：主题目录路径（默认 exe 同目录下 `Themes/`）

## build — 生成静态文件

```bash
picosite build [--output ./_site] [--theme default] [--theme-dir <path>]
```

- `--output`：输出目录，默认 `./_site`
- 多语言站点每语言输出一个子目录：`_site/zh/`、`_site/en/`

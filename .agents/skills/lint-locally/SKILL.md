# Skill: Lint format locally

Mục tiêu: chạy `dotnet format whitespace --verify-no-changes` local để match CI lint gate trước khi push.

## Cài dotnet 8 SDK (one-time)

```bash
curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
bash /tmp/dotnet-install.sh --channel 8.0 --install-dir ~/.dotnet
echo 'export PATH=$HOME/.dotnet:$PATH' >> ~/.bashrc
source ~/.bashrc
```

## Verify (matches CI)

```bash
cd /path/to/game
dotnet format whitespace \
  --folder Assets/_Project \
  --include "**/*.cs" \
  --verify-no-changes
```

- Exit 0 = pass.
- Exit 2 = format issues. Output liệt kê file + line cần fix.

## Auto-fix

```bash
dotnet format whitespace --folder Assets/_Project --include "**/*.cs"
```

Apply format vào file. Verify lại trước commit.

## Bash whitespace check (dup CI logic, nhanh)

```bash
# No tab indent
grep -rPl "^\t" Assets/_Project/Scripts Assets/_Project/Tests --include="*.cs" && echo "TAB FOUND"

# No trailing whitespace
grep -rPln "[ \t]+$" Assets/_Project/Scripts Assets/_Project/Tests --include="*.cs"

# No CRLF
grep -rlP $'\r' Assets/_Project/Scripts Assets/_Project/Tests --include="*.cs"
```

## Pitfalls

- **`dotnet format` với --folder** chỉ check whitespace rule subset của `.editorconfig`. Style rule
  (unused-using, naming) cần `.csproj` từ Unity → CI runner cần `UNITY_LICENSE`.
- **Switch arm alignment** bị format về single-space. Đây là "bug nice-to-have" — không có cách bypass
  trừ khi disable `dotnet format` cho file đó.
- **Markdown file**: trim trailing whitespace bị tắt trong `.editorconfig` (trailing 2-space là syntax
  line-break trong Markdown). KHÔNG fix manually nếu thấy.

import re, glob, os

# Historical migration helper. LegacyItems preserves pre-4.5 live-index ordering;
# a snapshot is NOT equivalent when callbacks add or remove entries.
# Review the resulting diff; this limited syntax matcher is not a C# parser.
total = 0
for path in sorted(glob.glob("Shooting/*.cs")):
    text = open(path, encoding="utf-8").read()
    if ".ForEach(" not in text:
        continue
    original = text
    pattern = re.compile(
        r"(?P<target>[A-Za-z_][\w\.\[\]]*)\.ForEach\(\s*delegate\s*\(\s*"
        r"(?P<type>[A-Za-z_][\w\.<>]*)\s+(?P<var>[A-Za-z_]\w*)\s*\)\s*\{"
    )
    pieces = []
    cursor = 0
    for match in pattern.finditer(text):
        if match.start() < cursor:
            continue  # Nested matches belong to the outer replacement; never duplicate source.
        pieces.append(text[cursor : match.start()])
        depth = 1
        scan = match.end()
        while scan < len(text) and depth:
            if text[scan] == "{":
                depth += 1
            elif text[scan] == "}":
                depth -= 1
            scan += 1
        body = text[match.end() : scan - 1]
        tail = scan
        while tail < len(text) and text[tail] in " \t":
            tail += 1
        if tail < len(text) and text[tail] == ")":
            tail += 1
        pieces.append(
            "foreach (%s %s in SlimDX.Compat.ListCompat.LegacyItems(%s)){%s}"
            % (match.group("type"), match.group("var"), match.group("target"), body)
        )
        cursor = tail
        total += 1
    pieces.append(text[cursor:])
    updated = "".join(pieces)
    if updated != original:
        open(path, "w", encoding="utf-8").write(updated)

print("rewrote", total, "ForEach calls")

import re, glob, os

# The game was written against .NET 2.0, whose List<T>.ForEach walked by index and
# tolerated a callback that mutated the list. .NET (Core) uses an enumerator and
# throws InvalidOperationException. Rewriting each call as a snapshot loop restores
# the original semantics: items appended mid-walk run on the next pass.
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
            "foreach (%s %s in SlimDX.Compat.ListCompat.Snapshot(%s)){%s}"
            % (match.group("type"), match.group("var"), match.group("target"), body)
        )
        cursor = tail
        total += 1
    pieces.append(text[cursor:])
    updated = "".join(pieces)
    if updated != original:
        open(path, "w", encoding="utf-8").write(updated)

print("rewrote", total, "ForEach calls")

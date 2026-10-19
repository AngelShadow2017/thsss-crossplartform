// Cross-platform INI reader/writer (replaces kernel32 Get/WritePrivateProfileString).
// Behaviour matched to the Win32 API: keys are case-insensitive, values are trimmed,
// missing sections/keys return the default (empty string), and writes create the file.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Shooting;

public class INI_RW
{
	public string inipath;

	public INI_RW(string INIPath)
	{
		inipath = INIPath;
	}

	public void IniWriteValue(string Section, string Key, string Value)
	{
		var data = Load();
		var section = GetSection(data, Section, create: true);
		var replaced = false;
		for (var i = 0; i < section.Count; i++)
		{
			if (string.Equals(section[i].Key, Key, StringComparison.OrdinalIgnoreCase))
			{
				section[i] = new KeyValuePair<string, string>(section[i].Key, Value ?? string.Empty);
				replaced = true;
				break;
			}
		}

		if (!replaced)
		{
			section.Add(new KeyValuePair<string, string>(Key, Value ?? string.Empty));
		}

		Save(data);
	}

	public string IniReadValue(string Section, string Key)
	{
		var data = Load();
		var section = GetSection(data, Section, create: false);
		if (section == null)
		{
			return string.Empty;
		}

		foreach (var pair in section)
		{
			if (string.Equals(pair.Key, Key, StringComparison.OrdinalIgnoreCase))
			{
				return pair.Value;
			}
		}

		return string.Empty;
	}

	public bool ExistINIFile()
	{
		return File.Exists(inipath);
	}

	private List<(string Name, List<KeyValuePair<string, string>> Entries)> Load()
	{
		var result = new List<(string, List<KeyValuePair<string, string>>)>();
		if (!File.Exists(inipath))
		{
			return result;
		}

		var current = new List<KeyValuePair<string, string>>();
		result.Add((string.Empty, current));

		foreach (var rawLine in File.ReadAllLines(inipath))
		{
			var line = rawLine.Trim();

			if (line.StartsWith(";") || line.StartsWith("#"))
			{
				continue;
			}

			if (line.StartsWith("[") && line.EndsWith("]"))
			{
				current = new List<KeyValuePair<string, string>>();
				result.Add((line.Substring(1, line.Length - 2).Trim(), current));
				continue;
			}

			var separator = line.IndexOf('=');
			if (separator <= 0)
			{
				continue;
			}

			var key = line.Substring(0, separator).Trim();
			var value = line.Substring(separator + 1).Trim();
			current.Add(new KeyValuePair<string, string>(key, value));
		}

		return result;
	}

	private static List<KeyValuePair<string, string>> GetSection(
		List<(string Name, List<KeyValuePair<string, string>> Entries)> data,
		string section,
		bool create)
	{
		foreach (var item in data)
		{
			if (string.Equals(item.Name, section, StringComparison.OrdinalIgnoreCase))
			{
				return item.Entries;
			}
		}

		if (!create)
		{
			return null;
		}

		var entries = new List<KeyValuePair<string, string>>();
		data.Add((section, entries));
		return entries;
	}

	private void Save(List<(string Name, List<KeyValuePair<string, string>> Entries)> data)
	{
		var builder = new StringBuilder();

		foreach (var section in data)
		{
			if (section.Entries.Count == 0 && section.Name.Length == 0)
			{
				continue;
			}

			if (section.Name.Length > 0)
			{
				builder.Append('[').Append(section.Name).Append(']').Append('\n');
			}

			foreach (var pair in section.Entries)
			{
				builder.Append(pair.Key).Append('=').Append(pair.Value).Append('\n');
			}

			builder.Append('\n');
		}

		File.WriteAllText(inipath, builder.ToString());
	}
}

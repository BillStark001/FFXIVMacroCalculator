
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace FfxivMacroCalculator
{
    using DSS = Dictionary<string, string>;
    static public class LanguageFile
    {
        public static readonly Regex KeySearchPattern = new Regex(@"(?:\\[nrtbf\\=&$#]|\\u[0-9a-fA-F]{4}|\\x[0-9a-fA-F]{2}|[^=])*");
        public static readonly Regex KeyReplacePattern = new Regex(@"(?:\\[nrtbf\\=&$#]|\\u[0-9a-fA-F]{4}|\\x[0-9a-fA-F]{2}|[^=])");
        public static readonly Regex ValueReplacePattern = new Regex(@"(?:\\[nrtbf\\]|\\u[0-9a-fA-F]{4}|\\x[0-9a-fA-F]{2})");
        public static string EvaluateMatch(Match match)
        {
            if (match.Length < 2)
                return match.Value;
            switch (match.Value[1])
            {
                case '=':
                case '#':
                case '&':
                case '$':
                    return match.Value[1].ToString();
                case 'n':
                    return "\n";
                case 'r':
                    return "\r";
                case 't':
                    return "\t";
                case 'f':
                    return "\f";
                case 'b':
                    return "\b";
                case '\\':
                    return "\\";
                case 'x':
                    return ((char)(int.Parse(match.Value.Substring(2), System.Globalization.NumberStyles.HexNumber))).ToString();
                case 'u':
                    return char.ConvertFromUtf32(int.Parse(match.Value.Substring(2), System.Globalization.NumberStyles.HexNumber));
                default:
                    throw new InvalidOperationException();
            }
        }
        public static string ParseKey(string str)
        {
            return KeyReplacePattern.Replace(str, EvaluateMatch);
        }

        public static string ParseValue(string str)
        {
            return ValueReplacePattern.Replace(str, EvaluateMatch);
        }

        public static string CreateKey(string str)
        {
            var ret = str
                .Replace("\\", "\\\\")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t")
                .Replace("\f", "\\f")
                .Replace("\b", "\\b")
                .Replace("=", "\\=");
            if (ret.StartsWith('$') || ret.StartsWith('#') || ret.StartsWith('&'))
                ret = "\\" + ret;
            return ret;
        }

        public static string CreateValue(string str)
        {
            var ret = str
                .Replace("\\", "\\\\")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t")
                .Replace("\f", "\\f")
                .Replace("\b", "\\b");
            return ret;
        }
        

        public static DSS Parse(string str, bool allowKeyConflict = true)
        {
            DSS ans = new();
            var strs = str.Split('\n');
            string? lastKey = null;
            for (int i = 0; i < strs.Length; ++i)
            {
                // read and pre-process the current line
                var s = strs[i];
                while (s.EndsWith('\n') || s.EndsWith('\r'))
                    s = s.Trim('\n').Trim('\r');

                // omit comments or white spaces
                if (string.IsNullOrWhiteSpace(s) || s.StartsWith('#'))
                    continue;

                var newKeyDefinedFlag = false;
                string? newKey = null;
                string? newValue = null;
                if (!s.StartsWith('$') && !s.StartsWith('&'))
                {
                    var mayBeKey = KeySearchPattern.Match(s).Groups[0];
                    if (mayBeKey.Length == s.Length)
                    {
                        // do nothing since this line defines no keys
                    }
                    else
                    {
                        var eqInd = mayBeKey.Length; // it must starts from zero point
                        newKey = ParseKey(mayBeKey.Value);
                        newValue = s.Substring(eqInd + 1);
                        newKeyDefinedFlag = true;
                    }
                }

                if (newKeyDefinedFlag)
                {
                    // check conflict
                    if (!allowKeyConflict && ans.ContainsKey(newKey!))
                        throw new InvalidDataException($"Key Conflict: {lastKey} at line {i}");
                    lastKey = newKey;
                    ans[newKey!] = ParseValue(newValue!);
                }
                else
                {
                    if (lastKey == null)
                        lastKey = $"#LINE{i}";
                    if (s.StartsWith('$'))
                        ans[lastKey] += "\n" + ParseValue(s.Substring(1));
                    else if (s.StartsWith('&'))
                        ans[lastKey] += ParseValue(s.Substring(1));
                    else
                        ans[lastKey] += "\n" + ParseValue(s);
                }

            }
            return ans;
        }

        public static string Create(IDictionary<string, string> pairs)
        {
            StringBuilder ret = new();
            foreach (var (key, value) in pairs)
            {

                // process keys
                var keyRes = CreateKey(key);
                ret.Append(keyRes);
                ret.Append("=");
                var firstFlag = false;
                foreach (var v in value.Split('\n'))
                {
                    if (!firstFlag)
                        ret.Append("$");
                    ret.AppendLine(CreateValue(v));
                }
            }
            return ret.ToString();
        }

    }

}



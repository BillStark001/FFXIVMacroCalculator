using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FfxivMacroCalculator
{
    public class Table
    {
        private int _columnCount = 1;
        private Dictionary<string, int> _headerDict = new();
        private Dictionary<int, string> _headerDict2 = new();
        private List<List<string>> _rows = new();

        public enum Option
        {
            LeftAligned = 0, 
            CentralAligned = 1, 
            RightAligned = 2,
        }

        public Table(params string[] header)
        {
            _columnCount = header.Length;
            _headerDict = new();
            for (int i = 0; i < header.Length; ++i)
            {
                _headerDict[header[i]] = i;
                _headerDict2[i] = header[i];
            }
        }

        public void SetColumnName(int column, string name)
        {
            _headerDict[name] = column;
            _headerDict2[column] = name;
            if (column >= _columnCount)
                _columnCount = column + 1;
        }

        public void AddColumn(string name)
        {
            SetColumnName(_columnCount, name);
        }

        public void AddRow(params object?[] objs)
        {
            _rows.Add(objs.Select(x => TableUtils.ToPrettierString(x)).ToList());
            if (objs.Length >= _columnCount)
                _columnCount = objs.Length;
        }

        public void AddRow(IEnumerable<object?> objs)
        {
            _rows.Add(objs.Select(x => TableUtils.ToPrettierString(x)).ToList());
            if (objs.Count() >= _columnCount)
                _columnCount = objs.Count();
        }

        public void AddRow(IDictionary<string, object?> objs)
        {
            var r = new List<string>(objs.Count);
            foreach (var (key, value) in objs)
            {
                var index = -1;
                if (_headerDict.TryGetValue(key, out index)) {
                    // do nothing
                }
                else
                {
                    SetColumnName(_columnCount, key);
                    index = _columnCount - 1;
                }
                r[index] = TableUtils.ToPrettierString(value);
            }
        }

        // to csv
        public override string ToString()
        {
            string[] headers = new string[_columnCount];
            for (int i = 0; i < _columnCount; ++i)
                headers[i] = _headerDict2.TryGetValue(i, out var h) ? h : $"#COLUMN: {i}#";
            StringBuilder ret = new(string.Join(',', headers.Select(x => x.CsvEncode())));
            ret.Append('\n');
            foreach (var row in _rows)
                ret.AppendLine(string.Join(',', row.Select(x => x.CsvEncode())));
            return ret.ToString();
        }

        public string ToString(int maxRowcharCount, int minRowCharCount = 0, Option option = Option.CentralAligned)
        {
            if (maxRowcharCount < 1)
                maxRowcharCount = 1;
            if (minRowCharCount > maxRowcharCount)
                minRowCharCount = maxRowcharCount;

            StringBuilder ret = new();
            string[] headers = new string[_columnCount];
            for (int i = 0; i < _columnCount; ++i)
                headers[i] = _headerDict2.TryGetValue(i, out var h) ? h : $"#COLUMN: {i}#";

            // calculate column width
            int[] columnWidth = new int[_columnCount];
            int r = 0;
            foreach (var row in new List<IEnumerable<string>>() { headers }.Concat(_rows))
            {
                int c = 0;
                foreach (var cell in row)
                {
                    // (r, c)
                    var l = TableUtils.GetConsolasOutputLength(cell);
                    columnWidth[c] = Math.Max(columnWidth[c], l);
                    ++c;
                }
                ++r;
            }
            for (int i = 0; i < _columnCount; ++i)
                columnWidth[i] = Math.Max(Math.Min(columnWidth[i], maxRowcharCount), minRowCharCount);

            // upper frame
            ret.Append('\u250c');
            for (int i = 0; i < _columnCount; ++i)
            {
                for (int _ = 0; _ < columnWidth[i]; ++_)
                    ret.Append('\u2500');
                if (i != _columnCount - 1)
                    ret.Append('\u252c');
            }
            ret.AppendLine("\u2510");
            // header & rows
            var isFirst = false;
            foreach (var row in new List<IEnumerable<string>>() { headers }.Concat(_rows))
            {
                int[] pointer = new int[Math.Min(row.Count(), _columnCount)];
                bool needNext = true;
                while (needNext)
                {
                    needNext = false;
                    ret.Append('\u2502');
                    for (int i = 0; i < pointer.Length; ++i)
                    {
                        pointer[i] = TableUtils.PutString(ret, row.ElementAt(i), pointer[i], columnWidth[i], option);
                        if (pointer[i] < row.ElementAt(i).Length)
                            needNext = true;
                        ret.Append('\u2502');
                    }
                    ret.AppendLine();
                }
                if (!isFirst)
                {
                    isFirst = true;
                    ret.Append('\u251c');
                    for (int i = 0; i < _columnCount; ++i)
                    {
                        for (int _ = 0; _ < columnWidth[i]; ++_)
                            ret.Append('\u2500');
                        if (i != _columnCount - 1)
                            ret.Append('\u253c');
                    }
                    ret.AppendLine("\u2524");
                }
            }

            // lower frame
            ret.Append('\u2514');
            for (int i = 0; i < _columnCount; ++i)
            {
                for (int _ = 0; _ < columnWidth[i]; ++_)
                    ret.Append('\u2500');
                if (i != _columnCount - 1)
                    ret.Append('\u2534');
            }
            ret.AppendLine("\u2518");

            // TODO!!!!

            return ret.ToString();
        }
    }

    public static class TableUtils
    {
        public static void AppendSpace(this StringBuilder sb, int count)
        {
            for (int _ = 0; _ < count; ++_)
                sb.Append(' ');
        }
        
        public static string ToPercentage(this double d, int radix = 2)
        {
            return (d * 100).ToString($"N{(radix > 0 ? radix : 0)}") + "%";
        }

        public static int PutString(StringBuilder target, string str, int startIndex, int length, Table.Option option)
        {
            int charToPut = 0;
            int ret = str.Length;
            for (int i = startIndex; i < str.Length; ++i)
            {
                var l = GetConsolasOutputLength(str[i]);
                if (charToPut + l > length)
                {
                    ret = i;
                    break;
                }
                else
                    charToPut += l;
            }
            var substr = str.Substring(startIndex, ret - startIndex);
            switch (option)
            {
                case Table.Option.LeftAligned:
                    target.Append(substr);
                    target.AppendSpace(length - charToPut);
                    break;
                case Table.Option.RightAligned:
                    target.AppendSpace(length - charToPut);
                    target.Append(substr);
                    break;
                case Table.Option.CentralAligned:
                    int mid = (length - charToPut) / 2;
                    target.AppendSpace(mid);
                    target.Append(substr);
                    target.AppendSpace(length - charToPut - mid);
                    break;
            }
            return ret;
        }

        public static string ToPrettierString(this object? obj)
        {
            if (obj == null)
                return "null";
            else
            {
                if (obj is float f)
                    return f.ToString("0.000");
                else if (obj is double d)
                    return d.ToString("0.000");
                else
                    return obj.ToString() ?? "";
            }
        }

        public static string CsvEncode(this string str)
        {
            if (str.Contains(','))
                return "\"" + str.Replace("\"", "\"\"") + "\"";
            return str;
        }

        public static int GetConsolasOutputLength(char chr)
        {
            return (int)chr > 0x2fff ? 2 : 1;
        }
        public static int GetConsolasOutputLength(string str)
        {
            var ans = 0;
            foreach (var chr in str)
                ans += GetConsolasOutputLength(chr); // maybe very slow
            return ans;
        }

    }
}

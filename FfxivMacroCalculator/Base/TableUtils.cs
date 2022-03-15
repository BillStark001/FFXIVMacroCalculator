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
            _rows.Add(objs.Select(x => x.ToString() ?? "").ToList());
            if (objs.Length >= _columnCount)
                _columnCount = objs.Length;
        }

        public void AddRow(IEnumerable<object?> objs)
        {
            _rows.Add(objs.Select(x => x.ToString() ?? "").ToList());
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
                r[index] = (value ?? "").ToString() ?? "";
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

        public string ToString(uint maxRowcharCount, uint minRowCharCount = 0)
        {
            StringBuilder ret = new();
            int[] columnWidth = new int[_columnCount];
            int[] rowHeight = new int[_rows.Count + 1];
            string[] headers = new string[_columnCount];
            for (int i = 0; i < _columnCount; ++i)
                headers[i] = _headerDict2.TryGetValue(i, out var h) ? h : $"#COLUMN: {i}#";
            int r = 0;
            foreach (var row in new List<IEnumerable<string>>() { headers }.Concat(_rows))
            {
                int c = 0;
                foreach (var cell in row)
                {
                    // (r, c)
                    var l = TableUtils.GetConsolasOutputLength(cell);
                    var ld = l / (int) maxRowcharCount;
                    var lr = l % (int) maxRowcharCount;
                    rowHeight[r] = Math.Max(rowHeight[r], ld + 1);
                    columnWidth[r] = Math.Max(columnWidth[r], ld > 0 ? (int)maxRowcharCount : Math.Max(lr, (int)minRowCharCount));
                    ++c;
                }
                ++r;
            }
            // TODO!!!!
            
            return ret.ToString();
        }
    }

    public static class TableUtils
    {
        public static string CsvEncode(this string str)
        {
            if (str.Contains(','))
                return "\"" + str.Replace("\"", "\"\"") + "\"";
            return str;
        }

        public static int GetConsolasOutputLength(char chr)
        {
            return Encoding.Default.GetBytes(new[] { chr }).Length == 1 ? 1 : 2;
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

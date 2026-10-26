using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace GameOfLife
{
    public static class Life106Util
    {
        public static HashSet<Cell> Parse(IEnumerable<string> lines)
        {
            var set = new HashSet<Cell>(); // duplicates in the input collapse naturally
            int lineNo = 0;

            foreach (var raw in lines)
            {
                lineNo++;
                var line = raw.Trim();
                if (line.Length == 0 || line[0] == '#') continue; // header / comments

                var parts = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2 ||
                    !long.TryParse(parts[0], NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out long x) ||
                    !long.TryParse(parts[1], NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out long y))
                {
                    throw new FormatException(
                        $"Line {lineNo}: expected two signed 64-bit integers, got '{line}'");
                }
                set.Add(new Cell(x, y));
            }
            return set;
        }

        // Streamed so a huge result never has to exist as one giant string in memory.
        public static void Write(TextWriter writer, HashSet<Cell> cells)
        {
            // Sorted output -> deterministic, diff-friendly results.
            var list = new List<Cell>(cells);
            list.Sort((a, b) => a.X != b.X ? a.X.CompareTo(b.X) : a.Y.CompareTo(b.Y));

            writer.Write("#Life 1.06\n");
            foreach (var cell in list)
            {
                writer.Write(cell.X.ToString(CultureInfo.InvariantCulture));
                writer.Write(' ');
                writer.Write(cell.Y.ToString(CultureInfo.InvariantCulture));
                writer.Write('\n');
            }
        }

        // Convenience wrapper for tests; prefer Write for real output.
        public static string Format(HashSet<Cell> cells)
        {
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                Write(writer, cells);
                return writer.ToString();
            }
        }
    }
}

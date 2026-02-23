using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace UziSport.Service
{
    public static class EscPos
    {
        public static byte[] Init() => new byte[] { 0x1B, 0x40 };                 // ESC @
        public static byte[] AlignLeft() => new byte[] { 0x1B, 0x61, 0x00 };
        public static byte[] AlignCenter() => new byte[] { 0x1B, 0x61, 0x01 };
        public static byte[] AlignRight() => new byte[] { 0x1B, 0x61, 0x02 };

        public static byte[] BoldOn() => new byte[] { 0x1B, 0x45, 0x01 };
        public static byte[] BoldOff() => new byte[] { 0x1B, 0x45, 0x00 };

        public static byte[] DoubleOn() => new byte[] { 0x1D, 0x21, 0x11 };       // GS ! (W x2, H x2)
        public static byte[] DoubleOff() => new byte[] { 0x1D, 0x21, 0x00 };

        public static byte[] Feed(int n) => new byte[] { 0x1B, 0x64, (byte)n };   // ESC d n
        public static byte[] CutFull() => new byte[] { 0x1D, 0x56, 0x00 };        // GS V 0
        public static byte[] Beep() => new byte[] { 0x1B, 0x42, 0x03, 0x02 };     // tuỳ máy

        public static byte[] Text(string s)
            => Encoding.UTF8.GetBytes(s ?? string.Empty);

        public static byte[] NewLine() => Text("\n");

        public static string Line(int charsPerLine, char c = '-')
            => new string(c, Math.Max(0, charsPerLine)) + "\n";

        public static string PadLR(string left, string right, int width)
        {
            left ??= "";
            right ??= "";

            if (left.Length + right.Length >= width)
            {
                // cắt bớt left nếu quá dài
                int maxLeft = Math.Max(0, width - right.Length - 1);
                if (left.Length > maxLeft) left = left.Substring(0, maxLeft);
                return left + " " + right;
            }
            return left + new string(' ', width - left.Length - right.Length) + right;
        }

        public static IEnumerable<string> Wrap(string text, int width)
        {
            text ??= "";
            var words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var line = "";
            foreach (var w in words)
            {
                if ((line.Length == 0 ? w.Length : line.Length + 1 + w.Length) <= width)
                    line = (line.Length == 0) ? w : line + " " + w;
                else
                {
                    yield return line;
                    line = w;
                }
            }
            if (line.Length > 0) yield return line;
        }
    }
}

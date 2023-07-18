using System;
using OpenMacroBoard.SDK;
using StreamDeckSharp;

namespace ManagedDoom.Video
{
    internal class ElgatoRenderer
    {
        //private Palette palette;
        private uint[] colors;
        private IMacroBoard deck;
        private byte[] data;
        private const int width = 3 * 64;
        private const int height = 3 * 64;
        private int[] buttons = new int[] { 1, 2, 3, 6, 7, 8, 11, 12, 13 };

        public ElgatoRenderer(Palette palette)
        {
            colors = palette[0];
            deck = StreamDeck.OpenDevice(null);
            deck.SetBrightness(100);
            data = new byte[width * height * 3];
        }

        public void DrawPatch(Patch patchesFace)
        {
            data = new byte[width * height * 3];
            DrawPatch(patchesFace, 0, 0, 6);

            for (int i = 0; i < height / 64; i++)
            {
                for (int j = 0; j < width / 64; j++)
                {
                    var buttonData = Copy(data, i, j, 3);
                    var faceBitmap = new KeyBitmap(64, 64, buttonData);

                    deck.SetKeyBitmap(buttons[i*3+j], faceBitmap);
                }
            }
        }

        private byte[] Copy(byte[] allData, int y, int x, int n)
        {
            //return data;
            var result = new byte[64 * 64 * 3];
            var offset = n * 64 * 3 * y * 64 + x * 64 * 3;
            for (int k = 0; k < 64; k++)
            {
                Array.Copy(allData, offset + k * n * 64 * 3, result, k * 64 * 3, 64 * 3);
            }

            return result;
        }

        public void DrawPatch(Patch patch, int x, int y, int scale)
        {
            var drawX = x - scale * patch.LeftOffset;
            var drawY = y - scale * patch.TopOffset;
            var drawWidth = scale * patch.Width;

            var i = 0;
            var frac = Fixed.One / scale - Fixed.Epsilon;
            var step = Fixed.One / scale;

            if (drawX < 0)
            {
                var exceed = -drawX;
                frac += exceed * step;
                i += exceed;
            }

            if (drawX + drawWidth > width)
            {
                var exceed = drawX + drawWidth - width;
                drawWidth -= exceed;
            }

            for (; i < drawWidth; i++)
            {
                DrawColumn(patch.Columns[frac.ToIntFloor()], drawX + i, drawY, scale);
                frac += step;
            }
        }

        private void DrawColumn(Column[] source, int x, int y, int scale)
        {
            var step = Fixed.One / scale;

            foreach (var column in source)
            {
                var exTopDelta = scale * column.TopDelta;
                var exLength = scale * column.Length;

                var sourceIndex = column.Offset;
                var drawY = y + exTopDelta;
                var drawLength = exLength;

                var i = 0;
                var p = height * x + drawY;
                var frac = Fixed.One / scale - Fixed.Epsilon;

                if (drawY < 0)
                {
                    var exceed = -drawY;
                    p += exceed;
                    frac += exceed * step;
                    i += exceed;
                }

                if (drawY + drawLength > height)
                {
                    var exceed = drawY + drawLength - height;
                    drawLength -= exceed;
                }

                for (; i < drawLength; i++)
                {
                    var c = column.Data[sourceIndex + frac.ToIntFloor()];
                    var rgb = colors[c];
                    var pos = (y * width + x) * 3;
                    data[pos+2] = (byte)(rgb & 255);
                    data[pos+1] = (byte)((rgb >> 8) & 255);
                    data[pos] = (byte)((rgb >> 16) & 255);
                    y++;
                    p++;

                    frac += step;
                }
            }
        }
    }
}

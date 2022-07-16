using GameEngine._2D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    internal class TextDescription : Description2D
    {
        private string text;
        private Bitmap bitmap;
        public TextDescription(string text, int x, int y, int size = 18) : base(null, x, y, text.Length * size, 24 * (1 + text.Count(ch => ch == '\n')))
        {
            this.text = text;
            this.DrawAction = Draw;
            bitmap = Bitmap.Create(text.Length * size, 24 * (1 + text.Count(ch => ch == '\n')));
            bitmap.GetGraphics().DrawText(text, new Point(0, 0), Color.Black, size);
        }

        private Bitmap Draw()
        {
            return bitmap;
        }

        public void ChangeText(string text)
        {
            this.text = text;
            bitmap.GetGraphics().Clear(Color.Transparent);
            bitmap.GetGraphics().DrawText(text, new Point(0, 0), Color.Black, 18);
        }

    }
}

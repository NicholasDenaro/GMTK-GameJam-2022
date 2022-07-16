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
        public TextDescription(string text, int x, int y) : base(null, x, y, 100, 24)
        {
            this.text = text;
            this.DrawAction = Draw;
            bitmap = Bitmap.Create(100, 24);
            bitmap.GetGraphics().DrawText(text, new Point(0, 0), Color.Black, 18);
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

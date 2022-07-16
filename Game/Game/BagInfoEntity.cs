using GameEngine;
using GameEngine._2D;
using GameEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    internal class BagInfoEntity : Entity
    {
        private List<Entity> diceInfo = new List<Entity>();
        private Description2D description;

        private bool isShown;
        public BagInfoEntity(int x, int y) : base(new Description2D(Sprite.Sprites["Scorecard"], x, y, 80, 80))
        {
            this.description = this.Description as Description2D;
            this.description.ZIndex = 10;
        }

        public void Display(List<Dice> diceList)
        {
            if (!isShown)
            {
                Program.GameLocation.AddEntity(this);
                for (int i = 0; i < diceList.Count(); i++)
                {
                    Dice dice = diceList[i];
                    int x = (i % 5) * 16;
                    int y = (i / 5) * 16;
                    Description2D description = new Description2D(Sprite.Sprites["dice"], (int)this.description.X + 16 + x, (int)this.description.Y + 16 + y, 16, 16);

                    description.ZIndex = 11;
                    Entity entity = new Entity(description);
                    description.ImageIndex = ((Description2D)dice.Description).ImageIndex;
                    diceInfo.Add(entity);
                    Program.GameLocation.AddEntity(entity);
                }


                isShown = true;
            }
        }

        public void Hide()
        {
            if (isShown)
            {
                Program.GameLocation.RemoveEntity(this.Id);

                foreach (Entity entity in diceInfo)
                {
                    Program.GameLocation.RemoveEntity(entity.Id);
                }

                diceInfo.Clear();
                isShown = false;
            }
        }
    }
}

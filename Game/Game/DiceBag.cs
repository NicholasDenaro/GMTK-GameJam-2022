using GameEngine;
using GameEngine._2D;
using GameEngine.Interfaces;
using GameEngine.UI.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Game.Program;

namespace Game
{
    internal class DiceBag : Entity
    {
        private Description2D description;
        private List<Dice> dice = new List<Dice>();
        private bool mouseDown;

        public DiceBag(int x, int y) : base(new Description2D(Sprite.Sprites["DiceBag"], x, y))
        {
            this.description = this.Description as Description2D;

        }

        public override void Tick(GameState state)
        {
            if (state.Controllers[0][Keys.CLICK].IsPress())
            {
                MouseControllerInfo info = state.Controllers[0][Keys.CLICK].Info as MouseControllerInfo;
                if (description.IsCollision(new Description2D(info.X + this.description.Sprite.X, info.Y + this.description.Sprite.Y, 1, 1)))
                {
                    mouseDown = true;
                }
            }

            if (mouseDown && state.Controllers[0][Keys.CLICK].IsDown())
            {
                MouseControllerInfo info = state.Controllers[0][Keys.CLICK].Info as MouseControllerInfo;
                if (!description.IsCollision(new Description2D(info.X + this.description.Sprite.X, info.Y + this.description.Sprite.Y, 1, 1)))
                {
                    mouseDown = false;
                    if (info.X + this.description.Sprite.X > this.description.X
                        && info.X + this.description.Sprite.X < this.description.X + this.description.Width
                        && info.Y + this.description.Sprite.Y < this.description.Y)
                    {
                        if (this.dice.Any() && GameRules.CanDrawAnotherDice())
                        {
                            DrawDice().Spawn(info.X, info.Y, true);
                        }
                    }
                }
            }
        }

        public void AddDice(Dice dice)
        {
            this.dice.Add(dice);
        }

        public void RemoveDice(Dice dice)
        {
            this.dice.Remove(dice);
        }


        public Dice DrawDice()
        {
            int index = Program.Random.Next(this.dice.Count);
            var ret = this.dice[index];
            this.dice.RemoveAt(index);

            return ret;
        }
    }
}

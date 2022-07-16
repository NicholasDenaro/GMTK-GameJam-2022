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
        private bool mouseDragging;

        private TextDescription descriptionDice;
        private Entity diceEntity;

        private Entity diceSymbolEntity;

        private BagInfoEntity infoEntity;
        private bool isInfoShown;

        public DiceBag(int x, int y) : base(new Description2D(Sprite.Sprites["DiceBag"], x, y))
        {
            this.description = this.Description as Description2D;
            string diceText = $"{this.dice.Count()}/{this.dice.Count() + GameRules.CurrentDiceInPlay}";
            Program.GameLocation.AddEntity(this.diceEntity = new Entity(this.descriptionDice = new TextDescription(diceText, x + 80 - diceText.Length * 12, y + description.Height - 4)));
            Program.GameLocation.AddEntity(this.diceSymbolEntity = new Entity(new Description2D(Sprite.Sprites["Symbols"], x + 80, y + description.Height + 8)));
            ((Description2D)this.diceSymbolEntity.Description).ImageIndex = 3;

            infoEntity = new BagInfoEntity(x - 100, y);
        }

        public override void Tick(GameState state)
        {
            if (state.Controllers[0][Keys.CLICK].IsPress())
            {
                MouseControllerInfo info = state.Controllers[0][Keys.CLICK].Info as MouseControllerInfo;
                if (description.IsCollision(new Description2D(info.X + this.description.Sprite.X, info.Y + this.description.Sprite.Y, 1, 1)))
                {
                    mouseDragging = true;
                }
            }

            if (mouseDragging && state.Controllers[0][Keys.CLICK].IsDown())
            {
                MouseControllerInfo info = state.Controllers[0][Keys.CLICK].Info as MouseControllerInfo;
                if (!description.IsCollision(new Description2D(info.X + this.description.Sprite.X, info.Y + this.description.Sprite.Y, 1, 1)))
                {
                    mouseDragging = false;
                    if (info.X + this.description.Sprite.X > this.description.X
                        && info.X + this.description.Sprite.X < this.description.X + this.description.Width
                        && info.Y + this.description.Sprite.Y < this.description.Y)
                    {
                        if (this.dice.Any() && GameRules.CanDrawAnotherDice())
                        {
                            DrawDice().Spawn(info.X, info.Y, true);
                        }
                    }
                    
                    this.infoEntity.Hide();
                    this.isInfoShown = false;
                }
            }

            if (!mouseDragging)
            {
                MouseControllerInfo info = state.Controllers[0][Keys.MOUSEINFO].Info as MouseControllerInfo;
                if (!state.Controllers[0][Keys.CLICK].IsDown() && !this.isInfoShown && this.description.IsCollision(new Description2D(info.X + this.description.Sprite.X, info.Y + this.description.Sprite.Y, 1, 1)))
                {
                    this.infoEntity.Display(dice);
                    this.isInfoShown = true;


                }
                else if (this.isInfoShown && !this.description.IsCollision(new Description2D(info.X + this.description.Sprite.X, info.Y + this.description.Sprite.Y, 1, 1)))
                {
                    this.infoEntity.Hide();
                    this.isInfoShown = false;
                }
            }
        }

        public void AddDice(Dice dice)
        {
            this.dice.Add(dice);

            string diceText = $"{this.dice.Count()}/{this.dice.Count() + GameRules.NumberOfDiceInPlay()}";
            this.descriptionDice.ChangeText(diceText);
            this.descriptionDice.SetCoords(this.description.X + 80 - diceText.Length * 12, this.description.Y + description.Height - 4);
        }

        public void RemoveDice(Dice dice)
        {
            this.dice.Remove(dice);

            string diceText = $"{this.dice.Count()}/{this.dice.Count() + GameRules.NumberOfDiceInPlay()}";
            this.descriptionDice.ChangeText(diceText);
            this.descriptionDice.SetCoords(this.description.X + 80 - diceText.Length * 12, this.description.Y + description.Height - 4);
        }


        public Dice DrawDice()
        {
            int index = Program.Random.Next(this.dice.Count);
            var ret = this.dice[index];
            this.dice.RemoveAt(index);


            // TODO: +1 is a big hack
            string diceText = $"{this.dice.Count()}/{this.dice.Count() + GameRules.NumberOfDiceInPlay() + 1}";
            this.descriptionDice.ChangeText(diceText);
            this.descriptionDice.SetCoords(this.description.X + 80 - diceText.Length * 12, this.description.Y + description.Height - 4);

            return ret;
        }
    }
}

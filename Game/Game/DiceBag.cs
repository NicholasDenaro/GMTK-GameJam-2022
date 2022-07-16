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
    internal class DiceBag : Entity
    {
        private List<Dice> dice = new List<Dice>();

        public DiceBag(int x, int y) : base(new Description2D(Sprite.Sprites["DiceBag"], x, y))
        {

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

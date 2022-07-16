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
    internal class DiceInfoEntity : Entity
    {
        private Description2D description;

        private Entity swordCountEntity;
        private Entity swordIconEntity;

        private Entity shieldCountEntity;
        private Entity shieldIconEntity;

        private Entity bowCountEntity;
        private Entity bowIconEntity;

        private Entity healCountEntity;
        private Entity healIconEntity;

        private Entity rollsCountEntity;
        private Entity rollsIconEntity;

        private bool isShown;
        public DiceInfoEntity(Dice dice, int x, int y) : base(new Description2D(Sprite.Sprites["Scorecard"], x, y, 40, 80))
        {
            this.description = this.Description as Description2D;
            this.description.ZIndex = 10;

            Description2D d2d;
            swordCountEntity = new Entity(d2d = new TextDescription($"{dice.Faces.Count(face => face == (int)Program.Faces.SWORD)}", x + 16, y + 16 * 0, 12));
            d2d.ZIndex = 12;
            swordIconEntity = new Entity(d2d = new Description2D(Sprite.Sprites["diceFaces"], x + 8, y + 16 * 0 + 8));
            d2d.ImageIndex = (int)Program.Faces.SWORD;
            d2d.ZIndex = 12;

            shieldCountEntity = new Entity(d2d = new TextDescription($"{dice.Faces.Count(face => face == (int)Program.Faces.SHIELD)}", x + 16, y + 16 * 1, 12));
            d2d.ZIndex = 12;
            shieldIconEntity = new Entity(d2d = new Description2D(Sprite.Sprites["diceFaces"], x + 8, y + 16 * 1 + 8));
            d2d.ImageIndex = (int)Program.Faces.SHIELD;
            d2d.ZIndex = 12;

            bowCountEntity = new Entity(d2d = new TextDescription($"{dice.Faces.Count(face => face == (int)Program.Faces.BOW)}", x + 16, y + 16 * 2, 12));
            d2d.ZIndex = 12;
            bowIconEntity = new Entity(d2d = new Description2D(Sprite.Sprites["diceFaces"], x + 8, y + 16 * 2 + 8));
            d2d.ImageIndex = (int)Program.Faces.BOW;
            d2d.ZIndex = 12;

            healCountEntity = new Entity(d2d = new TextDescription($"{dice.Faces.Count(face => face == (int)Program.Faces.HEAL)}", x + 16, y + 16 * 3, 12));
            d2d.ZIndex = 12;
            healIconEntity = new Entity(d2d = new Description2D(Sprite.Sprites["diceFaces"], x + 8, y + 16 * 3 + 8));
            d2d.ImageIndex = (int)Program.Faces.HEAL;
            d2d.ZIndex = 12;

            rollsCountEntity = new Entity(d2d = new TextDescription($"{dice.NumRolls}", x + 16, y + 16 * 4, 12));
            d2d.ZIndex = 12;
            rollsIconEntity = new Entity(d2d = new Description2D(Sprite.Sprites["Symbols"], x + 8, y + 16 * 4 + 8));
            d2d.ImageIndex = 4;
            d2d.ZIndex = 12;
        }

        public void SetCoords(double x, double y)
        {
            this.ChangeCoordsDelta(x - this.description.X, y - this.description.Y);
        }

        public void ChangeCoordsDelta(double dx, double dy)
        {
            this.description.ChangeCoordsDelta(dx, dy);

            (this.swordCountEntity.Description as Description2D).ChangeCoordsDelta(dx, dy);
            (this.swordIconEntity.Description as Description2D).ChangeCoordsDelta(dx, dy);

            (this.shieldCountEntity.Description as Description2D).ChangeCoordsDelta(dx, dy);
            (this.shieldIconEntity.Description as Description2D).ChangeCoordsDelta(dx, dy);

            (this.bowCountEntity.Description as Description2D).ChangeCoordsDelta(dx, dy);
            (this.bowIconEntity.Description as Description2D).ChangeCoordsDelta(dx, dy);

            (this.healCountEntity.Description as Description2D).ChangeCoordsDelta(dx, dy);
            (this.healIconEntity.Description as Description2D).ChangeCoordsDelta(dx, dy);

            (this.rollsCountEntity.Description as Description2D).ChangeCoordsDelta(dx, dy);
            (this.rollsIconEntity.Description as Description2D).ChangeCoordsDelta(dx, dy);
        }

        public void Display()
        {
            if (!isShown)
            {
                Program.GameLocation.AddEntity(this);

                Program.GameLocation.AddEntity(swordCountEntity);
                Program.GameLocation.AddEntity(swordIconEntity);

                Program.GameLocation.AddEntity(shieldCountEntity);
                Program.GameLocation.AddEntity(shieldIconEntity);

                Program.GameLocation.AddEntity(bowCountEntity);
                Program.GameLocation.AddEntity(bowIconEntity);

                Program.GameLocation.AddEntity(healCountEntity);
                Program.GameLocation.AddEntity(healIconEntity);

                Program.GameLocation.AddEntity(rollsCountEntity);
                Program.GameLocation.AddEntity(rollsIconEntity);

                isShown = true;
            }
        }

        public void Hide()
        {
            if (isShown)
            {
                Program.GameLocation.RemoveEntity(this.Id);

                Program.GameLocation.RemoveEntity(swordCountEntity.Id);
                Program.GameLocation.RemoveEntity(swordIconEntity.Id);

                Program.GameLocation.RemoveEntity(shieldCountEntity.Id);
                Program.GameLocation.RemoveEntity(shieldIconEntity.Id);

                Program.GameLocation.RemoveEntity(bowCountEntity.Id);
                Program.GameLocation.RemoveEntity(bowIconEntity.Id);

                Program.GameLocation.RemoveEntity(healCountEntity.Id);
                Program.GameLocation.RemoveEntity(healIconEntity.Id);

                Program.GameLocation.RemoveEntity(rollsCountEntity.Id);
                Program.GameLocation.RemoveEntity(rollsIconEntity.Id);

                isShown = false;
            }
        }
    }
}

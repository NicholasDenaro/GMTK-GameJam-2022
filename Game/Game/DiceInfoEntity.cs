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

        public void Display(Location location = null)
        {
            if (location == null)
            {
                location = Program.GameLocation;
            }

            if (!isShown)
            {
                location.AddEntity(this);

                location.AddEntity(swordCountEntity);
                location.AddEntity(swordIconEntity);

                location.AddEntity(shieldCountEntity);
                location.AddEntity(shieldIconEntity);

                location.AddEntity(bowCountEntity);
                location.AddEntity(bowIconEntity);

                location.AddEntity(healCountEntity);
                location.AddEntity(healIconEntity);

                location.AddEntity(rollsCountEntity);
                location.AddEntity(rollsIconEntity);

                isShown = true;
            }
        }

        public void Hide(Location location = null)
        {
            if (location == null)
            {
                location = Program.GameLocation;
            }

            if (isShown)
            {
                location.RemoveEntity(this.Id);

                location.RemoveEntity(swordCountEntity.Id);
                location.RemoveEntity(swordIconEntity.Id);

                location.RemoveEntity(shieldCountEntity.Id);
                location.RemoveEntity(shieldIconEntity.Id);

                location.RemoveEntity(bowCountEntity.Id);
                location.RemoveEntity(bowIconEntity.Id);

                location.RemoveEntity(healCountEntity.Id);
                location.RemoveEntity(healIconEntity.Id);

                location.RemoveEntity(rollsCountEntity.Id);
                location.RemoveEntity(rollsIconEntity.Id);

                isShown = false;
            }
        }
    }
}

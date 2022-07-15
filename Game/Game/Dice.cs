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
    internal class Dice : Entity
    {
        private Description2D description;
        private Description2D descriptionFace;
        private Entity diceFace;
        private bool rolling;
        private int rollingTime;

        private int[] faces;
        private int index;
        private bool held;

        public static Dice Create(GameEngine.GameEngine engine, int state, (Sides sides, Colors color, Faces[] faces) info, int x, int y)
        {
            return Create(engine, state, info.sides, info.color, info.faces, x, y);
        }

        private static Dice Create(GameEngine.GameEngine engine, int state, Sides sides, Colors color, Faces[] faces, int x, int y)
        {
            var dice = new Dice((int)sides, (int)color, faces.Select(f => (int)f).ToArray(), x, y);
            engine.AddEntity(state, dice);
            engine.AddEntity(state, dice.diceFace);

            return dice;
        }

        private Dice(int sides, int color, int[] faces, int x, int y) : base(new Description2D(Sprite.Sprites["dice"], x, y, 32, 32))
        {
            this.description = this.Description as Description2D;
            this.diceFace = new Entity(this.descriptionFace = new Description2D(Sprite.Sprites["diceFaces"], x, y, 10, 10));
            this.faces = faces;
            this.descriptionFace.ImageIndex = this.faces[index];
            this.description.ImageIndex = sidesToIndex(sides) + color * 6;
        }

        private static int sidesToIndex(int sides)
        {
            switch(sides)
            {
                case 4:
                    return 0;
                case 6:
                    return 1;
                case 8:
                    return 2;
                case 10:
                    return 3;
                case 12:
                    return 4;
                case 20:
                    return 5;
                default:
                    return 0;
            }
        }

        public void Roll()
        {
            this.rolling = true;
            this.rollingTime = Program.Random.Next(2 * Program.FPS, 3 * Program.FPS);
        }

        private Point lastMousePos;
        public override void Tick(GameState state)
        {
            if (!rolling && state.Controllers[0][Keys.RCLICK].IsPress())
            {
                MouseControllerInfo info = state.Controllers[0][Keys.RCLICK].Info as MouseControllerInfo;
                if (description.IsCollision(new Description2D(info.X + this.description.Sprite.X, info.Y + this.description.Sprite.Y, 1, 1)))
                {
                    this.held = true;
                    lastMousePos = new Point(info.X, info.Y);
                }
            }

            if (this.held && state.Controllers[0][Keys.RCLICK].IsDown())
            {
                MouseControllerInfo info = state.Controllers[0][Keys.RCLICK].Info as MouseControllerInfo;

                this.description.ChangeCoordsDelta(info.X - lastMousePos.X, info.Y - lastMousePos.Y);
                this.descriptionFace.ChangeCoordsDelta(info.X - lastMousePos.X, info.Y - lastMousePos.Y);

                lastMousePos = new Point(info.X, info.Y);
            }

            if (this.held && !state.Controllers[0][Keys.RCLICK].IsDown())
            {
                MouseControllerInfo info = state.Controllers[0][Keys.RCLICK].Info as MouseControllerInfo;
                if (description.IsCollision(new Description2D(info.X + this.description.Sprite.X, info.Y + this.description.Sprite.Y, 1, 1)))
                {
                    this.held = false;
                }
            }

            if (this.rolling && this.rollingTime-- > 0)
            {
                if (this.rollingTime > 2 * Program.FPS && this.rollingTime % 2 == 0)
                {
                    descriptionFace.ImageIndex = this.faces[index++ % this.faces.Length];
                }
                else if (this.rollingTime > 1 * Program.FPS && this.rollingTime % 3 == 0)
                {
                    descriptionFace.ImageIndex = this.faces[index++ % this.faces.Length];
                }
                else if (this.rollingTime > 1.5 * Program.FPS && this.rollingTime % 6 == 0)
                {
                    descriptionFace.ImageIndex = this.faces[index++ % this.faces.Length];
                }
                else if (this.rollingTime > 1 * Program.FPS && this.rollingTime % 8 == 0)
                {
                    descriptionFace.ImageIndex = this.faces[index++ % this.faces.Length];
                }
                else if (this.rollingTime % 10 == 0)
                {
                    descriptionFace.ImageIndex = this.faces[index++ % this.faces.Length];
                }
            }
            else
            {
                this.rolling = false;
            }
        }
    }
}

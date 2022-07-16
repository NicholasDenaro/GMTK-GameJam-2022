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

        private TextDescription descriptionHealth;
        private Entity healthEntity;

        private bool rolling;
        private int rollingTime = -1;

        private int[] faces;
        private int index;
        private bool held;
        private (double x, double y) avgVel = (0, 0);
        private (double x, double y) velocity;
        public const double minVelocity = 0.01;

        private bool showHealth = false;

        public static Dice Create((Sides sides, Colors color, Faces[] faces, int health) info, int x, int y)
        {
            return Create(info.sides, info.color, info.faces, info.health, x, y);
        }

        private static Dice Create(Sides sides, Colors color, Faces[] faces, int health, int x, int y)
        {
            var dice = new Dice((int)sides, (int)color, faces.Select(f => (int)f).ToArray(), health, x, y);
            Program.Engine.AddEntity(Program.GameState, dice);
            Program.Engine.AddEntity(Program.GameState, dice.diceFace);
            //Program.Engine.AddEntity(Program.GameState, dice.healthEntity);

            return dice;
        }

        private Dice(int sides, int color, int[] faces, int health, int x, int y) : base(new Description2D(Sprite.Sprites["dice"], x, y, 32, 32))
        {
            this.description = this.Description as Description2D;
            this.diceFace = new Entity(this.descriptionFace = new Description2D(Sprite.Sprites["diceFaces"], x, y + FaceOffset(sides), 10, 10));
            this.healthEntity = new Entity(this.descriptionHealth = new TextDescription(health <= 3 ? new string('♥', health) : $"{health}♥", x + (health <= 3 ? -6 * health : -12 + (health >= 10 ? -6 : 0) ), y + 16));

            this.faces = faces;
            this.descriptionFace.ImageIndex = this.faces[index];
            this.description.ImageIndex = sidesToIndex(sides) + color * 6;
            velocity = (0, 0);
        }

        private static int FaceOffset(int sides)
        {
            switch (sides)
            {
                case 4:
                    return 5;
                case 6:
                    return 1;
                case 8:
                    return 2;
                case 10:
                    return 1;
                case 12:
                    return 1;
                case 20:
                    return 3;
                default:
                    return 0;
            }
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
            // Pickup
            if (!rolling && state.Controllers[0][Keys.CLICK].IsPress())
            {
                MouseControllerInfo info = state.Controllers[0][Keys.CLICK].Info as MouseControllerInfo;
                if (description.IsCollision(new Description2D(info.X + this.description.Sprite.X, info.Y + this.description.Sprite.Y, 1, 1)))
                {
                    this.held = true;
                    lastMousePos = new Point(info.X, info.Y);
                    showHealth = true;
                    state.Location.AddEntity(healthEntity);
                }
            }

            // Throw
            if (this.held && !state.Controllers[0][Keys.CLICK].IsDown())
            {
                MouseControllerInfo info = state.Controllers[0][Keys.CLICK].Info as MouseControllerInfo;
                this.held = false;
                if (avgVel.x * avgVel.x + avgVel.y * avgVel.y > 16)
                {
                    this.velocity = avgVel;
                    this.rollingTime = (int)Math.Log(minVelocity / Math.Max(Math.Abs(this.velocity.x), Math.Abs(this.velocity.y)), 0.95);
                    this.rolling = true;
                }

                showHealth = false;
                state.Location.RemoveEntity(healthEntity.Id);
            }

            // Move/Shake
            if (this.held && state.Controllers[0][Keys.CLICK].IsDown())
            {
                MouseControllerInfo info = state.Controllers[0][Keys.CLICK].Info as MouseControllerInfo;

                this.description.ChangeCoordsDelta(info.X - lastMousePos.X, info.Y - lastMousePos.Y);
                this.descriptionFace.ChangeCoordsDelta(info.X - lastMousePos.X, info.Y - lastMousePos.Y);
                this.descriptionHealth.ChangeCoordsDelta(info.X - lastMousePos.X, info.Y - lastMousePos.Y);

                avgVel = (avgVel.x * 0.8 + (info.X - lastMousePos.X) * 0.2, avgVel.y * 0.8 + (info.Y - lastMousePos.Y) * 0.2);
                lastMousePos = new Point(info.X, info.Y);
            }

            // Rolling/Movement
            if (Math.Abs(this.velocity.x) - minVelocity > 0 || Math.Abs(this.velocity.y) - minVelocity > 0)
            {
                this.description.ChangeCoordsDelta(this.velocity.x, this.velocity.y);
                this.descriptionFace.ChangeCoordsDelta(this.velocity.x, this.velocity.y);
                this.descriptionHealth.ChangeCoordsDelta(this.velocity.x, this.velocity.y);
                velocity.x *= 0.95;
                velocity.y *= 0.95;
                if (!(Math.Abs(this.velocity.x) - minVelocity > 0 || Math.Abs(this.velocity.y) - minVelocity > 0))
                {
                    Console.WriteLine("stopped moving");
                }
            }

            // Face rolling
            if (this.rolling && --this.rollingTime > 0)
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
            else if (this.rollingTime == 0)
            {
                this.rolling = false;
                this.rollingTime = -1;
                Console.WriteLine("finished rolling");
            }
        }
    }
}

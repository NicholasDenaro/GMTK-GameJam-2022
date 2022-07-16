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
        public const double minVelocity = 0.01;

        private Description2D description;

        private Description2D descriptionFace;
        private Entity diceFace;

        private TextDescription descriptionHealth;
        private Entity healthEntity;

        private Description2D descriptionSymbol;
        private Entity symbolEntity;

        private int sides;
        private int[] faces;
        private int index;
        private int health;

        public bool IsRolling { get; private set; }
        private bool showHealth;
        private bool held;
        public bool IsLocked { get; private set; }

        private int rollingTime = -1;
        private (double x, double y) avgVel = (0, 0);
        private (double x, double y) velocity;



        public static Dice Create((Sides sides, Colors color, Faces[] faces, int health) info, int x, int y)
        {
            return Create(info.sides, info.color, info.faces, info.health, x, y);
        }

        private static Dice Create(Sides sides, Colors color, Faces[] faces, int health, int x, int y)
        {
            var dice = new Dice((int)sides, (int)color, faces.Select(f => (int)f).ToArray(), health, x, y);

            return dice;
        }

        private Dice(int sides, int color, int[] faces, int health, int x, int y) : base(new Description2D(Sprite.Sprites["dice"], x, y, 32, 32))
        {
            this.description = this.Description as Description2D;
            this.diceFace = new Entity(this.descriptionFace = new Description2D(Sprite.Sprites["diceFaces"], x, y + FaceOffset(sides), 10, 10));
            this.healthEntity = new Entity(this.descriptionHealth = new TextDescription(health <= 3 ? new string('♥', health) : $"{health}♥", x + (health <= 3 ? -6 * health : -12 + (health >= 10 ? -6 : 0) ), y + 16));
            this.symbolEntity = new Entity(this.descriptionSymbol = new Description2D(Sprite.Sprites["Symbols"], x, y - 20));

            this.health = health;
            this.sides = sides;
            this.faces = faces;
            this.descriptionFace.ImageIndex = this.faces[index];
            this.description.ImageIndex = sidesToIndex(sides) + color * 6;
            velocity = (0, 0);
        }

        private void resetPositions()
        {
            this.descriptionFace.SetCoords(this.description.X, this.description.Y + FaceOffset(sides));
            this.descriptionHealth.SetCoords(this.description.X + (health <= 3 ? -6 * health : -12 + (health >= 10 ? -6 : 0)), this.description.Y + 16);
            this.descriptionSymbol.SetCoords(this.description.X, this.description.Y - 20);
        }

        public void Spawn(int x, int y, bool held)

        {
            this.description.SetCoords(x, y);
            this.resetPositions();
            Program.Engine.AddEntity(Program.GameState, this);
            Program.Engine.AddEntity(Program.GameState, this.diceFace);
            Program.Engine.AddEntity(Program.GameState, this.symbolEntity);

            this.held = held;
            MouseControllerInfo info = Program.Engine.Controllers(Program.GameState)[0][Keys.CLICK].Info as MouseControllerInfo;

            lastMousePos = new Point(info.X, info.Y);
        }

        public void Despawn()
        {
            Program.Engine.Location(Program.GameState).RemoveEntity(this.Id);
            Program.Engine.Location(Program.GameState).RemoveEntity(this.diceFace.Id);
            if (this.showHealth)
            {
                Program.Engine.Location(Program.GameState).RemoveEntity(this.healthEntity.Id);
            }
            Program.Engine.Location(Program.GameState).RemoveEntity(this.symbolEntity.Id);
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

        public void Roll(bool withVelocity = false)
        {
            if (this.IsLocked)
            {
                return;
            }

            this.IsRolling = true;
            if (withVelocity)
            {
                double angle = Program.Random.NextDouble() * Math.PI * 2;
                this.velocity = (Math.Cos(angle) * 8, Math.Sin(angle) * 8);
                this.setRollingTime();
            }
        }

        private void setRollingTime()
        {
            this.rollingTime = (int)Math.Log(minVelocity / Math.Max(Math.Abs(this.velocity.x), Math.Abs(this.velocity.y)), 0.95);
        }

        private Point lastMousePos;
        public override void Tick(GameState state)
        {
            // Pickup
            if (!IsRolling && state.Controllers[0][Keys.CLICK].IsPress())
            {
                MouseControllerInfo info = state.Controllers[0][Keys.CLICK].Info as MouseControllerInfo;
                if (description.IsCollision(new Description2D(info.X + this.description.Sprite.X, info.Y + this.description.Sprite.Y, 1, 1)))
                {
                    this.held = true;
                    lastMousePos = new Point(info.X, info.Y);
                    if (!showHealth)
                    {
                        showHealth = true;
                        state.Location.AddEntity(healthEntity);
                    }
                }
            }

            // Lock
            if (GameRules.RollsLeft != GameRules.MaxRolls && !IsRolling && state.Controllers[0][Keys.RCLICK].IsPress())
            {
                MouseControllerInfo info = state.Controllers[0][Keys.RCLICK].Info as MouseControllerInfo;
                if (description.IsCollision(new Description2D(info.X + this.description.Sprite.X, info.Y + this.description.Sprite.Y, 1, 1)))
                {
                    this.IsLocked = !this.IsLocked;
                    if (this.IsLocked)
                    {
                        this.descriptionSymbol.ImageIndex = 2;
                    }
                    else
                    {
                        this.descriptionSymbol.ImageIndex = 0;
                    }
                }
            }

            // Show health if hovering and not held
            if (!held)
            {
                if (!showHealth)
                {
                    MouseControllerInfo info = state.Controllers[0][Keys.MOUSEINFO].Info as MouseControllerInfo;
                    if (info != null && description.IsCollision(new Description2D(info.X + this.description.Sprite.X, info.Y + this.description.Sprite.Y, 1, 1)))
                    {
                        showHealth = true;
                        state.Location.AddEntity(healthEntity);
                    }
                }
                else
                {
                    MouseControllerInfo info = state.Controllers[0][Keys.MOUSEINFO].Info as MouseControllerInfo;
                    if (info != null && !description.IsCollision(new Description2D(info.X + this.description.Sprite.X, info.Y + this.description.Sprite.Y, 1, 1)))
                    {
                        showHealth = false;
                        state.Location.RemoveEntity(healthEntity.Id);
                    }
                }
            }

            // Throw
            if (this.held && !state.Controllers[0][Keys.CLICK].IsDown())
            {
                MouseControllerInfo info = state.Controllers[0][Keys.CLICK].Info as MouseControllerInfo;
                this.held = false;
                //if (!IsLocked && avgVel.x * avgVel.x + avgVel.y * avgVel.y > 16)
                //{
                //    this.velocity = avgVel;
                //    this.setRollingTime();
                //    this.IsRolling = true;
                //}

                showHealth = false;
                state.Location.RemoveEntity(healthEntity.Id);
            }

            // Move/Shake/Hold
            if (this.held && state.Controllers[0][Keys.CLICK].IsDown())
            {
                MouseControllerInfo info = state.Controllers[0][Keys.CLICK].Info as MouseControllerInfo;

                ChangeCoordsDelta(info.X - lastMousePos.X, info.Y - lastMousePos.Y);

                avgVel = (avgVel.x * 0.8 + (info.X - lastMousePos.X) * 0.2, avgVel.y * 0.8 + (info.Y - lastMousePos.Y) * 0.2);
                lastMousePos = new Point(info.X, info.Y);
            }

            // Rolling/Movement
            if (Math.Abs(this.velocity.x) - minVelocity > 0 || Math.Abs(this.velocity.y) - minVelocity > 0)
            {
                ChangeCoordsDelta(this.velocity.x, 0);

                if (state.Location.Entities.Where(entity => entity is Dice && entity != this).Select(entity => entity as Dice).Any(dice => dice.description.IsCollision(this.description))
                    || this.description.X < 16
                    || this.description.X > Program.Width - 16)
                {
                    ChangeCoordsDelta(-this.velocity.x, 0);
                    velocity.x *= -1;
                }


                ChangeCoordsDelta(0, this.velocity.y);

                if (state.Location.Entities.Where(entity => entity is Dice && entity != this).Select(entity => entity as Dice).Any(dice => dice.description.IsCollision(this.description))
                    || this.description.Y < 16
                    || this.description.Y > Program.Height - 16)
                {
                    ChangeCoordsDelta(0, -this.velocity.y);
                    velocity.y *= -1;
                }

                velocity.x *= 0.95;
                velocity.y *= 0.95;
                if (!(Math.Abs(this.velocity.x) - minVelocity > 0 || Math.Abs(this.velocity.y) - minVelocity > 0))
                {
                    Console.WriteLine("stopped moving");
                }
            }

            // Face rolling
            if (this.IsRolling && --this.rollingTime > 0)
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
                this.IsRolling = false;
                this.rollingTime = -1;
                Console.WriteLine("finished rolling");
            }
        }

        private void ChangeCoordsDelta(double dx, double dy)
        {
            this.description.ChangeCoordsDelta(dx, dy);
            this.descriptionFace.ChangeCoordsDelta(dx, dy);
            this.descriptionHealth.ChangeCoordsDelta(dx, dy);
            this.descriptionSymbol.ChangeCoordsDelta(dx, dy);
        }
    }
}

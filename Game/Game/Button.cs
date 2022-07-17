using GameEngine;
using GameEngine._2D;
using GameEngine.Interfaces;
using GameEngine.UI.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    internal class Button : Entity
    {
        private Entity textEntity;
        private bool held;
        private Action action;
        public bool Enabled { get; set; } = true;

        public Button(Location location, string text, int x, int y, Action action) : base(new Description2D(Sprite.Sprites["Button"], x, y))
        {
            Description2D d2d;
            textEntity = new Entity(d2d = new TextDescription(text, x + 10, y + 2));
            d2d.ZIndex = 2;
            location.AddEntity(textEntity);
            this.action = action;
        }

        public override void Tick(GameState state)
        {
            if (state.Controllers.Count == 0)
            {
                return;
            }

            if (!state.Location.Entities.Contains(textEntity))
            {
                state.Location.AddEntity(textEntity);
            }

            if (Enabled && state.Controllers[0][Program.Keys.CLICK].IsPress())
            {
                Description2D description = this.Description as Description2D;
                MouseControllerInfo info = state.Controllers[0][Program.Keys.CLICK].Info as MouseControllerInfo;
                if (description.IsCollision(new Description2D(info.X + description.Sprite.X, info.Y + description.Sprite.Y, 1, 1)))
                {
                    this.held = true;
                    description.ImageIndex = 1;
                }
            }
            else if (this.held && !state.Controllers[0][Program.Keys.CLICK].IsDown())
            {
                Description2D description = this.Description as Description2D;
                MouseControllerInfo info = state.Controllers[0][Program.Keys.CLICK].Info as MouseControllerInfo;
                if (description.IsCollision(new Description2D(info.X + description.Sprite.X, info.Y + description.Sprite.Y, 1, 1)))
                {
                    this.action();
                }

                description.ImageIndex = 0;
                this.held = false;
            }
        }
    }
}

using GameEngine;
using GameEngine._2D;
using GameEngine.Interfaces;
using GameEngine.UI.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Game
{
    internal class Scorecard : Entity
    {
        private Sheet[] sheets;
        int level;

        public Scorecard(int x, int y) : base(new Description2D(Sprite.Sprites["Scorecard"], x, y))
        {
            sheets = GenerateRun();
            level = 0;
        }

        public void LoadSheet(Location location)
        {
            sheets[level].Display(location);
        }

        private Sheet[] GenerateRun()
        {
            var description = this.Description as Description2D;
            return new Sheet[] { new Sheet((int)description.X , (int)description.Y) };
        }
    }

    class Sheet
    {
        private Quest[] quests;
        private SideQuest[] sideQuests;
           
        public Sheet(int x , int y)
        {
            int xoffset = 36;
            int yoffset = 40;
            int yy = 0;

            quests = new Quest[]
            {
                new Quest("1♥ 1⸸", x + xoffset, y + yoffset + yy++ * 20),
                new Quest("2♥ 1⸸", x + xoffset, y + yoffset + yy++ * 20),
                new Quest("3♥ 2⸸", x + xoffset, y + yoffset + yy++ * 20),
                new Quest("4♥ 2⸸", x + xoffset, y + yoffset + yy++ * 20),
                new Quest("5♥ 2⸸", x + xoffset, y + yoffset + yy++ * 20),
                new Quest("6♥ 3⸸", x + xoffset, y + yoffset + yy++ * 20),
            };

            yoffset += yy * 20;
            yy = 0;
            sideQuests = new SideQuest[]
            {
                new SideQuest("Shop", x + xoffset, y + yoffset + yy++ * 19),
                new SideQuest("Rest", x + xoffset, y + yoffset + yy++ * 19),
                new SideQuest("Upgrade", x + xoffset, y + yoffset + yy++ * 19),
                new SideQuest("Recruit", x + xoffset, y + yoffset + yy++ * 19),
            };
        }

        public void Display(Location location)
        {
            for (int i = 0; i < quests.Length; i++)
            {
                quests[i].Display(location);
            }

            for (int i = 0; i < sideQuests.Length; i++)
            {
                sideQuests[i].Display(location);
            }
        }
    }

    class Quest
    {
        public string Text { get; private set; }
        public bool IsFinished { get; protected set; }

        private Entity checkBoxEntity;
        private Entity textEntity;

        public Quest(string name, int x, int y)
        {
            Text = name;

            textEntity = new Entity(new TextDescription(Text, x, y));
            checkBoxEntity = new Entity(new Description2D(Sprite.Sprites["Symbols"], x - 16, y + 12));
            ((Description2D)checkBoxEntity.Description).ImageIndex = 0;
        }

        public void Display(Location location)
        {
            location.AddEntity(textEntity);
            location.AddEntity(checkBoxEntity);

            Program.Engine.TickEnd(Program.GameState) += Tick;
        }

        protected virtual bool CanStart()
        {
            return GameRules.RollsLeft != GameRules.MaxRolls && GameRules.GetDiceInPlay().Any(dice => dice.IsLocked);
        }

        protected virtual void GoOnQuest()
        {
            List<Dice> diceToBattle = Program.GameLocation.Entities.Where(entity => entity is Dice && (entity as Dice).IsLocked).Select(entity => entity as Dice).ToList();
            List<Dice> diceToBag = Program.GameLocation.Entities.Where(entity => entity is Dice && !(entity as Dice).IsLocked).Select(entity => entity as Dice).ToList();
            foreach (Dice dice in diceToBag)
            {
                dice.Despawn();
                Program.DiceBag.AddDice(dice);
            }

            foreach (Dice dice in diceToBattle)
            {
                dice.Despawn();
            }

            Program.Engine.SetLocation(Program.GameState, Program.BattleLocation);
            Regex r = new Regex("(?<health>[0-9]+).*(?<attack>[0-9]+)");
            var match = r.Match(Text);
            GameRules.InitBattle(diceToBattle, int.Parse(match.Groups["health"].Value), int.Parse(match.Groups["attack"].Value));

        }

        private void Tick(object sender, GameState state)
        {
            var description = checkBoxEntity.Description as Description2D;
            if (((Description2D)checkBoxEntity.Description).ImageIndex == 5 && CanStart() && state.Controllers[0][Program.Keys.CLICK].IsPress())
            {
                MouseControllerInfo info = state.Controllers[0][Program.Keys.CLICK].Info as MouseControllerInfo;
                if (description.IsCollision(new Description2D(info.X + description.Sprite.X, info.Y + description.Sprite.Y, 1, 1)))
                {
                    ((Description2D)checkBoxEntity.Description).ImageIndex = 6;
                    GoOnQuest();
                }
            }

            if (CanStart())
            {
                if (((Description2D)checkBoxEntity.Description).ImageIndex == 0)
                {
                    ((Description2D)checkBoxEntity.Description).ImageIndex = 5;
                }
            }
            else
            {
                if (((Description2D)checkBoxEntity.Description).ImageIndex == 5)
                {
                    ((Description2D)checkBoxEntity.Description).ImageIndex = 0;
                }
            }
        }
    }

    class SideQuest : Quest
    {
        private string name;

        public SideQuest(string name, int x, int y) : base(name, x, y)
        {
            this.name = name;
        }

        protected override bool CanStart()
        {
            return name == "Shop" || name == "Recruit" ||  GameRules.CurrentDiceInPlay > 0;
        }

        protected override void GoOnQuest()
        {
            switch(name)
            {
                case "Shop":
                    GoToTheShop();
                    break;
                case "Rest":
                    TakeARest();
                    break;
                case "Upgrade":
                    UpgradeTheDice();
                    break;
                case "Recruit":
                    RecruitMoreDice();
                    break;
            }
        }

        private void GoToTheShop()
        {
            GameRules.OpenShop();
        }
        private void TakeARest()
        {
            List<Dice> diceList = Program.GameLocation.Entities.Where(entity => entity is Dice).Select(entity => entity as Dice).ToList();
            GameRules.HealDice(diceList);
            foreach (Dice dice in diceList)
            {
                dice.Despawn();
                Program.DiceBag.AddDice(dice);
            }
        }
        private void UpgradeTheDice()
        {
            List<Dice> dice = Program.GameLocation.Entities.Where(entity => entity is Dice).Select(entity => entity as Dice).ToList();
            GameRules.UpgradeDice(dice);
        }
        private void RecruitMoreDice()
        {
            GameRules.RecruitDice();
        }
    }
}

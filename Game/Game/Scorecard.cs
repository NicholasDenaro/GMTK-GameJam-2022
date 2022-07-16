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
    internal class Scorecard : Entity
    {
        private Sheet[] sheets;
        int level;

        public Scorecard(int x, int y) : base(new Description2D(Sprite.Sprites["Scorecard"], x, y))
        {
            sheets = GenerateRun();
            level = 0;
        }

        public void LoadSheet()
        {
            sheets[level].Display();
        }

        private static Sheet[] GenerateRun()
        {
            return new Sheet[] { new Sheet() };
        }
    }

    class Sheet
    {
        private Quest[] quests;
        private SideQuest[] sideQuests;
           
        public Sheet()
        {
            quests = new Quest[] { new Quest("1♥ 1⸸"), new Quest("2♥ 1⸸"), new Quest("3♥ 3⸸"), new Quest("4♥ 2⸸"), new Quest("5♥ 1⸸"), new Quest("6♥ 2⸸"), };
            sideQuests = new SideQuest[] { new SideQuest("Shop"), new SideQuest("Rest"), new SideQuest("Upgrade"), new SideQuest("Recruit"), };
        }

        public void Display()
        {
            int y = 0;
            int yoffset = 50;
            for (int i = 0; i < quests.Length; i++)
            {
                Program.Engine.AddEntity(0, new Entity(new TextDescription(quests[i].Text, 50, yoffset + y++ * 20)));
            }

            yoffset += y * 20;
            y = 0;
            for (int i = 0; i < sideQuests.Length; i++)
            {
                Program.Engine.AddEntity(0, new Entity(new TextDescription(sideQuests[i].Text, 50, yoffset + y++ * 19)));
            }
        }
    }

    class Quest
    {
        public string Text { get; private set; }
        public bool IsFinished { get; protected set; }

        public Quest(string name)
        {
            Text = name;
        }

        public virtual bool CheckComplete()
        {
            IsFinished = true;
            return true;
        }
    }

    class SideQuest : Quest
    {
        public SideQuest(string name) : base(name)
        {

        }

        public override bool CheckComplete()
        {
            IsFinished = true;
            return true;
        }
    }
}

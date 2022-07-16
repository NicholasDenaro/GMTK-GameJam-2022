using GameEngine;
using GameEngine._2D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    internal class GameRules
    {
        public static int DiceSlots { get; private set; }
        public static int CurrentDiceInPlay { get; private set; }

        public static int MaxRolls { get; private set; }
        public static int RollsLeft { get; private set; }

        public static int Lives { get; private set; }

        public static bool IsBattling { get; private set; }

        //Game
        private static Entity rollsLeftEntity;
        private static Entity rollsLeftSymbol;

        private static Entity diceSlotsEntity;
        private static Entity diceSlotsSymbol;

        private static Entity livesEntity;
        private static TextDescription livesDescription;

        //Battle
        private static Entity battleEnemyEntity;
        private static TextDescription battleEnemyDescription;

        private static Entity battleCursorEntity;


        public static int NumberOfDiceInPlay()
        {
            return Program.Engine.Location(Program.GameState).Entities.Count(entity => entity is Dice);
        }

        public static List<Dice> GetDiceInPlay()
        {
            return Program.GameLocation.Entities.Where(entity => entity is Dice).Select(entity => entity as Dice).ToList();
        }

        public static bool CanDrawAnotherDice()
        {
            return DiceSlots - NumberOfDiceInPlay() > 0;
        }

        public static void Init()
        {
            // Game
            DiceSlots = 3;
            CurrentDiceInPlay = 0;
            MaxRolls = 3;
            RollsLeft = 3;
            Lives = 3;
            diceSlotsEntity = new Entity(new TextDescription($"{NumberOfDiceInPlay()}/{DiceSlots}", 40, 10));
            diceSlotsSymbol = new Entity(new Description2D(Sprite.Sprites["Symbols"], 40 + 40, 10 + 12));
            ((Description2D)diceSlotsSymbol.Description).ImageIndex = 3;
            Program.GameLocation.AddEntity(diceSlotsEntity);
            Program.GameLocation.AddEntity(diceSlotsSymbol);

            rollsLeftEntity = new Entity(new TextDescription($"{RollsLeft}/{MaxRolls}", 100, 10));
            rollsLeftSymbol = new Entity(new Description2D(Sprite.Sprites["Symbols"], 100 + 40, 10 + 12));
            ((Description2D)rollsLeftSymbol.Description).ImageIndex = 4;
            Program.GameLocation.AddEntity(rollsLeftEntity);
            Program.GameLocation.AddEntity(rollsLeftSymbol);

            Program.GameLocation.AddEntity(livesEntity = new Entity(livesDescription = new TextDescription(new string('♥', Lives), 160, 10)));

            Program.Engine.TickEnd(0) += GameRules.Tick;

            // Battle
            Program.BattleLocation.AddEntity(battleEnemyEntity = new Entity(battleEnemyDescription = new TextDescription("----------", 0, 0)));
            Program.BattleLocation.AddEntity(battleCursorEntity = new Entity(new Description2D(Sprite.Sprites["Symbols"], 0, 0)));
            ((Description2D)battleCursorEntity.Description).ImageIndex = 10;

        }

        public static void Reset()
        {
            RollsLeft = MaxRolls;
            CurrentDiceInPlay = 0;
        }

        public static void EndBattle()
        {
            IsBattling = false;
            foreach(Dice dice in battleDice)
            {
                dice.Despawn();
                Program.DiceBag.AddDice(dice);
            }

            foreach (Dice dice in battleDice)
            {
                dice.RemoveFromBattle();
            }

            // TODO: MAJOR BUG, the engine needs another tick to remove entities, but doesn't get a tick until it load again.
            // This could be fine if you don't try to add the same entities back in, but adding is done first. Maybe swap the order of remove and add?
            // Don't add and silently continue because then they'll be removed
            Program.BattleLocation.Tick(new GameState());
            Program.Engine.SetLocation(Program.GameState, Program.GameLocation);

            Reset();
        }

        private static int battleEnemyHealth;
        private static int battleEnemyAttack;
        private static int battleShield;
        private static List<Dice> battleDice;
        private static int turn;
        public static void InitBattle(List<Dice> diceToBattle, int health, int attack)
        {
            IsBattling = true;
            battleEnemyHealth = health;
            battleEnemyAttack = attack;
            turn = 0;
            timer = 0;
            actionTimer = 0;

            battleDice = diceToBattle;
            string info = $"{health}♥ {attack}⸸";
            battleEnemyDescription.ChangeText(info);
            battleEnemyDescription.SetCoords(Program.Width / 2 - info.Length * 14, 30);

            List<Dice> line1 = new List<Dice>();
            List<Dice> line2 = new List<Dice>();
            List<Dice> line3 = new List<Dice>();
            List<Dice> line4 = new List<Dice>();

            foreach (Dice dice in battleDice.Where(dice => dice.Face == Program.Faces.SHIELD))
            {
                line1.Add(dice);
            }

            foreach (Dice dice in battleDice.Where(dice => dice.Face == Program.Faces.SWORD))
            {
                line2.Add(dice);
            }

            foreach (Dice dice in battleDice.Where(dice => dice.Face == Program.Faces.BOW))
            {
                line3.Add(dice);
            }

            foreach (Dice dice in battleDice.Where(dice => dice.Face == Program.Faces.HEAL))
            {
                line4.Add(dice);
            }

            for (int i = 0; i < line1.Count; i++)
            {
                Dice dice = line1[i];
                dice.MoveToBattle(3, i, line1.Count);
            }
            for (int i = 0; i < line2.Count; i++)
            {
                Dice dice = line2[i];
                dice.MoveToBattle(2, i, line2.Count);
            }
            for (int i = 0; i < line3.Count; i++)
            {
                Dice dice = line3[i];
                dice.MoveToBattle(1, i, line3.Count);
            }
            for (int i = 0; i < line4.Count; i++)
            {
                Dice dice = line4[i];
                dice.MoveToBattle(0, i, line4.Count);
            }

            battleDice = new List<Dice>();
            battleDice.AddRange(line1);
            battleDice.AddRange(line2);
            battleDice.AddRange(line3);
            battleDice.AddRange(line4);
        }

        static int timer = 0;
        static int actionTimer = 0;
        public static void Tick(object sender, GameState state)
        {
            if (IsBattling)
            {
                if (++timer % Program.FPS == 1)
                {
                    if (BattleStep())
                    {
                        EndBattle();
                    }
                    actionTimer++;
                }
            }
            if (!IsBattling)
            {
                if (CurrentDiceInPlay != NumberOfDiceInPlay())
                {
                    ((TextDescription)diceSlotsEntity.Description).ChangeText($"{NumberOfDiceInPlay()}/{DiceSlots}");

                    CurrentDiceInPlay = NumberOfDiceInPlay();
                }

                if (RollsLeft > 0 && state.Controllers[1][Program.Keys.ROLL].IsPress())
                {
                    IEnumerable<Dice> diceList = state.Location.Entities.Where(entity => entity is Dice).Select(entity => entity as Dice);
                    if (!diceList.Any(dice => dice.IsRolling) && diceList.Any(dice => !dice.IsLocked))
                    {
                        foreach (Dice dice in diceList)
                        {
                            dice.Roll(withVelocity: true);
                        }

                        RollsLeft--;
                        ((TextDescription)rollsLeftEntity.Description).ChangeText($"{RollsLeft}/{MaxRolls}");
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>true if finished</returns>
        private static bool BattleStep()
        {
            if (actionTimer % 3 == 0)
            {
                if (turn % (battleDice.Count + 1) < battleDice.Count)
                {
                    Dice dice = battleDice[turn % (battleDice.Count + 1)];
                    Description2D diceDescription = dice.Description as Description2D;
                    (battleCursorEntity.Description as Description2D).SetCoords(diceDescription.X - 16, diceDescription.Y + diceDescription.Height / 4);
                    dice.Roll();
                }
                else
                {
                    (battleCursorEntity.Description as Description2D).SetCoords(battleEnemyDescription.X - 16, battleEnemyDescription.Y + battleEnemyDescription.Height / 2);
                }
            }

            if (actionTimer % 3 == 1)
            {
                if (turn % (battleDice.Count + 1) < battleDice.Count)
                {
                    Dice dice = battleDice[turn % (battleDice.Count + 1)];
                    Description2D diceDescription = dice.Description as Description2D;
                    (battleCursorEntity.Description as Description2D).SetCoords(diceDescription.X - 16, diceDescription.Y + diceDescription.Height / 4);
                    Program.Faces face = dice.Face;
                    switch(face)
                    {
                        case Program.Faces.SWORD:
                        case Program.Faces.BOW:
                            battleEnemyHealth--;
                            battleEnemyDescription.ChangeText($"{battleEnemyHealth}♥ {battleEnemyAttack}⸸");
                            break;
                        case Program.Faces.SHIELD:
                            battleShield++;
                            break;
                        case Program.Faces.HEAL:
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    int damage = battleEnemyAttack - battleShield;
                    battleShield -= battleEnemyAttack;
                    battleShield = 0;
                    if (damage > 0 && battleDice[0].Damage(damage))
                    {
                        Dice deadDice = battleDice[0];
                        battleDice.RemoveAt(0);
                        deadDice.RemoveFromBattle();
                        turn--;
                    }
                }
            }

            if (actionTimer % 3 == 2)
            {
                turn++;
                return battleEnemyHealth <= 0;
            }

            return false;
        }
    }
}

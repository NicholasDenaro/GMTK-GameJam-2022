using GameEngine;
using GameEngine._2D;
using GameEngine.UI.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    internal class GameRules
    {
        public static int DiceSlots { get; set; }
        public static int CurrentDiceInPlay { get; private set; }

        public static int MaxRolls { get; set; }
        public static int RollsLeft { get; private set; }

        public static int Lives { get; private set; }

        public static int Coins { get; set; } = 5;

        public static int RecruitmentSlots { get; set; }
        public static int RecruitmentTier { get; set; }

        public static bool IsBattling { get; private set; }
        public static bool IsBattlingFinished { get; private set; }

        //Game
        private static Entity rollsLeftEntity;
        private static Entity rollsLeftSymbol;

        private static Entity diceSlotsEntity;
        private static Entity diceSlotsSymbol;

        private static Entity coinsEntity;
        private static Entity coinsSymbol;

        private static Entity livesEntity;
        private static TextDescription livesDescription;

        //Battle
        private static Entity battleEnemyEntity;
        private static TextDescription battleEnemyDescription;

        private static Entity battleCursorEntity;

        private static Entity battleEndEntity;
        private static Entity battleEndButtonEntity;


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

            coinsEntity = new Entity(new TextDescription($"{Coins:000}", 40, 30));
            coinsSymbol = new Entity(new Description2D(Sprite.Sprites["Symbols"], 40 + 40, 30 + 12));
            ((Description2D)coinsSymbol.Description).ImageIndex = 7;
            Program.GameLocation.AddEntity(coinsEntity);
            Program.GameLocation.AddEntity(coinsSymbol);

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


            battleEndEntity = new Entity(new TextDescription("End Battle", Program.Width / 2 - 64, Program.Height - 24));
            battleEndButtonEntity = new Entity(new Description2D(Sprite.Sprites["Symbols"], Program.Width / 2 + 32, Program.Height - 12));
            (battleEndButtonEntity.Description as Description2D).ImageIndex = 5;
        }

        public static void Reset()
        {
            RollsLeft = MaxRolls;
            CurrentDiceInPlay = 0;

            (rollsLeftEntity.Description as TextDescription).ChangeText($"{RollsLeft}/{MaxRolls}");
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

            if (battleDice.Count == 0)
            {
                Lives--;
                if (Lives == 0 || Program.DiceBag.Count == 0)
                {
                    Program.Engine.SetLocation(Program.GameState, Program.GameOverLocation);
                    return;
                }
                else
                {
                    livesDescription.ChangeText(new string('♥', Lives));
                    livesDescription.ChangeCoordsDelta(20, 0);
                }
            }
            else
            {
                Coins += 5;
                (coinsEntity.Description as TextDescription).ChangeText($"{Coins:000}");
            }

            Program.BattleLocation.RemoveEntity(battleEndEntity.Id);
            Program.BattleLocation.RemoveEntity(battleEndButtonEntity.Id);

            // TODO: MAJOR BUG, the engine needs another tick to remove entities, but doesn't get a tick until it load again.
            // This could be fine if you don't try to add the same entities back in, but adding is done first. Maybe swap the order of remove and add?
            // Don't add and silently continue because then they'll be removed
            Program.BattleLocation.Tick(new GameState());
            Program.Engine.SetLocation(Program.GameState, Program.GameLocation);

            Reset();
        }

        public static void OpenShop()
        {
            Program.Engine.SetLocation(Program.GameState, Program.ShopLocation);

            foreach (Dice dice in GetDiceInPlay())
            {
                dice.Despawn();
                Program.DiceBag.AddDice(dice);
            }
        }

        public static void CloseShop()
        {
            Reset();
            Program.Engine.SetLocation(Program.GameState, Program.GameLocation);
        }

        public static void HealDice(List<Dice> diceList)
        {
            foreach (Dice dice in diceList)
            {
                dice.Heal(10);
            }

            Reset();
        }
        public static void UpgradeDice(List<Dice> diceList)
        {
            foreach (Dice dice in GetDiceInPlay())
            {
                dice.Despawn();
                Program.DiceBag.AddDice(dice);
            }
            Program.Engine.SetLocation(Program.GameState, Program.UpgradeLocation);

            Description2D d2d;
            Entity ent;

            int numDice = diceList.Count;

            for (int i = 0; i < numDice; i++)
            {
                Dice dice = diceList[i];

                int upgradeIndex = Program.SidesToUpgradeIndex[dice.Sides];

                int cost = (upgradeIndex % 2 == 0 ? 2 : 3) * (1 + upgradeIndex / 2);

                int x = Program.Width / numDice / 2 + i * Program.Width / numDice - Program.Width / numDice / 2;
                int y = 80;

                Program.UpgradeLocation.AddEntity(new Entity(d2d = new Description2D(Sprite.Sprites["dice"], x + 16, y - 24)));
                d2d.ImageIndex = (dice.Description as Description2D).ImageIndex;

                dice.ForceShowInfo(Program.UpgradeLocation);

                Program.UpgradeLocation.AddEntity(ent = new Entity(new TextDescription($"{cost:00}", x + 16 - 14, y + 96 - 10)));
                Program.UpgradeLocation.AddEntity(ent = new Entity(d2d = new Description2D(Sprite.Sprites["Symbols"], x + 16 + 14, y + 96 + 2)));
                d2d.ImageIndex = 7;

                Program.UpgradeLocation.AddEntity(ent = new Entity(d2d = new Description2D(Sprite.Sprites["Symbols"], x + 16, y + 112)));
                d2d.ImageIndex = 5;
                ent.TickAction += (state, entity) =>
                {
                    Description2D description = entity.Description as Description2D;
                    if (GameRules.Coins >= cost && description.ImageIndex == 5 && state.Controllers[0][Program.Keys.CLICK].IsPress())
                    {
                        MouseControllerInfo info = state.Controllers[0][Program.Keys.CLICK].Info as MouseControllerInfo;
                        if (description.IsCollision(new Description2D(info.X + description.Sprite.X, info.Y + description.Sprite.Y, 1, 1)))
                        {
                            description.ImageIndex = 6;
                            dice.Upgrade();
                            GameRules.Coins -= cost;
                        }
                    }
                };
            }
        }

        public static void RecruitDice()
        {
            foreach (Dice dice in GetDiceInPlay())
            {
                dice.Despawn();
                Program.DiceBag.AddDice(dice);
            }

            Program.Engine.SetLocation(Program.GameState, Program.RecruitLocation);

            var presets = Program.DicePresetsT1;
            if (RecruitmentTier == 1)
            {
                presets = Program.DicePresetsT2;
            }
            if (RecruitmentTier == 2)
            {
                presets = Program.DicePresetsT3;
            }

            var keys = presets.Keys.ToList();
            Description2D d2d;
            Entity ent;
            for (int i = 0; i < 5; i++)
            {
                if ((i == 0 && RecruitmentSlots == 0) || (i == 4 && RecruitmentSlots < 2))
                {
                    continue;
                }

                int key = Program.Random.Next(keys.Count);

                int cost = (key < 3 ? 3 : 5) * (RecruitmentTier + 1);

                int x = Program.Width / 10 + i * Program.Width / 5 - Program.Width / 10;
                int y = 80;
                Dice dice = Dice.Create(presets[keys[key]], x, y);

                Program.RecruitLocation.AddEntity(new Entity(d2d = new Description2D(Sprite.Sprites["dice"], x + 16, y - 24)));
                d2d.ImageIndex = (dice.Description as Description2D).ImageIndex;

                dice.ForceShowInfo(Program.RecruitLocation);

                Program.RecruitLocation.AddEntity(ent = new Entity(new TextDescription($"{cost:00}", x + 16 - 14, y + 96 - 10)));
                Program.RecruitLocation.AddEntity(ent = new Entity(d2d = new Description2D(Sprite.Sprites["Symbols"], x + 16 + 14, y + 96 + 2)));
                d2d.ImageIndex = 7;

                Program.RecruitLocation.AddEntity(ent = new Entity(d2d = new Description2D(Sprite.Sprites["Symbols"], x + 16, y + 112)));
                d2d.ImageIndex = 5;
                ent.TickAction += (state, entity) =>
                {
                    Description2D description = entity.Description as Description2D;
                    if (GameRules.Coins >= cost && description.ImageIndex == 5 && state.Controllers[0][Program.Keys.CLICK].IsPress())
                    {
                        MouseControllerInfo info = state.Controllers[0][Program.Keys.CLICK].Info as MouseControllerInfo;
                        if (description.IsCollision(new Description2D(info.X + description.Sprite.X, info.Y + description.Sprite.Y, 1, 1)))
                        {
                            description.ImageIndex = 6;
                            Program.DiceBag.AddDice(dice);
                            GameRules.Coins -= cost;
                        }
                    }
                };
            }
        }

        private static int battleEnemyHealth;
        private static int battleEnemyAttack;
        private static int battleShield;
        private static List<Dice> battleDice;
        private static int turn;
        public static void InitBattle(List<Dice> diceToBattle, int health, int attack)
        {
            IsBattling = true;
            IsBattlingFinished = false;
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
            if (IsBattling && !IsBattlingFinished)
            {
                if (++timer % (Program.FPS / 2) == 1)
                {
                    if (BattleStep())
                    {
                        IsBattlingFinished = true;
                        Program.BattleLocation.AddEntity(battleEndEntity);
                        Program.BattleLocation.AddEntity(battleEndButtonEntity);
                    }
                    actionTimer++;
                }
            }
            if (IsBattlingFinished)
            {
                if (state.Controllers[0][Program.Keys.CLICK].IsPress())
                {
                    MouseControllerInfo info = state.Controllers[0][Program.Keys.CLICK].Info as MouseControllerInfo;
                    if (info != null && (battleEndButtonEntity.Description as Description2D).IsCollision(new Description2D(info.X + (battleEndButtonEntity.Description as Description2D).Sprite.X, info.Y + (battleEndButtonEntity.Description as Description2D).Sprite.Y, 1, 1)))
                    {
                        EndBattle();
                    }
                }
            }
            if (!IsBattling)
            {
                if (CurrentDiceInPlay != NumberOfDiceInPlay())
                {
                    ((TextDescription)diceSlotsEntity.Description).ChangeText($"{NumberOfDiceInPlay()}/{DiceSlots}");

                    CurrentDiceInPlay = NumberOfDiceInPlay();
                }

                if (state.Controllers[1][Program.Keys.ROLL].IsPress())
                {
                    Roll();
                }
            }
        }

        public static void Roll()
        {
            if (RollsLeft > 0)
            {
                IEnumerable<Dice> diceList = Program.GameLocation.Entities.Where(entity => entity is Dice).Select(entity => entity as Dice);
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
                            Dice target = battleDice.FirstOrDefault(d => !d.IsFullHealth);
                            if (target != null)
                            {
                                target.Heal(1);
                            }
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
                        turn = -1;
                    }
                }
            }

            if (actionTimer % 3 == 2)
            {
                turn++;
                return battleEnemyHealth <= 0 || battleDice.Count == 0;
            }

            return false;
        }
    }
}

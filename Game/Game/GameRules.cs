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

        private static Entity rollsLeftEntity;
        private static Entity rollsLeftSymbol;

        private static Entity diceSlotsEntity;
        private static Entity diceSlotsSymbol;

        private static Entity livesEntity;

        public static int DiceInPlay()
        {
            return Program.Engine.Location(Program.GameState).Entities.Count(entity => entity is Dice);
        }

        public static bool CanDrawAnotherDice()
        {
            return DiceSlots - DiceInPlay() > 0;
        }

        public static void Init()
        {
            DiceSlots = 3;
            CurrentDiceInPlay = 0;
            MaxRolls = 3;
            RollsLeft = 3;
            Lives = 3;
            diceSlotsEntity = new Entity(new TextDescription($"{DiceInPlay()}/{DiceSlots}", 40, 10));
            diceSlotsSymbol = new Entity(new Description2D(Sprite.Sprites["Symbols"], 40 + 40, 10 + 12));
            ((Description2D)diceSlotsSymbol.Description).ImageIndex = 3;
            Program.GameLocation.AddEntity(diceSlotsEntity);
            Program.GameLocation.AddEntity(diceSlotsSymbol);

            rollsLeftEntity = new Entity(new TextDescription($"{RollsLeft}/{MaxRolls}", 100, 10));
            rollsLeftSymbol = new Entity(new Description2D(Sprite.Sprites["Symbols"], 100 + 40, 10 + 12));
            ((Description2D)rollsLeftSymbol.Description).ImageIndex = 4;
            Program.GameLocation.AddEntity(rollsLeftEntity);
            Program.GameLocation.AddEntity(rollsLeftSymbol);

            Program.GameLocation.AddEntity(livesEntity = new Entity(new TextDescription(new string('♥', Lives), 160, 10)));

            Program.Engine.TickEnd(0) += GameRules.Tick;
        }

        public static void Reset()
        {
            RollsLeft = MaxRolls;
            CurrentDiceInPlay = 0;
        }

        public static void Tick(object sender, GameState state)
        {
            if (CurrentDiceInPlay != DiceInPlay())
            {
                ((TextDescription)diceSlotsEntity.Description).ChangeText($"{DiceInPlay()}/{DiceSlots}");

                CurrentDiceInPlay = DiceInPlay();
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
}

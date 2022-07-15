using GameEngine;
using GameEngine._2D;
using GameEngine.UI;
using GameEngine.UI.AvaloniaUI;
using GameEngine.UI.Controllers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Game
{
    internal class Program
    {
        public const int FPS = 60;

        public static readonly Random Random = new Random();

        static async Task Main()
        {
            (GameEngine.GameEngine engine, GameUI ui) = new GameBuilder()
                .GameEngine(new FixedTickEngine(FPS))
                .GameView(new GameView2D(new Drawer2DAvalonia(), 480, 320, 2, 2, Color.Gray))
                .GameFrame(new GameUI(
                    new AvaloniaWindowBuilder()
                        .StartupLocation(Avalonia.Controls.WindowStartupLocation.CenterScreen)
                        .Title("Game")
                    , 0, 0, 480, 320, 2, 2
                    ))
                .Controller(new WindowsMouseController(mouseMap))
                .Controller(new WindowsKeyController(keyMap))
                .StartingLocation(new Location(new Description2D(0, 0, 480, 320)))
                .Build();

            
            Sprite diceSprite = new Sprite("dice", "Sprites.dice.png", 32, 32, 16, 16);
            Sprite diceFacesSprite = new Sprite("diceFaces", "Sprites.diceFaces.png", 10, 10, 5, 5);
            Sprite deskSprite = new Sprite("desk", "Sprites.desk.png", 480, 320, 0, 0);
            engine.AddEntity(0, new Entity(new Description2D(deskSprite, 0, 0)));

            Dice[] dice = new Dice[10];
            for (int i = 0; i < dice.Length; i++)
            {
                var keys = new List<string>(DicePresets.Keys);
                dice[i] = Dice.Create(engine, 0, DicePresets[keys[Random.Next(keys.Count)]], 60 + i * 40, 60);
            }

            engine.TickEnd(0) += (object s, GameState state) =>
            {
                if (state.Controllers[1][Keys.ROLL].IsPress())
                {
                    for (int i = 0; i < dice.Length; i++)
                    {
                        dice[i].Roll();
                    }
                }
            };

            await engine.Start();
        }

        public static Dictionary<int, object> mouseMap = new Dictionary<int, object>()
        {
            { (int)Avalonia.Input.PointerUpdateKind.LeftButtonPressed, Keys.CLICK },
            { (int)Avalonia.Input.PointerUpdateKind.MiddleButtonPressed, Keys.RCLICK },
        };

        public static Dictionary<int, object> keyMap = new Dictionary<int, object>()
        {
            { (int)Avalonia.Input.Key.R, Keys.ROLL },
        };

        public enum Sides { FOUR = 4, SIX = 6, EIGHT = 8, TEN = 10, TWELVE = 12, TWENTY = 20 }

        public enum Colors { RED = 0, BLUE = 1, GREEN = 2, YELLOW = 3 }

        public enum Faces { NONE = 0, SWORD = 1, SHIELD = 2, HEAL = 3, BOW = 4}

        public enum Keys { CLICK, RCLICK, ROLL }

        public static Dictionary<string, (Sides sides, Colors color, Faces[] faces)> DicePresets = new Dictionary<string, (Sides sides, Colors color, Faces[] faces)>()
        {
            { "Red Warrior", (Sides.FOUR, Colors.RED, new []{ Faces.SWORD, Faces.SHIELD, Faces.SWORD, Faces.SHIELD }) },
            { "Red Archer", (Sides.FOUR, Colors.RED, new []{ Faces.BOW, Faces.SHIELD, Faces.BOW, Faces.SHIELD }) },
            { "Red Healer", (Sides.FOUR, Colors.RED, new []{ Faces.HEAL, Faces.SHIELD, Faces.SWORD, Faces.SHIELD }) },
            { "Blue Warrior", (Sides.FOUR, Colors.BLUE, new []{ Faces.SWORD, Faces.HEAL, Faces.SWORD, Faces.SHIELD }) },
            { "Blue Knight", (Sides.EIGHT, Colors.BLUE, new []{ Faces.SWORD, Faces.SHIELD, Faces.HEAL, Faces.SHIELD, Faces.SWORD, Faces.SHIELD, Faces.SWORD, Faces.SHIELD }) },
        };
    }
}
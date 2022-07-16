using GameEngine;
using GameEngine._2D;
using GameEngine.UI;
using GameEngine.UI.AvaloniaUI;
using GameEngine.UI.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Game
{
    internal class Program
    {
        public const int FPS = 60;

        public static readonly Random Random = new Random();

        public static GameEngine.GameEngine Engine;
        public const int GameState = 0;
        public const int Width = 480;
        public const int Height = 320;

        public static Location GameLocation { get; private set; }
        public static Location MenuLocation { get; private set; }
        public static Location ShopLocation { get; private set; }
        public  static Location BattleLocation { get; private set; }
        public static Location UpgradeLocation { get; private set; }
        public static Location RecruitLocation { get; private set; }

        public static DiceBag DiceBag { get; private set; }

        static async Task Main()
        {
            (Engine, GameUI ui) = new GameBuilder()
                .GameEngine(new FixedTickEngine(FPS))
                .GameView(new GameView2D(new Drawer2DAvalonia(), Width, Height, 2, 2, Color.Gray))
                .GameFrame(new GameUI(
                    new AvaloniaWindowBuilder()
                        .StartupLocation(Avalonia.Controls.WindowStartupLocation.CenterScreen)
                        .Title("Game")
                    , 0, 0, Width, Height, 2, 2
                    ))
                .Controller(new WindowsMouseController(mouseMap))
                .Controller(new WindowsKeyController(keyMap))
                .StartingLocation(MenuLocation = new Location(new Description2D(0, 0, Width, Height)))
                .Build();
;
            GameLocation = new Location(new Description2D(0, 0, Width, Height));
            BattleLocation = new Location(new Description2D(0, 0, Width, Height));

            new Sprite("dice", "Sprites.dice.png", 32, 32, 16, 16);
            new Sprite("diceFaces", "Sprites.diceFaces.png", 10, 10, 5, 5);
            new Sprite("DiceBag", "Sprites.dicebag.png", 105, 80, 0, 0);
            new Sprite("Scorecard", "Sprites.scorecard.png", 193, 300);
            new Sprite("Symbols", "Sprites.symbols.png", 16, 16, 8, 8);

            MainMenuSetup();

            Engine.SetLocation(GameState, MenuLocation);

            await Engine.Start();
        }

        private static void MainMenuSetup()
        {
            MenuLocation.AddEntity(new Entity(new TextDescription("Welcome to game", Program.Width / 2 - ("Welcome to game".Length - 1) * 12 / 2, Program.Height / 2 - 20)));
            Engine.TickEnd(0) += (object sender, GameState state) =>
            {
                if (state.Location == MenuLocation && state.Controllers[0][Keys.CLICK].IsPress())
                {
                    Engine.SetLocation(Program.GameState, GameLocation);
                    GameSetup();
                }
            };
        }

        private static void GameSetup()
        {
            GameLocation.AddEntity(new Entity(new Description2D(new Sprite("desk", "Sprites.desk.png", Width, Height, 0, 0), 0, 0)));

            Scorecard scorecard;
            GameLocation.AddEntity(scorecard = new Scorecard(10, 10));

            scorecard.LoadSheet(GameLocation);

            GameRules.Init();

            DiceBag = new DiceBag(Program.Width - 120, Program.Height - 100);
            //for (int i = 0; i < 5; i++)
            ////for (int i = 0; i < 20; i++)
            //{
            //    var keys = new List<string>(DicePresets.Keys);
            //    diceBag.AddDice(Dice.Create(DicePresets[keys[Random.Next(3)]], 60 + i * 40, 60));
            //    //diceBag.AddDice(Dice.Create(DicePresets[keys[Random.Next(DicePresets.Count)]], 60 + i * 40, 60));
            //}

            DiceBag.AddDice(Dice.Create(DicePresets["Red Warrior"], 0, 0));
            DiceBag.AddDice(Dice.Create(DicePresets["Red Warrior"], 0, 0));
            DiceBag.AddDice(Dice.Create(DicePresets["Red Warrior"], 0, 0));
            DiceBag.AddDice(Dice.Create(DicePresets["Red Archer"], 0, 0));
            DiceBag.AddDice(Dice.Create(DicePresets["Red Healer"], 0, 0));

            GameLocation.AddEntity(DiceBag);
        }

        public static Dictionary<int, object> mouseMap = new Dictionary<int, object>()
        {
            { AvaloniaWindow.Key(Avalonia.Input.PointerUpdateKind.LeftButtonPressed), Keys.CLICK },
            { AvaloniaWindow.Key(Avalonia.Input.PointerUpdateKind.Other), Keys.MOUSEINFO },
            { AvaloniaWindow.Key(Avalonia.Input.PointerUpdateKind.RightButtonPressed), Keys.RCLICK },
        };

        public static Dictionary<int, object> keyMap = new Dictionary<int, object>()
        {
            { (int)Avalonia.Input.Key.R, Keys.ROLL },
        };

        public enum Sides { FOUR = 4, SIX = 6, EIGHT = 8, TEN = 10, TWELVE = 12, TWENTY = 20 }

        public enum Colors { RED = 0, BLUE = 1, GREEN = 2, YELLOW = 3 }

        public enum Faces { NONE = 0, SWORD = 1, SHIELD = 2, HEAL = 3, BOW = 4}

        public enum Keys { CLICK, RCLICK, ROLL, MOUSEINFO }

        public static Dictionary<string, (Sides sides, Colors color, Faces[] faces, int health)> DicePresets = new Dictionary<string, (Sides sides, Colors color, Faces[] faces, int health)>()
        {
            { "Red Warrior",
                (Sides.FOUR,
                Colors.RED,
                new []{ 
                    Faces.SWORD,
                    Faces.SHIELD,
                    Faces.SWORD,
                    Faces.SHIELD
                }, 
                health: 3) },
            { "Red Archer",
                (Sides.FOUR,
                Colors.RED,
                new []{
                    Faces.BOW,
                    Faces.SHIELD,
                    Faces.BOW,
                    Faces.SHIELD
                },
                health: 2) },
            { "Red Healer",
                (Sides.FOUR,
                Colors.RED,
                new []{
                    Faces.HEAL,
                    Faces.SWORD,
                    Faces.HEAL,
                    Faces.SHIELD
                },
                health: 1) },
            { "Blue Warrior",
                (Sides.FOUR,
                Colors.BLUE,
                new []{
                    Faces.SWORD,
                    Faces.HEAL,
                    Faces.SWORD,
                    Faces.SHIELD
                },
                health: 4) },
            { "Green Squire",
                (Sides.SIX,
                Colors.GREEN,
                new []{
                    Faces.SWORD,
                    Faces.HEAL,
                    Faces.SWORD,
                    Faces.SHIELD,
                    Faces.SHIELD,
                    Faces.HEAL
                },
                health: 6) },
            { "Green Knight",
                (Sides.EIGHT,
                Colors.GREEN,
                new []{
                    Faces.SWORD,
                    Faces.SHIELD,
                    Faces.HEAL,
                    Faces.SHIELD,
                    Faces.SWORD,
                    Faces.SHIELD,
                    Faces.SWORD,
                    Faces.SHIELD
                },
                health: 8) },
            { "Yellow Prince",
                (Sides.TEN,
                Colors.YELLOW,
                new []{
                    Faces.SWORD,
                    Faces.SHIELD,
                    Faces.SWORD,
                    Faces.HEAL,
                    Faces.SHIELD,
                    Faces.SWORD,
                    Faces.SHIELD,
                    Faces.SWORD,
                    Faces.SHIELD,
                    Faces.SWORD
                },
                health: 3) },
            { "Blue Paladin",
                (Sides.TWELVE,
                Colors.BLUE,
                new []{
                    Faces.SWORD,
                    Faces.SHIELD,
                    Faces.HEAL,
                    Faces.SHIELD,
                    Faces.SWORD,
                    Faces.SHIELD,
                    Faces.SWORD,
                    Faces.SHIELD,
                    Faces.SHIELD,
                    Faces.SHIELD,
                    Faces.SHIELD,
                    Faces.SHIELD
                },
                health: 10) },
            { "Yellow General",
                (Sides.TWENTY,
                Colors.YELLOW,
                new []{
                    Faces.SWORD,
                    Faces.SHIELD,
                    Faces.SHIELD,
                    Faces.SHIELD,
                    Faces.SHIELD,
                    Faces.HEAL,
                    Faces.SHIELD,
                    Faces.SHIELD,
                    Faces.SWORD,
                    Faces.SHIELD,
                    Faces.SHIELD,
                    Faces.HEAL,
                    Faces.SHIELD,
                    Faces.SHIELD,
                    Faces.SWORD,
                    Faces.SHIELD,
                    Faces.SHIELD,
                    Faces.HEAL,
                    Faces.SHIELD,
                    Faces.SHIELD
                },
                health: 15) },
        };
    }
}
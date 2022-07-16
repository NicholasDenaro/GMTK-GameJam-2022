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

        public static GameEngine.GameEngine Engine;
        public const int GameState = 0;
        public const int MenuState = 1;

        static async Task Main()
        {
            (Engine, GameUI ui) = new GameBuilder()
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
            new Sprite("Scorecard", "Sprites.scorecard.png", 193, 300);

            Engine.AddEntity(0, new Entity(new Description2D(deskSprite, 0, 0)));

            Scorecard scorecard;
            Engine.AddEntity(0, scorecard = new Scorecard(10, 10));

            scorecard.LoadSheet();

            Dice[] dice = new Dice[10];
            for (int i = 0; i < dice.Length; i++)
            {
                var keys = new List<string>(DicePresets.Keys);
                dice[i] = Dice.Create(DicePresets[keys[Random.Next(keys.Count)]], 60 + i * 40, 60);
            }

            Engine.TickEnd(0) += (object s, GameState state) =>
            {
                if (state.Controllers[1][Keys.ROLL].IsPress())
                {
                    for (int i = 0; i < dice.Length; i++)
                    {
                        dice[i].Roll();
                    }
                }
            };

            await Engine.Start();
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
                }, 3) },
            { "Red Archer",
                (Sides.FOUR,
                Colors.RED,
                new []{
                    Faces.BOW,
                    Faces.SHIELD,
                    Faces.BOW,
                    Faces.SHIELD
                }, 2) },
            { "Red Healer",
                (Sides.FOUR,
                Colors.RED,
                new []{
                    Faces.HEAL,
                    Faces.SHIELD,
                    Faces.SWORD,
                    Faces.SHIELD
                }, 1) },
            { "Blue Warrior",
                (Sides.FOUR,
                Colors.BLUE,
                new []{
                    Faces.SWORD,
                    Faces.HEAL,
                    Faces.SWORD,
                    Faces.SHIELD
                }, 4) },
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
                }, 6) },
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
                }, 8) },
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
                }, 3) },
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
                }, 10) },
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
                }, 15) },
        };
    }
}
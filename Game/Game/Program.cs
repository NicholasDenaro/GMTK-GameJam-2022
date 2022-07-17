﻿using GameEngine;
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
        public static Location GameOverLocation { get; private set; }
        public static Location ShopLocation { get; private set; }
        public  static Location BattleLocation { get; private set; }
        public static Location UpgradeLocation { get; private set; }
        public static Location RecruitLocation { get; private set; }

        public static Description2D PlayArea { get; private set; }

        public static DiceBag DiceBag { get; private set; }

        static async Task Main()
        {
            (Engine, GameUI ui) = new GameBuilder()
                .GameEngine(new FixedTickEngine(FPS))
                .GameView(new GameView2D(new Drawer2DAvalonia(), Width, Height, 2, 2, Color.Gray))
                .GameFrame(new GameUI(
                    new AvaloniaWindowBuilder()
                        .StartupLocation(Avalonia.Controls.WindowStartupLocation.CenterScreen)
                        .Title("Dicey Guildkeeper")
                    , 0, 0, Width, Height, 2, 2
                    ))
                .Controller(new WindowsMouseController(mouseMap))
                .Controller(new WindowsKeyController(keyMap))
                .StartingLocation(MenuLocation = new Location(new Description2D(0, 0, Width, Height)))
                .Build();
;
            GameLocation = new Location(new Description2D(0, 0, Width, Height));
            GameOverLocation = new Location(new Description2D(0, 0, Width, Height));
            BattleLocation = new Location(new Description2D(0, 0, Width, Height));
            ShopLocation = new Location(new Description2D(0, 0, Width, Height));
            UpgradeLocation = new Location(new Description2D(0, 0, Width, Height));
            RecruitLocation = new Location(new Description2D(0, 0, Width, Height));

            new Sprite("dice", "Sprites.dice.png", 32, 32, 16, 16);
            new Sprite("diceFaces", "Sprites.diceFaces.png", 10, 10, 5, 5);
            new Sprite("DiceBag", "Sprites.dicebag.png", 105, 80, 0, 0);
            new Sprite("Scorecard", "Sprites.scorecard.png", 193, 300);
            new Sprite("Symbols", "Sprites.symbols.png", 16, 16, 8, 8);
            new Sprite("Button", "Sprites.button.png", 80, 32, 0, 0);
            new Sprite("desk", "Sprites.desk.png", Width, Height, 0, 0);

            MainMenuSetup();

            Engine.SetLocation(GameState, MenuLocation);

            await Engine.Start();
        }

        private static void MainMenuSetup()
        {
            MenuLocation.AddEntity(new Entity(new TextDescription("      Welcome to\n Dicey Guildkeeper", Program.Width / 2 - ("Dicey Guildkeeper".Length - 1) * 12 / 2, Program.Height / 2 - 20)));
            Engine.TickEnd(0) += (object sender, GameState state) =>
            {
                if (state.Location == MenuLocation && state.Controllers[0][Keys.CLICK].IsPress())
                {
                    Engine.SetLocation(Program.GameState, GameLocation);
                    GameSetup();
                }
            };

            //Game Over
            GameOverLocation.AddEntity(new Entity(new TextDescription("Game Over", Program.Width / 2 - ("Game Over".Length - 1) * 12 / 2, Program.Height / 2 - 20)));

            // Shop
            Entity ent;
            Description2D d2d;
            ShopLocation.AddEntity(new Entity(new Description2D(Sprite.Sprites["desk"], 0, 0)));

            ShopLocation.AddEntity(new Entity(new Description2D(Sprite.Sprites["Scorecard"], 16, 16, 320, 144)));

            int xOffset = 48;
            int yOffset = 36;
            int yMult = 28;

            ShopLocation.AddEntity(new Entity(new TextDescription("+1 dice slot ................... 05", xOffset + 24, yOffset + yMult * 0)));
            ShopLocation.AddEntity(new Entity(d2d = new Description2D(Sprite.Sprites["Symbols"], xOffset + 256, yOffset + 12 + yMult * 0)));
            d2d.ImageIndex = 7;
            ShopLocation.AddEntity(ent = new Entity(d2d = new Description2D(Sprite.Sprites["Symbols"], xOffset + 272, yOffset + 12 + yMult * 0)));
            d2d.ImageIndex = 5;
            ent.TickAction += (state, entity) =>
            {
                Description2D description = entity.Description as Description2D;
                if (GameRules.Coins >= 5 && description.ImageIndex == 5 && state.Controllers[0][Program.Keys.CLICK].IsPress())
                {
                    MouseControllerInfo info = state.Controllers[0][Program.Keys.CLICK].Info as MouseControllerInfo;
                    if (description.IsCollision(new Description2D(info.X + description.Sprite.X, info.Y + description.Sprite.Y, 1, 1)))
                    {
                        description.ImageIndex = 6;
                        GameRules.DiceSlots += 1;
                        GameRules.Coins -= 5;
                    }
                }
            };

            ShopLocation.AddEntity(new Entity(new TextDescription("+1 max rolls .................. 10", xOffset + 24, yOffset + yMult * 1)));
            ShopLocation.AddEntity(new Entity(d2d = new Description2D(Sprite.Sprites["Symbols"], xOffset + 256, yOffset + 12 + yMult * 1)));
            d2d.ImageIndex = 7;
            ShopLocation.AddEntity(ent = new Entity(d2d = new Description2D(Sprite.Sprites["Symbols"], xOffset + 272, yOffset + 12 + yMult * 1)));
            d2d.ImageIndex = 5;
            ent.TickAction += (state, entity) =>
            {
                Description2D description = entity.Description as Description2D;
                if (GameRules.Coins >= 10 && description.ImageIndex == 5 && state.Controllers[0][Program.Keys.CLICK].IsPress())
                {
                    MouseControllerInfo info = state.Controllers[0][Program.Keys.CLICK].Info as MouseControllerInfo;
                    if (description.IsCollision(new Description2D(info.X + description.Sprite.X, info.Y + description.Sprite.Y, 1, 1)))
                    {
                        description.ImageIndex = 6;
                        GameRules.MaxRolls += 1;
                        GameRules.Coins -= 10;
                    }
                }
            };

            ShopLocation.AddEntity(new Entity(new TextDescription("+1 recruitment slot ...... 10", xOffset + 24, yOffset + yMult * 2)));
            ShopLocation.AddEntity(new Entity(d2d = new Description2D(Sprite.Sprites["Symbols"], xOffset + 256, yOffset + 12 + yMult * 2)));
            d2d.ImageIndex = 7;
            ShopLocation.AddEntity(ent = new Entity(d2d = new Description2D(Sprite.Sprites["Symbols"], xOffset + 272, yOffset + 12 + yMult * 2)));
            d2d.ImageIndex = 5;
            ent.TickAction += (state, entity) =>
            {
                Description2D description = entity.Description as Description2D;
                if (GameRules.Coins >= 10 && state.Controllers[0][Program.Keys.CLICK].IsPress())
                {
                    MouseControllerInfo info = state.Controllers[0][Program.Keys.CLICK].Info as MouseControllerInfo;
                    if (description.IsCollision(new Description2D(info.X + description.Sprite.X, info.Y + description.Sprite.Y, 1, 1)))
                    {
                    }
                }
            };

            ShopLocation.AddEntity(new Entity(new TextDescription("+1 recruitment tier ...... 15", xOffset + 24, yOffset + yMult * 3)));
            ShopLocation.AddEntity(new Entity(d2d = new Description2D(Sprite.Sprites["Symbols"], xOffset + 256, yOffset + 12 + yMult * 3)));
            d2d.ImageIndex = 7;
            ShopLocation.AddEntity(ent = new Entity(d2d = new Description2D(Sprite.Sprites["Symbols"], xOffset + 272, yOffset + 12 + yMult * 3)));
            d2d.ImageIndex = 5;
            ent.TickAction += (state, entity) =>
            {
                Description2D description = entity.Description as Description2D;
                if (GameRules.Coins >= 15 && description.ImageIndex == 5 && state.Controllers[0][Program.Keys.CLICK].IsPress())
                {
                    MouseControllerInfo info = state.Controllers[0][Program.Keys.CLICK].Info as MouseControllerInfo;
                    if (description.IsCollision(new Description2D(info.X + description.Sprite.X, info.Y + description.Sprite.Y, 1, 1)))
                    {
                        description.ImageIndex = 6;
                    }
                }
            };

            ShopLocation.AddEntity(new Button(Program.ShopLocation, "Back", Program.Width - 80 - 16, Program.Height - 32 - 16, GameRules.CloseShop));
        }

        private static void GameSetup()
        {
            GameLocation.AddEntity(new Entity(new Description2D(Sprite.Sprites["desk"], 0, 0)));

            int x = Program.Width / 2 - 48 + 16;
            int xEnd = Program.Width - 16;
            int y = 16;
            int yEnd = Program.Height - 96 - 16;
            GameLocation.AddEntity(new Entity(PlayArea = new Description2D(new Sprite("desk2", "Sprites.desk2.png", 240, 160, 0, 0), x, y, xEnd - x, yEnd - y)));

            Scorecard scorecard;
            GameLocation.AddEntity(scorecard = new Scorecard(10, 10));

            scorecard.LoadSheet(GameLocation);

            GameLocation.AddEntity(new Button(Program.GameLocation, "Roll", Program.Width / 2 + 32, Program.Height - 96, GameRules.Roll));

            GameRules.Init();

            DiceBag = new DiceBag(Program.Width - 120, Program.Height - 100);
            //for (int i = 0; i < 5; i++)
            ////for (int i = 0; i < 20; i++)
            //{
            //    var keys = new List<string>(DicePresets.Keys);
            //    diceBag.AddDice(Dice.Create(DicePresets[keys[Random.Next(3)]], 60 + i * 40, 60));
            //    //diceBag.AddDice(Dice.Create(DicePresets[keys[Random.Next(DicePresets.Count)]], 60 + i * 40, 60));
            //}

            DiceBag.AddDice(Dice.Create(DicePresetsT1["Warrior"], 0, 0));
            DiceBag.AddDice(Dice.Create(DicePresetsT1["Warrior"], 0, 0));
            DiceBag.AddDice(Dice.Create(DicePresetsT1["Archer"], 0, 0));
            DiceBag.AddDice(Dice.Create(DicePresetsT1["Archer"], 0, 0));
            DiceBag.AddDice(Dice.Create(DicePresetsT1["Healer"], 0, 0));

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

        public static Dictionary<string, (Sides sides, Colors color, Faces[] faces, int health, int numRolls)> DicePresetsT1 = new Dictionary<string, (Sides sides, Colors color, Faces[] faces, int health, int numRolls)>()
        {
            { "Warrior",
                (Sides.FOUR,
                Colors.RED,
                new []{
                    Faces.SWORD,
                    Faces.SHIELD,
                    Faces.SWORD,
                    Faces.SHIELD
                },
                health: 3,
                numRolls: 1) },
            { "Archer",
                (Sides.FOUR,
                Colors.GREEN,
                new []{
                    Faces.BOW,
                    Faces.BOW,
                    Faces.BOW,
                    Faces.SHIELD
                },
                health: 2,
                numRolls: 1) },
            { "Healer",
                (Sides.FOUR,
                Colors.BLUE,
                new []{
                    Faces.HEAL,
                    Faces.SWORD,
                    Faces.HEAL,
                    Faces.SHIELD
                },
                health: 1,
                numRolls: 1) },

            { "Squire",
                (Sides.SIX,
                Colors.RED,
                new []{
                    Faces.SWORD,
                    Faces.SHIELD,
                    Faces.SWORD,
                    Faces.HEAL,
                    Faces.SWORD,
                    Faces.SHIELD
                },
                health: 5,
                numRolls: 1) },
            { "Sharpshooter",
                (Sides.SIX,
                Colors.GREEN,
                new []{
                    Faces.BOW,
                    Faces.HEAL,
                    Faces.SHIELD,
                    Faces.BOW,
                    Faces.BOW,
                    Faces.SHIELD
                },
                health: 4,
                numRolls: 1) },
            { "Cleric",
                (Sides.SIX,
                Colors.BLUE,
                new []{
                    Faces.HEAL,
                    Faces.SHIELD,
                    Faces.SWORD,
                    Faces.HEAL,
                    Faces.SHIELD,
                    Faces.HEAL
                },
                health: 6,
                numRolls: 1) },
        };

        public static Dictionary<string, (Sides sides, Colors color, Faces[] faces, int health, int numRolls)> DicePresetsT2 = new Dictionary<string, (Sides sides, Colors color, Faces[] faces, int health, int numRolls)>()
        {
            { "Knight",
                (Sides.EIGHT,
                Colors.RED,
                new []{
                    Faces.SWORD,
                    Faces.SHIELD,
                    Faces.SWORD,
                    Faces.SHIELD,
                    Faces.HEAL,
                    Faces.SWORD,
                    Faces.SWORD,
                    Faces.SHIELD
                },
                health: 8,
                numRolls: 1) },
            { "Huntsman",
                (Sides.EIGHT,
                Colors.GREEN,
                new []{
                    Faces.BOW,
                    Faces.BOW,
                    Faces.SHIELD,
                    Faces.BOW,
                    Faces.SHIELD,
                    Faces.HEAL,
                    Faces.BOW,
                    Faces.SHIELD,
                },
                health: 8,
                numRolls: 1) },
            { "Bishop",
                (Sides.EIGHT,
                Colors.BLUE,
                new []{
                    Faces.HEAL,
                    Faces.HEAL,
                    Faces.SHIELD,
                    Faces.HEAL,
                    Faces.HEAL,
                    Faces.SHIELD,
                    Faces.HEAL,
                    Faces.HEAL,
                },
                health: 6,
                numRolls: 1) },

            { "Paladin",
                (Sides.TEN,
                Colors.RED,
                new []{
                    Faces.SWORD,
                    Faces.SHIELD,
                    Faces.SWORD,
                    Faces.SHIELD,
                    Faces.HEAL,
                    Faces.SWORD,
                    Faces.SWORD,
                    Faces.HEAL,
                    Faces.SWORD,
                    Faces.SWORD,
                },
                health: 15,
                numRolls: 2) },
            { "EagleEye",
                (Sides.TEN,
                Colors.GREEN,
                new []{
                    Faces.BOW,
                    Faces.BOW,
                    Faces.SHIELD,
                    Faces.BOW,
                    Faces.SHIELD,
                    Faces.BOW,
                    Faces.HEAL,
                    Faces.BOW,
                    Faces.SHIELD,
                    Faces.BOW,
                },
                health: 11,
                numRolls: 2) },
            { "HighBishop",
                (Sides.TEN,
                Colors.BLUE,
                new []{
                    Faces.HEAL,
                    Faces.HEAL,
                    Faces.SHIELD,
                    Faces.HEAL,
                    Faces.HEAL,
                    Faces.SWORD,
                    Faces.HEAL,
                    Faces.HEAL,
                    Faces.HEAL,
                    Faces.HEAL,
                },
                health: 8,
                numRolls: 2) },
        };

        public static Dictionary<string, (Sides sides, Colors color, Faces[] faces, int health, int numRolls)> DicePresetsT3 = new Dictionary<string, (Sides sides, Colors color, Faces[] faces, int health, int numRolls)>()
        {
            { "HighPaladin",
                (Sides.TWELVE,
                Colors.RED,
                new []{
                    Faces.SWORD,
                    Faces.SHIELD,
                    Faces.SWORD,
                    Faces.SHIELD,
                    Faces.HEAL,
                    Faces.SWORD,
                    Faces.SWORD,
                    Faces.SHIELD,
                    Faces.HEAL,
                    Faces.SWORD,
                    Faces.SHIELD,
                    Faces.SWORD,
                },
                health: 20,
                numRolls: 2) },
            { "MasterHunter",
                (Sides.TWELVE,
                Colors.GREEN,
                new []{
                    Faces.BOW,
                    Faces.BOW,
                    Faces.SHIELD,
                    Faces.BOW,
                    Faces.HEAL,
                    Faces.BOW,
                    Faces.SHIELD,
                    Faces.BOW,
                    Faces.HEAL,
                    Faces.BOW,
                    Faces.SHIELD,
                    Faces.BOW,
                },
                health: 15,
                numRolls: 3) },
            { "Divinator",
                (Sides.TWELVE,
                Colors.BLUE,
                new []{
                    Faces.HEAL,
                    Faces.HEAL,
                    Faces.SHIELD,
                    Faces.HEAL,
                    Faces.HEAL,
                    Faces.SWORD,
                    Faces.HEAL,
                    Faces.HEAL,
                    Faces.SHIELD,
                    Faces.HEAL,
                    Faces.HEAL,
                    Faces.SHIELD,
                },
                health: 10,
                numRolls: 2) },

            { "HonedSword",
                (Sides.TWENTY,
                Colors.RED,
                new []{
                    Faces.SWORD,
                    Faces.SHIELD,
                    Faces.SWORD,
                    Faces.HEAL,
                    Faces.SWORD,
                    Faces.SWORD,
                    Faces.SHIELD,
                    Faces.SWORD,
                    Faces.SWORD,
                    Faces.SWORD,
                    Faces.HEAL,
                    Faces.SWORD,
                    Faces.SWORD,
                    Faces.SWORD,
                    Faces.SHIELD,
                    Faces.SWORD,
                    Faces.SWORD,
                    Faces.SWORD,
                    Faces.HEAL,
                    Faces.SWORD,
                },
                health: 30,
                numRolls: 2) },
            { "PerfectShot",
                (Sides.TWENTY,
                Colors.GREEN,
                new []{
                    Faces.BOW,
                    Faces.BOW,
                    Faces.SHIELD,
                    Faces.BOW,
                    Faces.BOW,
                    Faces.SHIELD,
                    Faces.BOW,
                    Faces.HEAL,
                    Faces.BOW,
                    Faces.SHIELD,
                    Faces.BOW,
                    Faces.BOW,
                    Faces.HEAL,
                    Faces.BOW,
                    Faces.BOW,
                    Faces.SHIELD,
                    Faces.BOW,
                    Faces.BOW,
                    Faces.BOW,
                    Faces.BOW,
                },
                health: 15,
                numRolls: 4) },
            { "BlessedWords",
                (Sides.TWENTY,
                Colors.BLUE,
                new []{
                    Faces.HEAL,
                    Faces.HEAL,
                    Faces.SHIELD,
                    Faces.HEAL,
                    Faces.HEAL,
                    Faces.HEAL,
                    Faces.SWORD,
                    Faces.HEAL,
                    Faces.SHIELD,
                    Faces.HEAL,
                    Faces.HEAL,
                    Faces.SWORD,
                    Faces.HEAL,
                    Faces.HEAL,
                    Faces.SHIELD,
                    Faces.HEAL,
                    Faces.HEAL,
                    Faces.SHIELD,
                    Faces.HEAL,
                    Faces.SHIELD,
                },
                health: 10,
                numRolls: 3) },
        };
    }
}
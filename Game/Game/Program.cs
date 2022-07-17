using GameEngine;
using GameEngine._2D;
using GameEngine.UI;
using GameEngine.UI.AvaloniaUI;
using GameEngine.UI.Controllers;
using GameEngine.UI.NAudio;
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
#if DEBUG
        public const bool DEBUG = true;
        public const int SOUNDCHANNEL = 7;
#elif RELEASE
        public const bool DEBUG = false;
        public const int SOUNDCHANNEL = 0;
#endif

        public static readonly Random Random = new Random();

        public static GameEngine.GameEngine Engine;
        public static GameUI UI;
        public static NAudioSoundPlayer SoundPlayer;
        public static NAudio.Wave.WaveOutEvent _soundPlayer;
        public const int GameStateIndex = 0;
        public const int Width = 480;
        public const int Height = 320;

        public static bool Mute { get; set; } = false;
        public static bool Quiet { get; set; } = false;
        public static Entity MuteEntity { get; private set; }
        public static Entity MuteTextEntity { get; private set; }

        public static Entity QuietEntity { get; private set; }
        public static Entity QuietTextEntity { get; private set; }

        public static GameState State { get; private set; }

        public static Location GameLocation { get; private set; }
        public static Location MenuLocation { get; private set; }
        public static Location GameOverLocation { get; private set; }
        public static Location ShopLocation { get; private set; }
        public  static Location BattleLocation { get; private set; }
        public static Location UpgradeLocation { get; private set; }
        public static Location RecruitLocation { get; private set; }
        public static Location WinLocation { get; private set; }

        public static Scorecard Scorecard { get; private set; }

        public static Description2D PlayArea { get; private set; }

        public static DiceBag DiceBag { get; private set; }

        static async Task Main()
        {
            (Engine, UI) = new GameBuilder()
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
                .SoundPlayer(SoundPlayer = new NAudioSoundPlayer(SOUNDCHANNEL))
                .Build();
;
            GameLocation = new Location(new Description2D(0, 0, Width, Height));
            GameOverLocation = new Location(new Description2D(0, 0, Width, Height));
            BattleLocation = new Location(new Description2D(0, 0, Width, Height));
            ShopLocation = new Location(new Description2D(0, 0, Width, Height));
            UpgradeLocation = new Location(new Description2D(0, 0, Width, Height));
            RecruitLocation = new Location(new Description2D(0, 0, Width, Height));
            WinLocation = new Location(new Description2D(0, 0, Width, Height));

            new Sprite("dice", "Sprites.dice.png", 32, 32, 16, 16);
            new Sprite("diceFaces", "Sprites.diceFaces.png", 10, 10, 5, 5);
            new Sprite("DiceBag", "Sprites.dicebag.png", 105, 80, 0, 0);
            new Sprite("Scorecard", "Sprites.scorecard.png", 193, 300);
            new Sprite("Symbols", "Sprites.symbols.png", 16, 16, 8, 8);
            new Sprite("Button", "Sprites.button.png", 80, 32, 0, 0);
            new Sprite("desk", "Sprites.desk.png", Width, Height, 0, 0);
            new Sprite("JamLogo", "Sprites.Jam Logo 2022.png", 1920, 1053, 0, 0);
            new Sprite("Title", "Sprites.Title.png", 128, 80, 64, 0);


            Description2D d2d;
            Description2D d2dMute;
            Description2D d2dQuiet;
            MuteTextEntity = new Entity(d2d = new TextDescription("Mute:", Program.Width - 64, -4));
            MuteEntity = new Entity(d2d = d2dMute = new Description2D(Sprite.Sprites["Symbols"], Program.Width - 9, 8));
            d2d.ImageIndex = 9;
            d2d.ZIndex = 1000;

            QuietTextEntity = new Entity(d2d = new TextDescription("Quiet:", Program.Width - 144, -4));
            QuietEntity = new Entity(d2d = d2dQuiet = new Description2D(Sprite.Sprites["Symbols"], Program.Width - 96 + 10, 8));
            d2d.ImageIndex = 5;
            d2d.ZIndex = 1000;

            MainMenuSetup();

            Engine.SetLocation(GameStateIndex, MenuLocation);

            TickHandler act = (o, s) => { };
            act = (o, s) => {
                State = s;
                Engine.TickEnd(0) -= act;
            };

            _soundPlayer = typeof(NAudioSoundPlayer).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).First(f => f.Name == "player").GetValue(SoundPlayer) as NAudio.Wave.WaveOutEvent;

            Engine.TickEnd(0) += act;

            Engine.TickEnd(0) += (object o, GameState state) =>
            {
                if (DEBUG)
                {
                    if (state.Controllers[1][Keys.DEBUG].IsPress())
                    {
                        //List<Dice> dice = Program.GameLocation.Entities.Where(entity => entity is Dice).Select(entity => entity as Dice).ToList();
                        //GameRules.UpgradeDice(dice);
                        //GameRules.RecruitDice();

                        //StopSounds();
                        //GameRules.OpenShop();
                        //GameRules.GameOver();
                    }
                }

                if (state.Controllers[0][Program.Keys.CLICK].IsPress())
                {
                    MouseControllerInfo info = state.Controllers[0][Program.Keys.CLICK].Info as MouseControllerInfo;
                    if (d2dMute.IsCollision(new Description2D(info.X + d2dMute.Sprite.X, info.Y + d2dMute.Sprite.Y, 1, 1)))
                    {
                        if (Mute)
                        {
                            d2dMute.ImageIndex += 1;
                            Mute = false;
                        }
                        else
                        {
                            d2dMute.ImageIndex -= 1;
                            Mute = true;
                            StopSounds();
                        }
                    }
                    if (d2dQuiet.IsCollision(new Description2D(info.X + d2dQuiet.Sprite.X, info.Y + d2dQuiet.Sprite.Y, 1, 1)))
                    {
                        if (Quiet)
                        {
                            d2dQuiet.ImageIndex -= 1;
                            Quiet = false;
                        }
                        else
                        {
                            d2dQuiet.ImageIndex += 1;
                            Quiet = true;
                            StopSounds();
                        }
                    }
                }
            };

            await Engine.Start();
        }

        public static void MakeQuiet()
        {
            Description2D d2dQuiet = QuietEntity.Description as Description2D;

            d2dQuiet.ImageIndex = 6;
            Quiet = true;
            StopSounds();
        }

        static List<string> rollingSounds = new List<string>()
        {
            "Sounds.longdice.wav",
            "Sounds.mediumdice.wav",
            "Sounds.mediumdice2.wav",
            "Sounds.mediumdice3.wav",
            "Sounds.mediumdice4.wav",
            "Sounds.shortdice.wav",
            "Sounds.shortdice2.wav",
            "Sounds.shortdice3.wav",
        };

        static List<string> rollingQuickSounds = new List<string>()
        {
            "Sounds.shortdice.wav",
            "Sounds.shortdice2.wav",
            "Sounds.shortdice3.wav",
        };

        static List<string> collisionSounds = new List<string>()
        {
            "Sounds.collidedice.wav",
            "Sounds.collidedice2.wav",
            "Sounds.collidedice3.wav",
        };

        public static void PlayRollingDice()
        {
            if (!Mute)
            {
                UI.PlayResource(rollingSounds[Program.Random.Next(rollingSounds.Count)]);
            }
        }

        public static void PlayQuickRollingDice()
        {
            if (!Mute)
            {
                UI.PlayResource(rollingQuickSounds[Program.Random.Next(rollingQuickSounds.Count)]);
            }
        }

        public static void PlayCollisionDice()
        {
            if (!Mute)
            {
                UI.PlayResource(collisionSounds[Program.Random.Next(collisionSounds.Count)]);
            }
        }

        public static void PlayBowSound()
        {
            if (!Mute)
            {
                UI.PlayResource("Sounds.bow2.wav");
            }
        }

        public static void PlayAttackSound()
        {
            if (!Mute)
            {
                UI.PlayResource("Sounds.attack.wav");
            }
        }

        public static void PlayBlockedSound()
        {
            if (!Mute)
            {
                UI.PlayResource("Sounds.blocked.wav");
            }
        }

        public static void PlayDamageSound(bool blocked)
        {
            if (!Mute)
            {
                if (blocked)
                {
                    UI.PlayResource("Sounds.damage.wav");
                }
                else
                {
                    UI.PlayResource("Sounds.damage2.wav");
                }
            }
        }

        public static void PlayArrowSound()
        {
            if (!Mute)
            {
                UI.PlayResource("Sounds.arrow.wav");
            }
        }

        public static void PlayCoinSound()
        {
            if (!Mute)
            {
                UI.PlayResource("Sounds.coin.wav");
            }
        }

        public static void PlayHealSound()
        {
            if (!Mute)
            {
                UI.PlayResource("Sounds.heal.wav");
            }
        }

        private static int timer = 0;
        private static bool addedRepeatForChatter;
        public static void PlayChatterSound()
        {
            if (!Mute)
            {
                if (!Quiet && timer <= 0)
                {
                    UI.PlayResource("Sounds.chatter2.wav");
                    timer = Program.FPS * 7;
                }

                if (!addedRepeatForChatter)
                {
                    Engine.TickEnd(0) += (object sender, GameState state) =>
                    {
                        timer--;
                        if (timer <= 0 && state.Location == GameLocation)
                        {
                            if (!Mute && !Quiet)
                            {
                                UI.PlayResource("Sounds.chatter2.wav");
                                timer = Program.FPS * 7;
                            }
                        }
                    };
                    addedRepeatForChatter = true;
                }
            }
        }

        public static void StopSounds()
        {
            // TODO: Expose a way to stop sound through the game's sound player abstraction
            // Info: The sound buffer isn't being cleared, and is being re-used when another sound starts up again
            _soundPlayer.Stop();
            //NAudio.Wave.WaveOutBuffer[] buffers = typeof(NAudio.Wave.WaveOutEvent).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).First(f => f.Name == "buffers").GetValue(_soundPlayer) as NAudio.Wave.WaveOutBuffer[];
            
            // This doesn't seem to work
            //for (int i = 0; i < buffers.Length; i++)
            //{
            //    byte[] buffer = typeof(NAudio.Wave.WaveOutBuffer).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).First(f => f.Name == "buffer").GetValue(buffers[i]) as byte[];
            //    for (int j = 0; j < buffer.Length; j++)
            //    {
            //        buffer[j] = 0;
            //    }

            //    typeof(NAudio.Wave.WaveOutBuffer).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).First(f => f.Name == "buffer").SetValue(buffers[i], buffer);
            //}
        }

        private static void MainMenuSetup()
        {
            WinLocation.AddEntity(new Entity(new Description2D(Sprite.Sprites["desk"], 0, 0)));
            WinLocation.AddEntity(new Entity(new Description2D(Sprite.Sprites["Title"], Program.Width / 2 - 64, 16, 128 * 2, 80 * 2)));
            WinLocation.AddEntity(new Entity(new Description2D(Sprite.Sprites["JamLogo"], Program.Width / 2 - 64, Program.Height / 2 + 48, 128, 80)));
            WinLocation.AddEntity(new Entity(new TextDescription("          You win!\nThank you for playing", Program.Width / 2 - "Thank you for playing".Length * 18 / 4 + 6, Program.Height / 2 + 5)));
            WinLocation.AddEntity(MuteEntity);
            WinLocation.AddEntity(MuteTextEntity);
            WinLocation.AddEntity(QuietEntity);
            WinLocation.AddEntity(QuietTextEntity);

            MenuLocation.AddEntity(new Entity(new Description2D(Sprite.Sprites["desk"], 0, 0)));
            MenuLocation.AddEntity(new Entity(new Description2D(Sprite.Sprites["Title"], Program.Width / 2 - 64, 16, 128 * 2, 80 * 2)));
            MenuLocation.AddEntity(new Entity(new Description2D(Sprite.Sprites["JamLogo"], Program.Width / 2 - 64, Program.Height / 2 + 48, 128, 80)));
            MenuLocation.AddEntity(new Entity(new TextDescription("Click to continue", Program.Width / 2 - "Click to continue".Length * 18 / 4 + 6, Program.Height  - 32)));
            MenuLocation.AddEntity(MuteEntity);
            MenuLocation.AddEntity(MuteTextEntity);
            MenuLocation.AddEntity(QuietEntity);
            MenuLocation.AddEntity(QuietTextEntity);
            Engine.TickEnd(0) += (object sender, GameState state) =>
            {
                if (state.Location == MenuLocation && state.Controllers[0][Keys.CLICK].IsPress())
                {
                    Engine.SetLocation(Program.GameStateIndex, GameLocation);
                    GameSetup();
                }
            };

            //Game Over
            GameOverLocation.AddEntity(new Entity(new Description2D(Sprite.Sprites["desk"], 0, 0)));
            GameOverLocation.AddEntity(new Entity(new Description2D(Sprite.Sprites["Title"], Program.Width / 2 - 64, 16, 128 * 2, 80 * 2)));
            GameOverLocation.AddEntity(new Entity(new Description2D(Sprite.Sprites["JamLogo"], Program.Width / 2 - 64, Program.Height / 2 + 48, 128, 80)));
            GameOverLocation.AddEntity(new Entity(new TextDescription("       Game Over\nThank you for playing", Program.Width / 2 - "Thank you for playing".Length * 18 / 4 + 6, Program.Height / 2 + 5)));
            GameOverLocation.AddEntity(MuteEntity);
            GameOverLocation.AddEntity(MuteTextEntity);
            GameOverLocation.AddEntity(QuietEntity);
            GameOverLocation.AddEntity(QuietTextEntity);

            // Shop
            Entity ent;
            Description2D d2d;
            ShopLocation.AddEntity(new Entity(new Description2D(Sprite.Sprites["desk"], 0, 0)));
            ShopLocation.AddEntity(MuteEntity);
            ShopLocation.AddEntity(MuteTextEntity);
            ShopLocation.AddEntity(QuietEntity);
            ShopLocation.AddEntity(QuietTextEntity);

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
                        GameRules.SpendCoins(5);
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
                        GameRules.SpendCoins(10);
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
                        description.ImageIndex = 6;
                        GameRules.RecruitmentSlots++;
                        GameRules.SpendCoins(10);
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
                        GameRules.RecruitmentTier++;
                        GameRules.SpendCoins(15);
                    }
                }
            };

            //int yPos = Program.Height - 32 - 16;
            int yPos = 16;
            ShopLocation.AddEntity(new Button(Program.ShopLocation, "Back", Program.Width - 80 - 16, yPos, GameRules.CloseShop));

            // Recruitment
            RecruitLocation.AddEntity(new Entity(new Description2D(Sprite.Sprites["desk"], 0, 0)));
            RecruitLocation.AddEntity(new Button(Program.RecruitLocation, "Back", Program.Width - 80 - 16, yPos, GameRules.CloseRecruitment));
            RecruitLocation.AddEntity(MuteEntity);
            RecruitLocation.AddEntity(MuteTextEntity);
            RecruitLocation.AddEntity(QuietEntity);
            RecruitLocation.AddEntity(QuietTextEntity);

            // Upgrade
            UpgradeLocation.AddEntity(new Entity(new Description2D(Sprite.Sprites["desk"], 0, 0)));
            UpgradeLocation.AddEntity(new Button(Program.UpgradeLocation, "Back", Program.Width - 80 - 16, yPos, GameRules.CloseUpgrades));
            UpgradeLocation.AddEntity(MuteEntity);
            UpgradeLocation.AddEntity(MuteTextEntity);
            UpgradeLocation.AddEntity(QuietEntity);
            UpgradeLocation.AddEntity(QuietTextEntity);


            BattleLocation.AddEntity(new Entity(new Description2D(Sprite.Sprites["desk"], 0, 0)));
            BattleLocation.AddEntity(MuteEntity);
            BattleLocation.AddEntity(MuteTextEntity);
            BattleLocation.AddEntity(QuietEntity);
            BattleLocation.AddEntity(QuietTextEntity);
        }

        private static void GameSetup()
        {
            GameLocation.AddEntity(new Entity(new Description2D(Sprite.Sprites["desk"], 0, 0)));
            GameLocation.AddEntity(MuteEntity);
            GameLocation.AddEntity(MuteTextEntity);
            GameLocation.AddEntity(QuietEntity);
            GameLocation.AddEntity(QuietTextEntity);

            int x = Program.Width / 2 - 48 + 16;
            int xEnd = Program.Width - 16;
            int y = 16;
            int yEnd = Program.Height - 96 - 16;
            GameLocation.AddEntity(new Entity(PlayArea = new Description2D(new Sprite("desk2", "Sprites.desk2.png", 240, 160, 0, 0), x, y, xEnd - x, yEnd - y)));

            GameLocation.AddEntity(Scorecard = new Scorecard(10, 10));

            Scorecard.LoadSheet(GameLocation);

            GameLocation.AddEntity(new Button(Program.GameLocation, "Roll", Program.Width / 2 - 24, Program.Height - 96, GameRules.Roll));

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

            PlayChatterSound();
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
            { (int)Avalonia.Input.Key.OemCloseBrackets, Keys.DEBUG },
        };

        public enum Sides { FOUR = 4, SIX = 6, EIGHT = 8, TEN = 10, TWELVE = 12, TWENTY = 20 }

        public enum Colors { RED = 0, BLUE = 1, GREEN = 2, YELLOW = 3 }

        public enum Faces { NONE = 0, SWORD = 1, SHIELD = 2, HEAL = 3, BOW = 4}

        public enum Keys { CLICK, RCLICK, ROLL, MOUSEINFO, DEBUG }

        public static Dictionary<int, int> SidesToUpgradeIndex { get; private set; } = new Dictionary<int, int>()
        {
            { 4, 0 },
            { 6, 1 },
            { 8, 2 },
            { 10, 3 },
            { 12, 4 },
            { 20, 5 },
        };

        public static List<string> WarriorUpgrades { get; private set; } = new List<string>()
        {
            "Warrior",
            "Squire",
            "Knight",
            "Paladin",
            "HighPaladin",
            "HonedSword"
        };

        public static List<string> ArcherUpgrades { get; private set; } = new List<string>()
        {
            "Archer",
            "Sharpshooter",
            "Huntsman",
            "EagleEye",
            "MasterHunter",
            "PerfectShot"
        };

        public static List<string> HealerUpgrades { get; private set; } = new List<string>()
        {
            "Healer",
            "Cleric",
            "Bishop",
            "HighBishop",
            "Divinator",
            "BlessedWords"
        };

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
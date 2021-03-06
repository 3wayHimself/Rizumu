﻿using Rizumu.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rizumu.Objects;
using Microsoft.Xna.Framework.Input;
using Rizumu.GuiObjects;
using System.IO;
using Newtonsoft.Json.Linq;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input.Touch;
using Rizumu.Enums;

namespace Rizumu.GameObjects.Screens
{
    class InGame : IScreen
    {
        public Map Playing;
        public List<Note> NotesLeft;
        public List<Note> NotesUp;
        public List<Note> NotesRight;
        public List<Note> NotesDown;
        public List<SNote> SNotesLeft;
        public List<SNote> SNotesUp;
        public List<SNote> SNotesRight;
        public List<SNote> SNotesDown;
        public bool MapLoaded = false;
        public bool Paused = false;
        public int Timer;
        public int lastnote = 0;
        public Background Background;
        public Background PauseOverlay;
        public Sprite VisionUp;
        public Sprite VisionDown;
        public Sprite VisionLeft;
        public Sprite VisionRight;
        public Sprite OpacityOverlay;
        public Replay Recording;
        public bool Replaying = false;
        public bool ready;
        public Replay Replay;
        public float Rotation = 0f;
        float Modrotation;

        public KeyboardState OldState;

        public Sprite LeftNote;
        public Sprite UpNote;
        public Sprite RightNote;
        public Sprite DownNote;

        public Button ExitButton;
        public Button ResumeButton;

        public Text TimerTex;
        public Text ComboText;
        public Text ComboTextSmall;
        public int CurrentCombo = 0;
        public int HighestCombo = 0;
        bool skippable = false;
        int firstnote = 0;
        SpriteBatch sb;

        public Text ModCollection;
        public int ScreenWidth;

        public bool oldleft;
        public bool oldup;
        public bool oldright;
        public bool olddown;

        public Screen Name => Screen.Ingame;

        public bool up;
        public TimeSpan LeftVisTS = TimeSpan.FromSeconds(0);
        public TimeSpan UpVisTS = TimeSpan.FromSeconds(0);
        public TimeSpan RightVisTS = TimeSpan.FromSeconds(0);
        public TimeSpan DownVisTS = TimeSpan.FromSeconds(0);
        public void Draw(SpriteBatch spriteBatch, GameTime gameTime, Rectangle cursor, bool clicked, GraphicsDevice g)
        {
            sb = spriteBatch;
            Rotation = Timer * 0.005f;
            if (GameData.Instance.Mods.RotationMode)
                Modrotation = Timer * 0.005f;
            if (CurrentCombo > HighestCombo)
                HighestCombo = CurrentCombo;
            if (!MapLoaded)
            {
                ready = false;
                IsRestarted = false;
                GameData.MusicManager.Stop();
                Playing = GameData.MapManager.Current;
                Timer = Playing.Offset;
                OpacityOverlay.Alpha = GameData.Instance.Mods.BackgroundOpacity;
                #region Preloading notes
                NotesLeft = new List<Note>();
                NotesUp = new List<Note>();
                NotesRight = new List<Note>();
                NotesDown = new List<Note>();
                SNotesLeft = new List<SNote>();
                SNotesUp = new List<SNote>();
                SNotesRight = new List<SNote>();
                SNotesDown = new List<SNote>();
                CurrentCombo = 0;
                HighestCombo = 0;
                #region Standard notes
                foreach (int n in Playing.NotesLeft)
                {
                    float sped = Playing.Speeds.Last(x => x.Time < n).NewSpeed * GameData.Instance.Mods.SpeedMultiplier;
                    NotesLeft.Add(new Note(spriteBatch, NoteMode.left, Background.Width, Background.Height, sped)
                    {
                        Hit = false,
                        Position = 0,
                        Time = n,
                        Accuracy = 0
                    });
                }

                foreach (int n in Playing.NotesUp)
                {
                    float sped = Playing.Speeds.Last(x => x.Time < n).NewSpeed * GameData.Instance.Mods.SpeedMultiplier;
                    NotesUp.Add(new Note(spriteBatch, NoteMode.up, Background.Width, Background.Height, sped)
                    {
                        Hit = false,
                        Position = 0,
                        Time = n,
                        Accuracy = 0
                    });
                }

                foreach (int n in Playing.NotesRight)
                {
                    float sped = Playing.Speeds.Last(x => x.Time < n).NewSpeed * GameData.Instance.Mods.SpeedMultiplier;
                    NotesRight.Add(new Note(spriteBatch, NoteMode.right, Background.Width, Background.Height, sped)
                    {
                        Hit = false,
                        Position = 0,
                        Time = n,
                        Accuracy = 0
                    });
                }

                foreach (int n in Playing.NotesDown)
                {
                    float sped = Playing.Speeds.Last(x => x.Time < n).NewSpeed * GameData.Instance.Mods.SpeedMultiplier;
                    NotesDown.Add(new Note(spriteBatch, NoteMode.down, Background.Width, Background.Height, sped)
                    {
                        Hit = false,
                        Position = 0,
                        Time = n,
                        Accuracy = 0
                    });
                }
                #endregion
                #region Slider notes
                foreach (int n in Playing.SlidesLeft)
                {
                    SNotesLeft.Add(new SNote(spriteBatch, NoteMode.left, Background.Width, Background.Height)
                    {
                        Hit = false,
                        Position = 0,
                        Time = n,
                        Accuracy = 0
                    });
                }

                foreach (int n in Playing.SlidesUp)
                {
                    SNotesUp.Add(new SNote(spriteBatch, NoteMode.up, Background.Width, Background.Height)
                    {
                        Hit = false,
                        Position = 0,
                        Time = n,
                        Accuracy = 0
                    });
                }

                foreach (int n in Playing.SlidesRight)
                {
                    SNotesRight.Add(new SNote(spriteBatch, NoteMode.right, Background.Width, Background.Height)
                    {
                        Hit = false,
                        Position = 0,
                        Time = n,
                        Accuracy = 0
                    });
                }

                foreach (int n in Playing.SlidesDown)
                {
                    SNotesDown.Add(new SNote(spriteBatch, NoteMode.down, Background.Width, Background.Height)
                    {
                        Hit = false,
                        Position = 0,
                        Time = n,
                        Accuracy = 0
                    });
                }
                #endregion
                #endregion
                Background.Texture = Playing.Background;

                VisionUp = new Sprite(spriteBatch, 0, 0, GameData.Instance.CurrentSkin.VisionUp, Color.White);
                VisionDown = new Sprite(spriteBatch, 0, 0, GameData.Instance.CurrentSkin.VisionDown, Color.White);
                VisionLeft = new Sprite(spriteBatch, 0, 0, GameData.Instance.CurrentSkin.VisionLeft, Color.White);
                VisionRight = new Sprite(spriteBatch, 0, 0, GameData.Instance.CurrentSkin.VisionRight, Color.White);
                VisionUp.Alpha = 0f;
                VisionDown.Alpha = 0f;
                VisionLeft.Alpha = 0f;
                VisionRight.Alpha = 0f;

                lastnote = GetLastNote();
                firstnote = GetFirstNote();
                if (firstnote > 2500)
                    skippable = true;
                Recording = new Replay();
                Recording.Md5 = Playing.MD5;
                Recording.Player = GameData.Instance.Options.Player;
                if (GameData.Instance.Mods.Automode)
                    ready = true;
                MapLoaded = true;
            }
            else
            {
                var NewState = Keyboard.GetState();
                bool LeftPress = false;
                bool UpPress = false;
                bool RightPress = false;
                bool DownPress = false;

                bool LeftHold = false;
                bool UpHold = false;
                bool RightHold = false;
                bool DownHold = false;

                if (skippable && NewState.IsKeyDown(Keys.Space))
                {
                    GameData.MusicManager.Restart(TimeSpan.FromMilliseconds((GetFirstNote() - 500) * 2));
                    skippable = false;
                }
                if (Timer > firstnote)
                {
                    skippable = false;
                }

                if (!Replaying)
                {
                    if (!GameData.Instance.Mods.Automode)
                    {
                        if (Game1.Windows)
                        {
                            if (NewState.IsKeyDown((Keys)GameData.Instance.Options.Left) && !OldState.IsKeyDown((Keys)GameData.Instance.Options.Left))
                            {
                                Recording.PressesLeft.Add(Timer);
                                LeftPress = true;
                            }
                            if (NewState.IsKeyDown((Keys)GameData.Instance.Options.Up) && !OldState.IsKeyDown((Keys)GameData.Instance.Options.Up))
                            {
                                Recording.PressesUp.Add(Timer);
                                UpPress = true;
                            }
                            if (NewState.IsKeyDown((Keys)GameData.Instance.Options.Right) && !OldState.IsKeyDown((Keys)GameData.Instance.Options.Right))
                            {
                                Recording.PressesRight.Add(Timer);
                                RightPress = true;
                            }
                            if (NewState.IsKeyDown((Keys)GameData.Instance.Options.Down) && !OldState.IsKeyDown((Keys)GameData.Instance.Options.Down))
                            {
                                Recording.PressesDown.Add(Timer);
                                DownPress = true;
                            }

                            LeftHold = NewState.IsKeyDown((Keys)GameData.Instance.Options.Left);

                            UpHold = NewState.IsKeyDown((Keys)GameData.Instance.Options.Up);

                            RightHold = NewState.IsKeyDown((Keys)GameData.Instance.Options.Right);

                            DownHold = NewState.IsKeyDown((Keys)GameData.Instance.Options.Down);
                        }
                        else
                        {
                            TouchCollection touchCollection = TouchPanel.GetState();
                            foreach (var t in touchCollection)
                            {
                                float x = t.Position.X;
                                float y = t.Position.Y;
                                int gbh = GameData.globalheight;
                                int gbw = GameData.globalwidth;
                                if (y > gbh / 4 && y < (gbh / 4 * 3))
                                {
                                    Recording.PressesDown.Add(Timer);
                                    DownHold = true;
                                    if (!olddown)
                                        DownPress = true;
                                }
                                else
                                {
                                    Recording.PressesUp.Add(Timer);
                                    UpHold = true;
                                    if (!oldup)
                                        UpPress = true;
                                }
                                if (x > GameData.globalwidth / 2)
                                {
                                    Recording.PressesRight.Add(Timer);
                                    RightHold = true;
                                    if (!oldright)
                                        RightPress = true;
                                }
                                else
                                {
                                    Recording.PressesLeft.Add(Timer);
                                    DownHold = true;
                                    if (!oldleft)
                                        LeftPress = true;
                                }
                                olddown = DownPress;
                                oldleft = LeftPress;
                                oldright = RightPress;
                                olddown = DownPress;
                            }
                        }
                    }
                    up = UpPress;
                }
                else
                {
                    if (Replay.PressesLeft.Contains(Timer))
                    {
                        LeftPress = true;
                        LeftHold = true;
                    }
                    if (Replay.PressesUp.Contains(Timer))
                    {
                        UpPress = true;
                        UpHold = true;
                    }
                    if (Replay.PressesRight.Contains(Timer))
                    {
                        RightPress = true;
                        RightHold = true;
                    }
                    if (Replay.PressesDown.Contains(Timer))
                    {
                        DownPress = true;
                        DownHold = true;
                    }
                }

                if (NewState.IsKeyDown(Keys.Escape) && !OldState.IsKeyDown(Keys.Escape))
                {
                    Paused = !Paused;
                }

                OldState = NewState;

                if (!GameData.Instance.Mods.Automode)
                {
                    if (Game1.Windows)
                    {
                        LeftNote.Color = NewState.IsKeyDown((Keys)GameData.Instance.Options.Left) ? Color.DarkGray : Color.White;

                        UpNote.Color = NewState.IsKeyDown((Keys)GameData.Instance.Options.Up) ? Color.DarkGray : Color.White;

                        RightNote.Color = NewState.IsKeyDown((Keys)GameData.Instance.Options.Right) ? Color.DarkGray : Color.White;

                        DownNote.Color = NewState.IsKeyDown((Keys)GameData.Instance.Options.Down) ? Color.DarkGray : Color.White;
                    }
                    else
                    {
                        LeftNote.Color = LeftPress ? Color.DarkGray : Color.White;

                        UpNote.Color = UpPress ? Color.DarkGray : Color.White;

                        RightNote.Color = RightPress ? Color.DarkGray : Color.White;

                        DownNote.Color = DownPress ? Color.DarkGray : Color.White;
                    }
                }

                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                {
                    Paused = true;
                }

                if (GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed)
                {
                    MapLoaded = false;
                }

                Background.Draw();
                OpacityOverlay.Draw();

                #region Gameplay (yay!)
                float leftdist = VisionLeft.Alpha;
                float updist = VisionUp.Alpha;
                float rightdist = VisionRight.Alpha;
                float downdist = VisionDown.Alpha;
                foreach (Note n in NotesLeft)
                {
                    //if (n.Time - ((Background.Width / 2) + n.NoteSprite.Texture.Width) < Timer)
                    n.Draw(ref LeftPress, Paused, ready, Rotation, ref CurrentCombo, ref leftdist, Timer, GameData.Instance.Mods.Automode);
                }
                foreach (Note n in NotesUp)
                {
                    //if (n.Time - ((Background.Height / 2) + n.NoteSprite.Texture.Height) < Timer)
                    n.Draw(ref UpPress, Paused, ready, Rotation, ref CurrentCombo, ref updist, Timer, GameData.Instance.Mods.Automode);
                }
                foreach (Note n in NotesRight)
                {
                    //if (n.Time - ((Background.Width / 2) + (n.NoteSprite.Texture.Width * 2)) < Timer)
                    n.Draw(ref RightPress, Paused, ready, Rotation, ref CurrentCombo, ref rightdist, Timer, GameData.Instance.Mods.Automode);
                }
                foreach (Note n in NotesDown)
                {
                    //if (n.Time - ((Background.Height / 2) + (n.NoteSprite.Texture.Height * 2)) < Timer)
                    n.Draw(ref DownPress, Paused, ready, Rotation, ref CurrentCombo, ref downdist, Timer, GameData.Instance.Mods.Automode);
                }

                LeftNote.Rotation = Rotation;
                UpNote.Rotation = Rotation;
                RightNote.Rotation = Rotation;
                DownNote.Rotation = Rotation;

                if (leftdist == 0.4f)
                    LeftVisTS = MediaPlayer.PlayPosition;
                if (updist == 0.4f)
                    UpVisTS = MediaPlayer.PlayPosition;
                if (rightdist == 0.4f)
                    RightVisTS = MediaPlayer.PlayPosition;
                if (downdist == 0.4f)
                    DownVisTS = MediaPlayer.PlayPosition;

                VisionUp.Alpha = updist - (float)((MediaPlayer.PlayPosition - UpVisTS).TotalMilliseconds / 20000);

                VisionDown.Alpha = downdist - (float)((MediaPlayer.PlayPosition - DownVisTS).TotalMilliseconds / 20000);

                VisionLeft.Alpha = leftdist - (float)((MediaPlayer.PlayPosition - LeftVisTS).TotalMilliseconds / 20000);

                VisionRight.Alpha = rightdist - (float)((MediaPlayer.PlayPosition - RightVisTS).TotalMilliseconds / 20000);

                VisionUp.DrawScaled(Background.Width, Background.Height);
                VisionDown.DrawScaled(Background.Width, Background.Height);
                VisionLeft.DrawScaled(Background.Width, Background.Height);
                VisionRight.DrawScaled(Background.Width, Background.Height);
                #endregion

                LeftNote.Draw(true);
                UpNote.Draw(true);
                RightNote.Draw(true);
                DownNote.Draw(true);

                if (Paused)
                {
                    ready = false;
                    PauseOverlay.Draw();
                    ResumeButton.Draw(cursor, clicked);
                    ExitButton.Draw(cursor, clicked);
                }

                var totaltime = TimeSpan.FromSeconds((lastnote + 1000) / 500);
                var currenttime = TimeSpan.FromSeconds(Timer / 500);

                TimerTex.Content = $"{currenttime.ToReadableString()} / {totaltime.ToReadableString()}";
                TimerTex.Draw();
                ComboText.Content = $"{CurrentCombo}";
                ComboText.Draw();
                ComboTextSmall.Content = $"{HighestCombo}";
                ComboTextSmall.Draw();

                if (NewState.IsKeyDown(Keys.OemTilde))
                    MapLoaded = false;
            }

            if (!ready && !Paused)
                new Background(spriteBatch, GameData.Instance.CurrentSkin.GetReady, Color.White, Background.Width, Background.Height).Draw();
            if (up)
                ready = true;

            ModCollection.Content = GameData.Instance.Mods.GetCollectionString();
            ModCollection.X = (ScreenWidth - ModCollection.Width) - 5;
            ModCollection.Draw();
        }

        public void Preload(SpriteBatch spriteBatch, GraphicsDeviceManager Graphics)
        {
            Background = new Background(spriteBatch, GameData.Instance.CurrentSkin.MenuBackground, Color.White, GameData.globalwidth, GameData.globalheight);
            PauseOverlay = new Background(spriteBatch, GameData.Instance.CurrentSkin.PauseOverlay, Color.White, GameData.globalwidth, GameData.globalheight);
            var notetex = GameData.Instance.CurrentSkin.Note;
            LeftNote = new Sprite(spriteBatch, (int)(Background.Width / 2 - notetex.Width * 1.5), (int)(Background.Height / 2 - notetex.Width * 0.5), notetex, Color.White);
            UpNote = new Sprite(spriteBatch, (int)(Background.Width / 2 - notetex.Width * 0.5), (int)(Background.Height / 2 - notetex.Width * 1.5), notetex, Color.White);
            RightNote = new Sprite(spriteBatch, (int)(Background.Width / 2 + notetex.Width * 0.5), (int)(Background.Height / 2 - notetex.Width * 0.5), notetex, Color.White);
            DownNote = new Sprite(spriteBatch, (int)(Background.Width / 2 - notetex.Width * 0.5), (int)(Background.Height / 2 + notetex.Width * 0.5), notetex, Color.White);
            TimerTex = new Text(spriteBatch, GameData.Instance.CurrentSkin.Font, "" + Timer, 0, 0, Color.White);
            OpacityOverlay = new Sprite(spriteBatch, 0, 0, GameData.Instance.CurrentSkin.BgOverlay, Color.White);

            ComboText = new Text(spriteBatch, GameData.Instance.CurrentSkin.FontBig, "0", 15, 0, Color.White);
            ComboText.Y = Background.Height - ComboText.Height - 15;
            ComboTextSmall = new Text(spriteBatch, GameData.Instance.CurrentSkin.FontSmall, "0", 15, 0, Color.White);
            ComboTextSmall.Y = ((Background.Height - ComboText.Height - 15) - ComboTextSmall.Height) - 3;
            // Making sure OldState is not null
            OldState = Keyboard.GetState();
            ResumeButton = new Button(spriteBatch, (GameData.globalwidth / 2) - (GameData.Instance.CurrentSkin.Button.Width / 2),
                GameData.globalheight / 2 - (GameData.Instance.CurrentSkin.Button.Height) - 25, GameData.Instance.CurrentSkin.Button,
                GameData.Instance.CurrentSkin.ButtonHover, "Resume");
            ExitButton = new Button(spriteBatch, (GameData.globalwidth / 2) - (GameData.Instance.CurrentSkin.Button.Width / 2),
                GameData.globalheight / 2 + 25, GameData.Instance.CurrentSkin.Button,
                GameData.Instance.CurrentSkin.ButtonHover, "Exit");

            ResumeButton.OnClick += (sender, e) =>
            {
                Paused = false;
            };

            ExitButton.OnClick += (sender, e) =>
            {
                MapLoaded = false;
                GameData.Instance.CurrentScreen = Screen.Select;
                Paused = false;
                Replaying = false;
                GameData.MusicManager.UnPause();
                GameData.MusicManager.Restart();
            };

            ModCollection = new Text(spriteBatch, GameData.Instance.CurrentSkin.FontSmall, "", GameData.globalwidth, 5, Color.Azure);
            ScreenWidth = GameData.globalwidth;
        }

        public bool IsRestarted = false;
        public bool LetsGoPlayed = false;
        public void Update(GameTime gameTime, Rectangle cursor, bool clicked)
        {
            if (MapLoaded && !Paused && ready)
            {
                Timer = (int)((MediaPlayer.PlayPosition.TotalMilliseconds * 500) / 1000) + GameData.MapManager.Current.Offset;
            }
            if (ready && !IsRestarted)
            {
                GameData.MusicManager.Restart();
                IsRestarted = true;
                LetsGoPlayed = false;
            }
            if (Paused)
            {
                GameData.MusicManager.Pause();
                LetsGoPlayed = false;
                ready = false;
            }
            else if (ready)
            {
                GameData.MusicManager.UnPause();
                if (!LetsGoPlayed)
                {
                    GameData.Instance.CurrentSkin.LetsGo.Play();
                    LetsGoPlayed = true;
                }
            }
            if (Timer > lastnote + 1000 || MediaPlayer.State == MediaState.Stopped && ready)
            {
                string path = Path.Combine("replays/", $"{Playing.Name}-{GameData.Instance.Options.Player}-{new Random().Next(int.MaxValue)}.rizumuplay");
                File.Create(path).Close();
                File.WriteAllText(path, JObject.FromObject(Recording).ToString());
                GameData.Instance.CurrentScreen = Screen.Results;
                Replaying = false;
                ((Results)GameData.Instance.Screens.Find(x => x.Name == Screen.Results)).ResultsPreloaded = false;
                Timer = 0;
                LetsGoPlayed = false;
                ready = false;
            }
        }

        public int GetLastNote()
        {
            int left = 0;
            int up = 0;
            int right = 0;
            int down = 0;
            foreach (Note n in NotesLeft)
            {
                if (n.Time > left)
                    left = n.Time;
            }
            foreach (Note n in NotesUp)
            {
                if (n.Time > up)
                    up = n.Time;
            }
            foreach (Note n in NotesRight)
            {
                if (n.Time > right)
                    right = n.Time;
            }
            foreach (Note n in NotesDown)
            {
                if (n.Time > down)
                    down = n.Time;
            }

            return Math.Max(left, Math.Max(up, Math.Max(right, down)));
        }

        public int GetFirstNote()
        {
            int left = 0;
            int up = 0;
            int right = 0;
            int down = 0;
            left = NotesLeft[0].Time;
            up = NotesUp[0].Time;
            right = NotesRight[0].Time;
            down = NotesDown[0].Time;

            return Math.Min(left, Math.Min(up, Math.Min(right, down)));
        }


    }

    public static class tshelper
    {
        public static string ToReadableString(this TimeSpan span)
        {
            string formatted = $"{span.Minutes:0}:{$"{span.Seconds:0}".PadLeft(2, '0')}";


            if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

            if (string.IsNullOrEmpty(formatted)) formatted = "0 seconds";

            return formatted;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.IO;

/*
 * VDZ's Basic XNA Sprite Engine - A sprite engine for XNA 4.0 (version 3)

    Written in 2012-2013 by Vincent de Zwaan

    To the extent possible under law, the author(s) have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty. 

    You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>. 
  
 * Any modifications I make to this software in the future will likely be published at <https://github.com/VDZx/VBXSE>.
 */

namespace VBXSE
{
    //SpriteEngine - Main class for VDZ's Basic XNA Sprite Engine.
    public static partial class SpriteEngine
    {
        //The default resolution.
        //  When a larger or smaller resolution is used, it scales the image from this resolution to the target resolution.
        public static int DEFAULT_WIDTH = 1024;
        public static int DEFAULT_HEIGHT = 768;

        //The XNA GraphicsDeviceManager used by the game. Generated at Initialize(...).
        public static GraphicsDeviceManager graphics;
        //The XNA spritebatch used by the engine. Again, generated at Initialize(...).
        public static SpriteBatch spriteBatch;
        //The default spriteFont to use. Set to 'Default' by default.
        public static SpriteFont spriteFont;

        //All sprite images to be used with the engine. Automatically loads from Content/Graphics. Can be manually added to.
        public static Dictionary<string, Texture2D> textures;
        //All sprites to display. Calling any of the CreateSprite functions automatically adds them. You can also manually create and add a sprite.
        public static List<GObject> gObjects;
        //When it's set to loading, instead render this list of objects and ignore the normal list.
        public static List<GObject> loadingGObjects;

        //You generally assume that (0,0) is the top left of the screen. In cases where this isn't always practical (think
        //  scrolling levels) you can instead move the 'camera' so it renders from a different top left corner.
        public static float cameraOffsetX = 0;
        public static float cameraOffsetY = 0;

        //For smooth camera scrolling. It's easier to just use ScrollToOffset(...) instead of setting it manually,
        //  but if you want to set the parameters manually, you're free to do so.
        public static float scrollDestinationX = 0;
        public static float scrollDestinationY = 0;
        public static float scrollTimeLeft = 0f; //Time left in seconds for scrolling to reach target. Will determine scrolling speed using this.

        //Let's say you're playing a 4:3 game in a 16:9 resolution or vice versa. Should the screen retain its aspect ratio or stretch to fit the resolution?
        public static bool stretch = false;

        //This engine has a log function. The log is displayed in the bottom right corner.
        //  You can enable this during development for easy debug output, then just turn it off before release, and any calls to Log(...) will be ignored.
        public static bool logEnabled = true;
        //By enabling logToFile, it will also write log output to a file.
        public static bool logToFile = false;
        public static StreamWriter streamWriter; //The StreamWriter used for writing to the file.

        //When set to true to display the 'loadingGObjects' list instead of the regular 'gOjects' list. Use ShowLoadScreen() to set to true.
        public static bool loading = false;
        //If you need to make sure the loading screen is displayed before doing something, you can use this variable to check.
        //  When the loadingGObjects are displayed, this is set to true. Please note that it may also be true if loading hasn't started yet.
        public static bool printedLoadingFrame = false;

        //Set this to true to show the framerate in the top left corner.
        public static bool showFPS = false;

        //By default, GTexts are unaffected by the camera position. Enable this to also make GTexts camera-dependent.
        public static bool unlockGTextPositions = false;

        //If enabled, it will only render GObjects with (X, Y) on the screen or within bleed range.
        public static bool drawOnlyOnScreen = false;
        public static Vector2 drawOnlyOnScreenBleed = Vector2.Zero;
        
        //Spritebatch options
        public static SpriteSortMode spriteSortMode = SpriteSortMode.Deferred;
        public static BlendState blendState = BlendState.AlphaBlend;
        public static SamplerState samplerState = SamplerState.LinearClamp;
        public static DepthStencilState depthStencilState = DepthStencilState.None;
        public static RasterizerState rasterizerState = RasterizerState.CullCounterClockwise;

        //Internal variables
        private static string[] logText;
        private static Game game;
        private static GraphicsDevice GraphicsDevice;
        private static ContentManager Content;

        //Must be called before you try to create any sprites or call the Draw function.
        public static void Initialize(Game game, int width = 1024, int height = 768)
        {
            SpriteEngine.game = game;
            Content = game.Content;
            graphics = new GraphicsDeviceManager(game);

            DEFAULT_WIDTH = width;
            DEFAULT_HEIGHT = height;
            ChangeResolution(width, height);

            gObjects = new List<GObject>();
            loadingGObjects = new List<GObject>();

            logText = new string[10];
            for (int i = 0; i < logText.Length; i++) logText[i] = "";

            if (logToFile)
            {
                try
                {
                    streamWriter = new StreamWriter("log.txt");
                }
                catch (Exception) { /* This is not important enough to crash a game for. */ }
            }
        }

        //Does exactly what it says on the tin.
        public static Sprite CreateSprite(string texture, int x, int y, float scale)
        {
            Sprite sprite = new Sprite(textures[texture], texture);
            sprite.scale = scale;
            sprite.SetPosition(x, y);
            sprite.name = texture;
            gObjects.Add(sprite);
            return sprite;
        }

        //Identical to above, but takes a Vector2 instead of two coordinates.
        public static Sprite CreateSprite(string texture, Vector2 position, float scale)
        {
            return CreateSprite(texture, Convert.ToInt32(position.X), Convert.ToInt32(position.Y), scale);
        }

        //Same thing again, but automatically at (0,0).
        public static Sprite CreateSprite(string texture, float scale)
        {
            return CreateSprite(texture, Vector2.Zero, scale);
        }

        //Same thing, automatically at (0,0) with scale 1.0.
        public static Sprite CreateSprite(string texture)
        {
            return CreateSprite(texture, Vector2.Zero, 1.0f);
        }

        //Sorry, I have no idea what this does. At all.
        public static Sprite CreateSprite(string texture, Vector2 position)
        {
            return CreateSprite(texture, position, 1.0f);
        }

        //This one doesn't use the texture dictionary.
        public static Sprite CreateSprite(Texture2D tex, int x = 0, int y = 0)
        {
            Sprite sprite = new Sprite(tex, new Vector2(x, y));
            gObjects.Add(sprite);
            return sprite;
        }

        //Creates a sprite with multiple images. Can be used for animation, different states, etc.
        public static Sprite CreateMultiSprite(string[] texture, int x, int y, float scale)
        {
            List<Texture2D> texes = new List<Texture2D>();
            for (int i = 0; i < texture.Length; i++)
            {
                texes.Add(textures[texture[i]]);
            }
            Sprite sprite = new Sprite(texes, texture);
            sprite.scale = scale;
            sprite.SetPosition(x, y);
            sprite.name = texture[0];
            gObjects.Add(sprite);
            return sprite;
        }

        //Creates a LayeredSprite. LayeredSprites are sprites composed of multiple sprites.
        //  For example, a base body sprite combined with an armor sprite and a weapon sprite.
        //  The names given are to identify the different layers if you want to modify them later (for example, replace the character's weapon).
        //  Note that this asks for SPRITES, not IMAGES. As such, each layer can also have multiple images (see CreateMultiSprite).
        //  Each sprite has to be created in advance before creating the LayeredSprite to contain them.
        public static LayeredSprite CreateLayeredSprite(Sprite[] sprites, string[] names = null, int x = 0, int y = 0)
        {
            LayeredSprite lspr = new LayeredSprite();

            for (int i = 0; i < sprites.Length; i++)
            {
                if (names == null)
                {
                    lspr.AddLayer(sprites[i].name, sprites[i]);
                }
                else lspr.AddLayer(names[i], sprites[i]);
            }
            lspr.SetPosition(x, y);
            gObjects.Add(lspr);

            return lspr;
        }

        //Creates a GText. A GText is pretty much just text that can be used in ways similar to a sprite (including depth, for example).
        public static GText CreateGText(string text, int x, int y)
        {
            GText gt = new GText(text, x, y);
            gt.scale = 1.0f;
            gt.depth = -100.0f;
            gt.name = text;
            gObjects.Add(gt);
            return gt;
        }

        //Use this to indicate the game is loading. It will stop rendering the normal sprites, and instead use the loadingGObjects list.
        public static void ShowLoadScreen()
        {
            loading = true;
            printedLoadingFrame = false;
        }

        //Use this to indicate the game has finished loading. It will return to displaying normal sprites.
        public static void HideLoadScreen()
        {
            loading = false;
        }

        //Here's where the magic happens. Call this in your draw routine. It automatically starts and ends the spriteBatch.
        public static void Draw(GameTime gameTime)
        {
            DrawGObjects(float.MinValue, float.MaxValue);
            DrawLog();
            DrawFPS();
        }

        //Use this if you want to manually draw GObjects without the log or FPS counter;
        //  It also allows you to only draw GObjects between a minimum/maximum depth.
        public static void DrawGObjects(float minimumDepth, float maximumDepth)
        {
            spriteBatch.Begin(spriteSortMode, blendState, samplerState, depthStencilState, rasterizerState);

            List<GObject> renderableObjects = new List<GObject>();
            if (loading) renderableObjects = loadingGObjects;
            else renderableObjects = gObjects;

            //Draw sprites
            renderableObjects.Sort();

            //--
            for (int i = 0; i < renderableObjects.Count; i++)
            {
                if (renderableObjects[i] != null)
                {
                    bool allowDraw = true;

                    if (drawOnlyOnScreen)
                    {
                        allowDraw = false;

                        if (renderableObjects[i] is Sprite)
                        {
                            if (((Sprite)renderableObjects[i]).positionLocked) allowDraw = true;
                        }

                        if (!allowDraw)
                        {
                            if (renderableObjects[i].position.X >= cameraOffsetX - drawOnlyOnScreenBleed.X)
                            {
                                if (renderableObjects[i].position.X <= cameraOffsetX + DEFAULT_WIDTH + drawOnlyOnScreenBleed.X)
                                {
                                    if (renderableObjects[i].position.Y >= cameraOffsetY - drawOnlyOnScreenBleed.Y)
                                    {
                                        if (renderableObjects[i].position.Y <= cameraOffsetY + DEFAULT_WIDTH + drawOnlyOnScreenBleed.Y)
                                        {
                                            allowDraw = true;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (allowDraw)
                    {
                        if (renderableObjects[i].depth < minimumDepth) i = renderableObjects.Count;
                        else
                        {
                            if (renderableObjects[i].depth < maximumDepth)
                            {
                                if (renderableObjects[i].enabled)
                                {
                                    (renderableObjects[i]).Draw(spriteBatch);
                                }
                            }
                        }
                    }
                }
            }

            if (loading) printedLoadingFrame = true;
            else printedLoadingFrame = false;

            spriteBatch.End();
        }

        //It, uh, draws the log. Is automatically called by Draw(...), but you can do it manually if you want.
        public static void DrawLog()
        {
            if (logEnabled)
            {
                spriteBatch.Begin();
                for (int i = 0; i < logText.Length; i++)
                {
                    try
                    {
                        spriteBatch.DrawString(spriteFont, logText[i], new Vector2(0, GraphicsDevice.Viewport.Height - (i + 1) * 20), Color.White);
                    }
                    catch (Exception)
                    {
                        spriteBatch.DrawString(spriteFont, "ERROR: COULD NOT LOG MESSAGE", new Vector2(0, GraphicsDevice.Viewport.Height - (i + 1) * 20), Color.White);
                    }
                }
                spriteBatch.End();
            }
        }

        //Should not need any explanation.
        public static void DrawFPS()
        {
            //Show FPS
            if (showFPS)
            {
                _total_frames++;
                ShowFPS();
            }
        }

        //---

        //Let's say you just really hate GTexts. Use this to draw text the usual XNA way, but with automatic scaling to the proper resolution.
        public static void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color, float rotation, float scale, SpriteEffects spriteEffects)
        {
            spriteBatch.DrawString(spriteFont, text, new Vector2(position.X * GetDefaultScaleX(), position.Y * GetDefaultScaleY()),
                color, rotation, Vector2.Zero, scale * GetDefaultScaleNoStretch(), spriteEffects, 0);
        }

        //---

        //You have to call this from your game's Update function for camera scrolling and animations to work.
        public static void Update(GameTime gameTime)
        {
            List<GObject> renderableObjects = new List<GObject>();
            if (loading) renderableObjects = loadingGObjects;
            else renderableObjects = gObjects;

            for (int i = 0; i < renderableObjects.Count; i++)
            {
                if (renderableObjects[i] != null)
                {
                    if (renderableObjects[i].enabled)
                    {
                        renderableObjects[i].Update(gameTime);
                    }
                }
            }

            if (scrollTimeLeft > 0)
            {
                float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
                float perc = elapsedSeconds / scrollTimeLeft;
                if (perc > 1) perc = 1;

                if (perc > 0)
                {
                    cameraOffsetX += ((float)scrollDestinationX - (float)cameraOffsetX) * perc;
                    cameraOffsetY += ((float)scrollDestinationY - (float)cameraOffsetY) * perc;
                }
                scrollTimeLeft -= elapsedSeconds;
            }

            if (showFPS) CalculateFPS(gameTime);
        }

        //---
        
        //If, for some reason, you need to find out how much the screen is scaled to fit the resolution, use this function for the Y value.
        public static float GetDefaultScaleY()
        {
            if (!stretch) return GetDefaultScaleNoStretch();
            return (float)graphics.PreferredBackBufferHeight / (float)DEFAULT_HEIGHT;
        }

        //Same as above, but for the X value.
        public static float GetDefaultScaleX()
        {
            if (!stretch) return GetDefaultScaleNoStretch();
            return (float)graphics.PreferredBackBufferWidth / (float)DEFAULT_WIDTH;
        }

        //Use this to get the lowest of the X and Y values.
        public static float GetDefaultScaleNoStretch()
        {
            float xScale = (float)graphics.PreferredBackBufferWidth / (float)DEFAULT_WIDTH;
            float yScale = (float)graphics.PreferredBackBufferHeight / (float)DEFAULT_HEIGHT;
            if (yScale < xScale) return yScale;
            else return xScale;
        }

        //Prints a line to the log displayed in the bottom left, if logEnabled is true.
        public static void Log(string message)
        {
            if (logEnabled)
            {
                for (int i = 1; i < logText.Length; i++)
                {
                    logText[i - 1] = logText[i];
                }
                try
                {
                    logText[logText.Length - 1] = message;
                }
                catch (Exception) { logText[logText.Length - 1] = "ERROR: COULD NOT LOG MESSAGE"; }
                if (logToFile)
                {
                    try
                    {
                        streamWriter.WriteLine("[" + DateTime.Now.ToLongTimeString() + "] " + message);
                    }
                    catch (Exception) { /* Not important enough to crash a game for. */ }
                }
            }
        }

        //Changes the resolution.
        public static void ChangeResolution(int width, int height)
        {
            graphics.PreferredBackBufferWidth = width;
            graphics.PreferredBackBufferHeight = height;
            graphics.ApplyChanges();
        }

        //Switch from full screen to windowed or vice versa.
        public static void ToggleFullscreen()
        {
            if (graphics.IsFullScreen) SetFullScreen(false);
            else SetFullScreen(true);
        }

        //Set the game to windowed if false, or to full screen if true.
        public static void SetFullScreen(bool toSet)
        {
            graphics.IsFullScreen = toSet;
            graphics.ApplyChanges();
        }

        //Remove a GObject from the (main) GObject list and destroy it.
        public static void EraseGObject(GObject gObject)
        {

            gObjects.Remove(gObject);

            if (gObject != null)
            {
                gObject.Free();
            }
        }

        //Destroys all display gobjects and resets camera position
        public static void Nuke()
        {
            for (int i = 0; i < gObjects.Count; i++)
            {
                gObjects[i].Free();
                gObjects[i] = null;
            }
            gObjects = new List<GObject>();

            cameraOffsetX = 0;
            cameraOffsetY = 0;
            scrollDestinationX = 0;
            scrollDestinationY = 0;
            scrollTimeLeft = 0;
        }

        //You generally assume that (0,0) is the top left of the screen. In cases where this isn't always practical (think
        //  scrolling levels) you can instead move the 'camera' so it renders from a different top left corner.
        //  Use this function to make the 'camera' scroll smoothly to a target location.
        //  Note: To move there instantly, set 'scrollDestinationX' and 'scrollDestinationY' manually.
        //  If timeInSeconds is 0, the camera won't scroll.
        public static void ScrollToOffset(float destinationX, float destinationY, float timeInSeconds)
        {
            scrollDestinationX = destinationX;
            scrollDestinationY = destinationY;
            scrollTimeLeft = timeInSeconds;
        }

        //FPS counter
        private static int _total_frames = 0;
        private static float _elapsed_time = 0.0f;
        private static int _fps = 0;

        private static void CalculateFPS(GameTime gameTime)
        {
            _elapsed_time += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // 1 Second has passed
            if (_elapsed_time >= 1000.0f)
            {
                _fps = _total_frames;
                _total_frames = 0;
                _elapsed_time = 0;
            }
        }

        private static void ShowFPS(Color color)
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(spriteFont, Convert.ToString(_fps), Vector2.Zero, color);
            spriteBatch.End();
        }

        private static void ShowFPS() { ShowFPS(Color.White); }
    }
}

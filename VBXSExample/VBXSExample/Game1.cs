using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using VBXSE;

/*
 * VDZ's Basic XNA Sprite Engine - A sprite engine for XNA 4.0 (version 2)

    Written in 2012 by Vincent de Zwaan

    To the extent possible under law, the author(s) have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty. 

    You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>. 
  
 * Any modifications I make to this software in the future will likely be published at <https://github.com/VDZx/VBXSE>.
 */

namespace VBXSExample
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        //Obligatory code
        public Game1()
        {
            Content.RootDirectory = "Content";
            SpriteEngine.Initialize(this, 1024, 768); //Initialize the sprite engine. Note that this happens in the CONSTRUCTOR, not Initialize().
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            SpriteEngine.LoadContent(); //Load everything from Content/Graphics and spriteFont 'Default'
            StartGame();
        }

        protected override void Update(GameTime gameTime)
        {
            SpriteEngine.Update(gameTime);
            DoActualUpdateStuff(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue); //The SpriteEngine will not clear the screen for you, so you have to do it yourself.
            SpriteEngine.Draw(gameTime);
            base.Draw(gameTime);
        }

        //Actual code
        LayeredSprite player = null;
        GText instructionText = null;
        KeyboardState state = new KeyboardState();
        KeyboardState oldState = new KeyboardState();

        public void StartGame()
        {
            //-----------------Creating a normal sprite----------------
            //Create the background
            Sprite backgroundSprite = SpriteEngine.CreateSprite("background");
            backgroundSprite.depth = 800;

            //---------------Creating a sprite with position--------------
            //Create a building at (200, 200)
            Sprite buildingSprite = SpriteEngine.CreateSprite("building", 200, 50, 1f);
            buildingSprite.depth = -(50 + buildingSprite.GetImage().Height); //Set its depth equal to the negative value of the Y of the bottom of the sprite.
            //(Note that the engine supports transparency, mspaint just doesn't.)

            //---------------Creating a multi-layer sprite---------------
            //Create layers to build the player with
            Sprite heads = new Sprite(
                new Texture2D[] { SpriteEngine.textures["head1"], SpriteEngine.textures["head2"], SpriteEngine.textures["head3"] },
                new string[] { "head1", "head2", "head3" });
            Sprite bodies = new Sprite(
                new Texture2D[] { SpriteEngine.textures["body1"], SpriteEngine.textures["body2"], SpriteEngine.textures["body3"] },
                new string[] { "body1", "body2", "body3" });
            Sprite legs = new Sprite(
                 new Texture2D[] { SpriteEngine.textures["legs1"], SpriteEngine.textures["legs2"], SpriteEngine.textures["legs3"] },
                 new string[] { "legs1", "legs2", "legs3" });
            //Create the player at (500, 650)
            player = SpriteEngine.CreateLayeredSprite(new Sprite[] { heads, bodies, legs }, new string[] { "head", "body", "legs" }, 500, 650);

            //--------------Creating an animated sprite---------------
            //Create an animated sign at (700, 400)
            Sprite sign = SpriteEngine.CreateMultiSprite(new string[] { "Anim1", "Anim2", "Anim3" }, 700, 300, 1f);
            sign.depth = -(300 + sign.GetImage().Height); //Set depth
            Animation anim = new Animation("text", "Anim1", 3, 100);
            sign.AddAnimation(anim.name, anim);
            sign.ChangeAnimation("text");

            //-------------Creating a GText-------
            instructionText = SpriteEngine.CreateGText("Arrows to move around, Q to change head, W to change body, E to change legs, Enter to fake loading screen.", 300, 250);
            instructionText.depth = -250; //It will be behind the building

            //-----------Setting a load screen----
            //Note: Loading support is very basic and it kinda sucks. Build your own for fancy loading.
            Sprite loadSprite = new Sprite(SpriteEngine.textures["loading"], Vector2.Zero);
            SpriteEngine.loadingGObjects.Add(loadSprite);
        }

        public void DoActualUpdateStuff(GameTime gameTime)
        {
            state = Keyboard.GetState();

            if (!SpriteEngine.loading)
            {
                float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
                //Move the GText
                if (instructionText != null)
                {
                    instructionText.SetX(instructionText.position.X - 200f * seconds);
                    if (instructionText.position.X < -1200) instructionText.SetX(1024);
                }

                //Move character
                float xMovement = 0;
                float yMovement = 0;
                float moveSpeed = 100;
                if (state.IsKeyDown(Keys.Up)) yMovement = -moveSpeed;
                if (state.IsKeyDown(Keys.Down)) yMovement = +moveSpeed;
                if (state.IsKeyDown(Keys.Left)) xMovement = -moveSpeed;
                if (state.IsKeyDown(Keys.Right)) xMovement = +moveSpeed;
                player.SetPosition(player.position.X + xMovement * seconds, player.position.Y + yMovement * seconds);
                //Set player depth
                player.depth = -(player.position.Y + player.GetImage().Height);

                //--------Switching between frames and using layers--------
                //Change head/body/legs at request
                if (JustPushed(Keys.Q))
                {
                    int targetFrame = player.GetLayer("head").sprite.currentImage + 1;
                    if (targetFrame > 2) targetFrame = 0;
                    player.GetLayer("head").sprite.ChangeFrame(targetFrame);
                }
                if (JustPushed(Keys.W))
                {
                    int targetFrame = player.GetLayer("body").sprite.currentImage + 1;
                    if (targetFrame > 2) targetFrame = 0;
                    player.GetLayer("body").sprite.ChangeFrame(targetFrame);
                }
                if (JustPushed(Keys.E))
                {
                    int targetFrame = player.GetLayer("legs").sprite.currentImage + 1;
                    if (targetFrame > 2) targetFrame = 0;
                    player.GetLayer("legs").sprite.ChangeFrame(targetFrame);
                }

                //----------Using a load screen---------
                if (JustPushed(Keys.Enter))
                {
                    SpriteEngine.loading = true;
                    System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(delegate
                        {
                            System.Threading.Thread.Sleep(5000);
                            SpriteEngine.loading = false;
                        }));
                }
            }

            oldState = state;
        }

        public bool JustPushed(Keys key)
        {
            if (state.IsKeyDown(key) && !oldState.IsKeyDown(key)) return true;
            else return false;
        }
    }
}

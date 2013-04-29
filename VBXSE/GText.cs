using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

/*
 * VDZ's Basic XNA Sprite Engine - A sprite engine for XNA 4.0 (version 2)

    Written in 2012 by Vincent de Zwaan

    To the extent possible under law, the author(s) have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty. 

    You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>. 
  
 * Any modifications I make to this software in the future will likely be published at <https://github.com/VDZx/VBXSE>.
 */

namespace VBXSE
{
    public class GText : GObject
    {
        public string text; //The text to display.
        public Color color = Color.White; //The color of the text.
        public bool positionLocked = true; //Set to false to make text move with camera.

        public GText(string text)
        {
            Initialize();
            this.text = text;

        }

        public GText(string text, int x, int y)
        {
            Initialize();
            this.text = text;
            this.SetPosition(x, y);
        }

        private void Initialize()
        {
            this.SetPosition(0, 0);
            this.scale = 1.0f;
        }

        //Draws the text.
        public override void Draw(SpriteBatch spriteBatch)
        {
            float usedCameraX = 0;
            float usedCameraY = 0;

            if (SpriteEngine.unlockGTextPositions || !positionLocked)
            {
                usedCameraX = -SpriteEngine.cameraOffsetX;
                usedCameraY = -SpriteEngine.cameraOffsetY;
            }

            spriteBatch.DrawString(SpriteEngine.spriteFont, text,
                new Vector2((position.X + usedCameraX) * SpriteEngine.GetDefaultScaleX(),
                    (position.Y + usedCameraY) * SpriteEngine.GetDefaultScaleY())
                , this.color, 0f, Vector2.Zero, this.scale * SpriteEngine.GetDefaultScaleNoStretch(), SpriteEffects.None, 0f);
        }

        public override void Update(GameTime gameTime)
        {
            //Do nothing
        }

        public override void Free()
        {
            //Nothing to free here
        }

        public override void SetX(float x)
        {
            this.position.X = x;
        }

        public override void SetY(float y)
        {
            this.position.Y = y;
        }
    }

}
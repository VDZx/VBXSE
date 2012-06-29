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

/*
 * VDZ's Basic XNA Sprite Engine - A sprite engine for XNA 4.0 (version 2)

    Written in 2012 by Vincent de Zwaan

    To the extent possible under law, the author(s) have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty. 

    You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>. 
  
 * Any modifications I make to this software in the future will likely be published at <https://github.com/VDZx/VBXSE>.
 */

namespace VBXSE
{
    //GObject - Base class for displayable objects such as Sprites and GObjects.
    public abstract class GObject : IComparable<GObject>
    {
        public Vector2 position = Vector2.Zero; //Position.
        public float depth = 0.0f; //Determines priority of rendering: Higher depth value means it's rendered behind other objects.
        public float scale = 1.0f; //Resizes the sprite using the anchorPoint (default (0,0), always (0,0) on GTexts) as center.
        public bool enabled = true; //When set to false, the object is not rendered.
        public string name = "unnamed gobject"; //Not used internally, but can be useful to identify sprites.

        abstract public void Draw(SpriteBatch spriteBatch); //Derivative classes specify how to be drawn here.
        abstract public void Update(GameTime gameTime); //Derivative classes specify what to do when time passes here.
        abstract public void Free(); //Clears the GObject. The texture itself remains in memory, but the GObject is fully erased.

        public void SetPosition(float x, float y)
        {
            SetX(x);
            SetY(y);
        }

        public void SetPosition(Vector2 position)
        {
            SetPosition(position.X, position.Y);
        }

        public abstract void SetX(float x);
        public abstract void SetY(float y);

        //Compares depth of objects. Returns 1 if less deep than comparison object, -1 if deeper, or 0 if equally deep.
        public int CompareTo(GObject obj)
        {
            if (obj is GObject)
            {
                GObject gobj = (GObject)obj;
                if (gobj.depth == depth) return 0;
                else if (gobj.depth > depth) return 1;
                else return -1;
            }
            else return -1;
        }
    }
}
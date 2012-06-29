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
    //Animation - Defines a sprite animation.
    public struct Animation
    {
        public Animation(string name, string startingFrame, int numberOfFrames, float msPerFrame)
        {
            this.name = name; //Can be used to identify the animation.
            this.startingFrame = startingFrame; //The image name of the sprite where the animation starts. All frames must be identified in the frames Dictionary.
            this.numberOfFrames = numberOfFrames; //The number of frames in the animation
            this.msPerFrame = msPerFrame; //How long each frame plays, in milliseconds
            this.currentAnimPos = 0;
        }

        public string name;
        public string startingFrame;
        public int numberOfFrames;
        public float msPerFrame;
        public float currentAnimPos; //Curent time position in animation
    }

    //Sprite - Contains information and methods for rendering a graphical object.
    public class Sprite : GObject
    {
        public List<Texture2D> images; //The different textures in the image
        public int currentImage = 0; //Defines which of the textures should be displayed
        public bool positionLocked = false; //Set to true to keep sprite on the same screen position even when the camera moves
        public float rotation = 0f; //Rotates the sprite in radians (PI being 180 degrees, 2 PI being a full rotation)
        //It's possible to change how it interprets its location and allow it to be drawn somewhere else than just (X,Y).
        //  The X and Y values are multiplied by the positionScale to determine the position to draw the sprite.
        //  For example, a sprite with X and Y (15, 30) with positionScale = 2 will be rendered at (30, 60) instead.
        public float positionScale = 1f;
        public Vector2 anchorPoint; //For rotation and display, which point in the sprite is considered the (X,Y). Default is top left (0,0).
        public Color color = Color.White; //You can use XNA color trickery to make your sprite look different with this value.
        public string[] names = new string[] { "unnamed" }; //The names of the various images in the sprite (not to be confused with the name of the sprite itself).
        public Dictionary<string, Animation> animations = new Dictionary<string, Animation>(); //A list of the animations the sprite has.
        public string currentAnimation = ""; //The animation to play. If set to "", don't play any animation.
        public Dictionary<string, int> frames; //You can add name-frame# pairs here to quickly refer to specific images in the sprite. Required for animations.
        public SpriteEffects isFlipped = SpriteEffects.None;

        //Constructors should need no explanation, other than that you should use SpriteEngine.CreateSprite functions instead,
        //  unless you're trying to perform some kind of trickery (for example, creating a LayeredSprite requires manual creation).
        //  In contrast to SpriteEngine.CreateSprite, this will NOT add them to the GObject list and they will not be rendered by default!
        public Sprite(Texture2D tex, string name, string[] eff = null)
        {
            images = new List<Texture2D>();
            images.Add(tex);
            currentImage = 0;
            anchorPoint = Vector2.Zero;
            names = new string[] { name };
        }

        public Sprite(Texture2D tex, Vector2 position)
            : this(tex, "unnamed")
        {
            this.position = position;
        }

        public Sprite(List<Texture2D> tex)
        {
            images = tex;
            anchorPoint = Vector2.Zero;
        }

        public Sprite(List<Texture2D> tex, string[] names)
        {
            cSpr(tex, names);
        }

        private void cSpr(List<Texture2D> tex, string[] names)
        {
            images = tex;
            frames = new Dictionary<string, int>();
            this.names = names;
            for (int i = 0; i < names.Length; i++)
            {
                frames.Add(names[i], i);
            }
            anchorPoint = Vector2.Zero;
        }

        public Sprite(Texture2D[] tex, string[] names)
        {
            List<Texture2D> list = new List<Texture2D>();
            for (int i = 0; i < tex.Length; i++) list.Add(tex[i]);
            cSpr(list, names);
        }

        public Sprite() { anchorPoint = Vector2.Zero; }

        private void Initialize()
        {
            this.SetPosition(0, 0);
        }

        //Returns the currently displayed texture.
        public virtual Texture2D GetImage()
        {
            return images[currentImage];
        }

        //Automatically called by SpriteEngine.Draw if it's in the list and enabled; draws the sprite.
        public override void Draw(SpriteBatch spriteBatch)
        {
            float xoffset = 0; if (!positionLocked) xoffset = -SpriteEngine.cameraOffsetX;
            float yoffset = 0; if (!positionLocked) yoffset = -SpriteEngine.cameraOffsetY;
            Texture2D image = GetImage();

            if (rotation != 0)
            {
                int xToRender = (int)(Math.Round(((float)(position.X + xoffset) * SpriteEngine.GetDefaultScaleX()) + (image.Width / positionScale) / 2) * positionScale);
                int yToRender = (int)(Math.Round(((float)(position.Y + yoffset) * SpriteEngine.GetDefaultScaleY()) + (image.Height / positionScale) / 2) * positionScale);

                spriteBatch.Draw(image, new Rectangle(xToRender, yToRender,
                    Convert.ToInt32(image.Width * scale * SpriteEngine.GetDefaultScaleX()),
                    Convert.ToInt32(image.Height * scale * SpriteEngine.GetDefaultScaleY())),
                    null, color, rotation,
                    new Vector2((image.Width * scale * SpriteEngine.GetDefaultScaleX()) / 2, (image.Height * scale * SpriteEngine.GetDefaultScaleY()) / 2),
                    isFlipped, 0);
            }
            else
            {
                int xToRender = (int)(Math.Round(((float)(position.X + xoffset) * SpriteEngine.GetDefaultScaleX())) * positionScale);
                int yToRender = (int)(Math.Round(((float)(position.Y + yoffset) * SpriteEngine.GetDefaultScaleY())) * positionScale);

                spriteBatch.Draw(image, new Rectangle(xToRender, yToRender,
                    Convert.ToInt32(image.Width * scale * SpriteEngine.GetDefaultScaleX()),
                    Convert.ToInt32(image.Height * scale * SpriteEngine.GetDefaultScaleY())),
                    null, color, 0f, anchorPoint, isFlipped, 0);
            }
        }

        //Called by SpriteEngine.Update if it's in the list and enabled; handles the animation.
        public override void Update(GameTime gameTime)
        {
            if (this.currentAnimation != "")
            {
                if (animations.ContainsKey(currentAnimation))
                {
                    if (frames != null)
                    {
                        Animation anim = animations[currentAnimation];
                        anim.currentAnimPos += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                        while (anim.currentAnimPos > anim.numberOfFrames * anim.msPerFrame) anim.currentAnimPos -= anim.numberOfFrames * anim.msPerFrame;
                        int originalFramePos = -1;
                        foreach (KeyValuePair<string, int> frame in frames)
                        {
                            if (frame.Key == anim.startingFrame) originalFramePos = frame.Value;
                        }
                        if (originalFramePos > -1)
                            this.ChangeFrame(originalFramePos + Convert.ToInt32(Math.Floor(anim.currentAnimPos / anim.msPerFrame)));

                        animations[currentAnimation] = anim;
                    }
                }
            }
        }

        //Changes the currently displayed image to the image with that number.
        public virtual void ChangeFrame(int number)
        {
            this.currentImage = number;
        }

        //Changes the currently displayed image to the image with that name in the frames Dictionary, if possible.
        public virtual void ChangeFrame(string name)
        {
            try
            {
                if (frames != null)
                    ChangeFrame(frames[name]);
            }
            catch (KeyNotFoundException)
            {
                SpriteEngine.Log("ERROR: Could not find frame " + name + "!");
            }
        }

        //Adds an animation to the animation list.
        public virtual void AddAnimation(string name, Animation animation)
        {
            this.animations.Add(name, animation);
        }

        //Changes the current animation. Set to "" to display a still sprite.
        public virtual void ChangeAnimation(string animation)
        {
            this.currentAnimation = animation;
        }

        //Clears the references to the textures it has in its images list.
        public override void Free()
        {
            if (images != null)
            {
                for (int i = 0; i < images.Count; i++)
                {
                    images[i] = null;
                }
            }
            images = null;
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
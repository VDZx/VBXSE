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
    //LayeredSprite - A sprite composed of multiple overlaid sprites. For example, a base body plus armor and a weapon.
    //Please note that this was kind of hacked in and might be a bit clunky.
    public class LayeredSprite : Sprite
    {
        //Layer - Defines a layer (for example, 'armor') with a reference to the current sprite for that layer and its X and Y relative to the LayeredSprite.
        public struct Layer
        {
            public Layer(string name, Sprite sprite) { this.name = name; this.sprite = sprite; depth = 0; offset_x = 0; offset_y = 0; }
            public string name;
            public Sprite sprite;
            public int depth; //Not actually used right now, the layers are rendered in the order they're added. I'm lazy!
            public int offset_x;
            public int offset_y;
        }

        public List<Layer> layers;

        public LayeredSprite()
        {
            Initialize();
        }

        public void Initialize()
        {
            layers = new List<Layer>();
        }

        //Different functions to add layers from texture alone, from sprite, or from completely defined layer.
        //  All of the AddLayer functions will replace any already existing layer with the same name.
        //  So if you, for example, assign a club sprite to the layer 'weapon', the sword that was previously the 'weapon' vanishes.
        public void AddLayer(string name, string texture)
        {
            Sprite spr = new Sprite(SpriteEngine.textures[texture], name);
            spr.position = this.position;
            AddLayer(new Layer(name, spr));
        }

        public void AddLayer(string name, Sprite sprite)
        {
            AddLayer(new Layer(name, sprite));
        }

        public void AddLayer(Layer layer)
        {
            int existing = -1;
            for (int i = 0; i < layers.Count; i++)
            {
                if (layers[i].name == layer.name) { existing = i; }
            }
            if (existing == -1)
            {
                if (layers.Count > 0)
                    layer.depth = layers[layers.Count - 1].depth - 1;
                layers.Add(layer);
            }
            else { layers[existing] = layer; }
        }

        //Removes the layer, if present.
        public void RemoveLayer(Layer layer)
        {
            RemoveLayer(layer.name);
        }

        //Same but identified by name.
        public void RemoveLayer(string name)
        {
            int existing = -1;
            for (int i = 0; i < layers.Count; i++)
            {
                if (layers[i].name == name) existing = i;
            }
            if (existing != -1) layers.RemoveAt(existing);
        }

        //Gets the layer with the given name, or an empty layer if the layer isn't found.
        public Layer GetLayer(string name)
        {
            int existing = -1;
            for (int i = 0; i < layers.Count; i++)
            {
                if (layers[i].name == name) existing = i;
            }
            if (existing != -1) return layers[existing];
            else return new Layer();
        }

        //Clunkiness in action. Returns only the image of the very first layer.
        public override Texture2D GetImage()
        {
            return layers[0].sprite.GetImage();
        }

        //Draws the thing, obviously.
        public override void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].sprite.Draw(spriteBatch);
            }
        }

        //LayeredSprites can also have animations. Also automatically called through SpriteEngine.Update.
        public override void Update(GameTime gameTime)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].sprite.Update(gameTime);
            }
        }

        public override void SetX(float x)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].sprite.SetX(x + layers[i].offset_x);
            }
            base.SetX(x);
        }

        public override void SetY(float y)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].sprite.SetY(y + layers[i].offset_y);
            }
            base.SetY(y);
        }

        //Changes the frame for every single layer. Make sure they match up!
        public override void ChangeFrame(int number)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].sprite.ChangeFrame(number);
            }
            base.ChangeFrame(number);
        }

        public override void ChangeFrame(string name)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                try
                {
                    if (layers[i].sprite.frames != null)
                        layers[i].sprite.ChangeFrame(layers[i].sprite.frames[name]);
                }
                catch (KeyNotFoundException) { SpriteEngine.Log("ERROR: Could not find frame " + name + "!"); }
            }
            base.ChangeFrame(name);
        }

        public override void AddAnimation(string name, Animation animation)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].sprite.AddAnimation(name, animation);
            }
        }

        public override void ChangeAnimation(string animation)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].sprite.ChangeAnimation(animation);
            }
        }
    }

}
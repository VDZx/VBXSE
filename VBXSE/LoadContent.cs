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
 * VDZ's Basic XNA Sprite Engine - A sprite engine for XNA 4.0 (version 2)

    Written in 2012 by Vincent de Zwaan

    To the extent possible under law, the author(s) have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty. 

    You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>. 
  
 * Any modifications I make to this software in the future will likely be published at <https://github.com/VDZx/VBXSE>.
 */

namespace VBXSE
{
    public static partial class SpriteEngine
    {

        //Call this in your LoadContent routine. Does some intialization, imports the graphics in Content/Graphics, and loads the font.
        public static void LoadContent()
        {
            GraphicsDevice = game.GraphicsDevice;

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>("Default");
            textures = new Dictionary<string, Texture2D>();

            DirectoryInfo di = new DirectoryInfo(game.Content.RootDirectory + "\\Graphics");
            FileInfo[] fi = di.GetFiles("*", SearchOption.AllDirectories);

            List<string> loadableAssets = new List<string>();
            foreach (FileInfo f in fi)
            {
                string fullname = f.FullName;
                string cutname = fullname.Substring(f.FullName.LastIndexOf("\\Content\\") + 9);
                loadableAssets.Add(cutname.Replace("\\", "/")); //Replace \ with /
            }

            for (int i = 0; i < loadableAssets.Count; i++)
            {
                string name = loadableAssets[i].Substring(0, loadableAssets[i].LastIndexOf("."));
                if (name.Contains("/")) name = name.Substring(name.LastIndexOf("/") + 1);
                if (loadableAssets[i].EndsWith(".xnb")) //Texture
                {
                    Texture2D tex = Content.Load<Texture2D>(loadableAssets[i].Substring(0, loadableAssets[i].LastIndexOf(".")));
                    tex.Name = name;
                    textures.Add(name, tex);
                }
            }
        }
    }

}
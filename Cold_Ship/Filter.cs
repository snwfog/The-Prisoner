﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cold_Ship
{
    public class Filter : GenericSprite2D
    {
        public float filterScale = 1;

        public Filter(Texture2D texture, Vector2 position)
            : base(texture, position, Rectangle.Empty)
        {
        }

        //draws the shadow filter onto the screen, the size of the filter
        //is changed according to the parameter
        public override void Draw(SpriteBatch spriteBatch, Vector2 drawPosition)
        {
            if (filterScale < 1)
            {
                base.Draw(spriteBatch, drawPosition);
            }
            drawPosition = new Vector2((drawPosition.X) + ((texture.Width - texture.Width * filterScale) / 2),
            (drawPosition.Y + ((texture.Height - texture.Height * filterScale) / 2)));
            spriteBatch.Draw(texture, drawPosition, new Rectangle(0, 0, texture.Width, texture.Height), Color.White, 0f, new Vector2(0, 0), filterScale, SpriteEffects.None, 0);
        }
    }
}

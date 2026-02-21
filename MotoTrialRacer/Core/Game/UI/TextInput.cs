/**
 * Copyright (c) 2011-2014 Microsoft Mobile and/or its subsidiary(-ies).
 * All rights reserved.
 *
 * For the applicable distribution terms see the license text file included in
 * the distribution.
 */

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace MotoTrialRacer.UI
{
    /// <summary>
    /// Thread-safe character buffer filled by Game1 via Window.TextInput.
    /// TextInput UI component drains it each Update() when active.
    /// </summary>
    public static class TextInputBuffer
    {
        private static readonly Queue<char> _buffer = new Queue<char>();

        public static void PushChar(char c)
        {
            lock (_buffer) { _buffer.Enqueue(c); }
        }

        public static bool HasChars
        {
            get { lock (_buffer) { return _buffer.Count > 0; } }
        }

        public static char PopChar()
        {
            lock (_buffer) { return _buffer.Dequeue(); }
        }
    }

    /// <summary>
    /// A single-line text input component. When tapped/clicked the field becomes
    /// active and accepts keyboard input via the Window.TextInput event (hooked in
    /// Game1 constructor).  Backspace removes the last character.
    /// Works on Desktop (keyboard) and Mobile (virtual keyboard fires Window.TextInput).
    /// </summary>
    class TextInput : IDrawable
    {
        public String Text { get; set; }
        public String HintText { get; private set; }

        private SpriteFont font;
        private Color hintColor = new Color(1.0f, 1.0f, 0.0f, 0.0f);
        private Vector2 textPos;
        private Texture2D texture;
        private Rectangle destination;
        private int width;
        private int height;
        private bool isActive = false;

        /// <summary>
        /// Creates a new text input UI component.
        /// </summary>
        public TextInput(String hint, Vector2 pos, ContentManager contentManager,
                         int pWidth, int pHeight)
        {
            HintText = hint;
            Text = HintText;
            width = pWidth;
            height = pHeight;
            texture = new Texture2D(MotoTrialRacerGame.Graphics.GraphicsDevice, width, height);
            Color[] colors = new Color[width * height];
            for (int i = 0; i < colors.Length; i++)
            {
                if (i < 2 * width || colors.Length - 2 * width < i ||
                    i % width < 2 || width - 3 < i % width)
                    colors[i] = new Color(1.0f, 1.0f, 0.0f, 1.0f);
                else
                    colors[i] = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            }
            destination = new Rectangle((int)pos.X, (int)pos.Y, width, height);
            texture.SetData<Color>(colors);
            font = contentManager.Load<SpriteFont>("SpriteFont1");
            textPos = new Vector2(pos.X + width * 0.1f,
                                  pos.Y + height * 0.5f - font.MeasureString(HintText).Y * 0.5f);
        }

        /// <summary>
        /// Activates the field when touched/clicked, and drains the TextInputBuffer
        /// when active to update the displayed text.
        /// </summary>
        public void Update(TouchLocation touchLocation)
        {
            // Activate on tap / click
            if (destination.Contains(new Point((int)touchLocation.Position.X,
                                               (int)touchLocation.Position.Y)))
            {
                if (touchLocation.State == TouchLocationState.Pressed)
                {
                    isActive = true;
                    if (Text == HintText)
                        Text = "";
                }
            }

            // Consume keyboard characters while active
            if (isActive)
            {
                while (TextInputBuffer.HasChars)
                {
                    char c = TextInputBuffer.PopChar();
                    if (c == '\b') // Backspace
                    {
                        if (Text.Length > 0)
                            Text = Text.Substring(0, Text.Length - 1);
                    }
                    else if (c == '\r' || c == '\n') // Enter – deactivate
                    {
                        isActive = false;
                        if (Text.Length == 0)
                            Text = HintText;
                    }
                    else if (!char.IsControl(c))
                    {
                        Text += c;
                    }
                }
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, destination, Color.White);
            spriteBatch.DrawString(font, Text, textPos, hintColor);
        }
    }
}

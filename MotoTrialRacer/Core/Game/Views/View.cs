/**
 * Copyright (c) 2011-2014 Microsoft Mobile and/or its subsidiary(-ies).
 * All rights reserved.
 *
 * For the applicable distribution terms see the license text file included in
 * the distribution.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using MotoTrialRacer.UI;

namespace MotoTrialRacer
{
    /// <summary>
    /// The base class for all the views that are shown on top the game world.
    /// Implements the ButtonListener interface
    /// </summary>
    class View : IDrawable
    {
        protected MotoTrialRacerGame game;
        protected TouchLocation touchLocation;
        protected bool touchChanged = false;
        private Texture2D background;
        private Rectangle destination;

        // Keyboard / gamepad navigation state
        private Button _focusedButton;

        /// <summary>
        /// Override to return the ordered list of buttons reachable by Up/Down/Enter.
        /// Return null (default) to opt out of keyboard navigation entirely.
        /// The list may change each frame (e.g. LevelSelector tabs); the navigation
        /// system detects the change and resets focus automatically.
        /// </summary>
        protected virtual List<Button> GetNavigableButtons() => null;

        /// <summary>
        /// Sets keyboard focus to the first item in GetNavigableButtons().
        /// Call at the end of a subclass constructor to pre-highlight the first option.
        /// </summary>
        protected void FocusFirst()
        {
            var list = GetNavigableButtons();
            if (list == null || list.Count == 0) return;
            _focusedButton = list[0];
            _focusedButton.IsFocused = true;
        }

        /// <summary>
        /// Creates a new view
        /// </summary>
        /// <param name="pGame">The Game instance that will show this view</param>
        public View(MotoTrialRacerGame pGame)
        {
            game = pGame;
            destination = new Rectangle(0, 0, pGame.getWidth(), pGame.getHeight() );
            background = new Texture2D(game.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            Color[] colors = new Color[background.Width * background.Height];
            for (int i = 0; i < colors.Length; i++)
                colors[i] = new Color(0.0f, 0.0f, 0.0f, 0.4f);
            background.SetData<Color>(colors);
        }

        /// <summary>
        /// Updates the state and location of the touch.
        /// Uses InputManager.GetTouches() so that mouse clicks are
        /// synthesised into touch events on Desktop.
        /// </summary>
        public virtual void Update(InputManager input)
        {
            var touchCollection = input.GetTouches();

            if (touchCollection.Count > 0)
            {
                touchLocation = touchCollection[0];
                touchChanged = true;
            }
            else if (touchChanged)
                touchChanged = false;

            HandleNavigation(input);
        }

        /// <summary>
        /// Handles Up/Down/Confirm navigation for views that expose a button list
        /// via GetNavigableButtons(). Mouse/touch activity clears keyboard focus.
        /// </summary>
        private void HandleNavigation(InputManager input)
        {
            var navList = GetNavigableButtons();
            if (navList == null || navList.Count == 0)
            {
                // Navigable list gone (e.g. view changed state) – clear any stale focus.
                if (_focusedButton != null)
                {
                    _focusedButton.IsFocused = false;
                    _focusedButton = null;
                }
                return;
            }

            // If the previously focused button is no longer in the current list
            // (e.g. LevelSelector switched tabs), clear the stale focus.
            if (_focusedButton != null && !navList.Contains(_focusedButton))
            {
                _focusedButton.IsFocused = false;
                _focusedButton = null;
            }

            // Mouse / touch interaction clears keyboard focus.
            if (touchChanged && _focusedButton != null)
            {
                _focusedButton.IsFocused = false;
                _focusedButton = null;
            }

            bool goDown    = input.IsMenuDownJustPressed;
            bool goUp      = input.IsMenuUpJustPressed;
            bool doConfirm = input.IsMenuConfirmJustPressed;

            if (goDown || goUp)
            {
                int current = _focusedButton != null ? navList.IndexOf(_focusedButton) : -1;
                if (_focusedButton != null) _focusedButton.IsFocused = false;

                int next;
                if (goDown)
                    next = (current + 1) % navList.Count;    // -1 → 0 on first Down
                else
                    next = current == -1
                        ? navList.Count - 1                  // first Up → last item
                        : (current - 1 + navList.Count) % navList.Count;

                _focusedButton = navList[next];
                _focusedButton.IsFocused = true;
            }
            else if (doConfirm && _focusedButton != null)
            {
                _focusedButton.Press();
            }
        }

		public virtual void Draw(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(background, destination, Color.White);   
		}
	}
}

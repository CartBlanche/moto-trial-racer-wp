/**
 * Copyright (c) 2011-2014 Microsoft Mobile and/or its subsidiary(-ies).
 * All rights reserved.
 *
 * For the applicable distribution terms see the license text file included in
 * the distribution.
 */

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace MotoTrialRacer
{
    /// <summary>
    /// Centralised input manager (inspired by ShipGame's InputManager pattern).
    /// Reads Keyboard, GamePad and Mouse/Touch once per frame and exposes both
    /// raw state and high-level game-action properties.
    ///
    /// On mobile the real TouchPanel is used for UI hit-testing.
    /// On Desktop the left mouse button is synthesised into a TouchLocation so
    /// that every Button.Update(TouchLocation) call works without any changes
    /// to Button.cs, and without relying on the broken
    /// TouchPanel.EnableMouseTouchPoint.
    ///
    /// Gamepad mapping (single player, Player One):
    ///   Throttle forward  – Right trigger
    ///   Throttle back     – Left trigger
    ///   Lean left         – Left-stick X (negative) or D-Pad Left
    ///   Lean right        – Left-stick X (positive) or D-Pad Right
    ///   Back / pause      – Back button (edge-triggered)
    /// </summary>
    public class InputManager
    {
        private static readonly bool IsMobile =
            OperatingSystem.IsAndroid() || OperatingSystem.IsIOS();

        private KeyboardState _currentKeys;
        private KeyboardState _lastKeys;
        private GamePadState  _currentPad;
        private GamePadState  _lastPad;
        private MouseState    _currentMouse;
        private MouseState    _lastMouse;

        public InputManager()
        {
            _currentKeys  = Keyboard.GetState();
            _currentPad   = GamePad.GetState(PlayerIndex.One);
            _currentMouse = Mouse.GetState();
            _lastKeys     = _currentKeys;
            _lastPad      = _currentPad;
            _lastMouse    = _currentMouse;
        }

        // ------------------------------------------------------------------ //
        //  Frame management                                                   //
        // ------------------------------------------------------------------ //

        /// <summary>Call once at the very start of Game.Update() to snapshot input.</summary>
        public void BeginFrame()
        {
            _lastKeys  = _currentKeys;
            _lastPad   = _currentPad;
            _lastMouse = _currentMouse;

            _currentKeys = Keyboard.GetState();
            _currentPad  = GamePad.GetState(PlayerIndex.One);
            if (!IsMobile)
                _currentMouse = Mouse.GetState();
        }

        // ------------------------------------------------------------------ //
        //  Bike / gameplay actions                                            //
        // ------------------------------------------------------------------ //

        /// <summary>Throttle forward: keyboard Up OR gamepad right trigger.</summary>
        public bool IsThrottleForward =>
            _currentKeys.IsKeyDown(Keys.Up) ||
            _currentPad.Triggers.Right > 0.2f;

        /// <summary>Throttle back / brake: keyboard Down OR gamepad left trigger.</summary>
        public bool IsThrottleBack =>
            _currentKeys.IsKeyDown(Keys.Down) ||
            _currentPad.Triggers.Left > 0.2f;

        /// <summary>Neither throttle key/axis is active.</summary>
        public bool IsThrottleIdle =>
            !IsThrottleForward && !IsThrottleBack;

        /// <summary>
        /// Lean backwards (counter-clockwise): keyboard Left OR
        /// gamepad left-stick pushed left OR D-Pad Left.
        /// </summary>
        public bool IsLeanLeft =>
            _currentKeys.IsKeyDown(Keys.Left)      ||
            _currentPad.ThumbSticks.Left.X < -0.4f ||
            _currentPad.DPad.Left == ButtonState.Pressed;

        /// <summary>
        /// Lean forwards (clockwise): keyboard Right OR
        /// gamepad left-stick pushed right OR D-Pad Right.
        /// </summary>
        public bool IsLeanRight =>
            _currentKeys.IsKeyDown(Keys.Right)     ||
            _currentPad.ThumbSticks.Left.X > 0.4f  ||
            _currentPad.DPad.Right == ButtonState.Pressed;

        /// <summary>
        /// True when a gamepad is physically connected and recognised by MonoGame.
        /// When true on mobile the accelerometer codepath is bypassed so the gamepad
        /// drives the bike exclusively.
        /// </summary>
        public bool IsGamepadActive => _currentPad.IsConnected;

        /// <summary>
        /// Back / pause: gamepad Back button only (edge-triggered).
        /// On Android/iOS this is the hardware back button.
        /// </summary>
        public bool IsGamepadBackJustPressed =>
            _currentPad.Buttons.Back == ButtonState.Pressed &&
            _lastPad.Buttons.Back    == ButtonState.Released;

        /// <summary>
        /// Keyboard Escape key, edge-triggered.
        /// Context-sensitive: pauses during gameplay, resumes from pause menu,
        /// navigates back from level selector / editor, exits from main menu.
        /// </summary>
        public bool IsEscapeJustPressed =>
            _currentKeys.IsKeyDown(Keys.Escape) &&
            _lastKeys.IsKeyUp(Keys.Escape);

        /// <summary>
        /// Either back input fired this frame – convenience for places that treat them equally.
        /// </summary>
        public bool IsBackJustPressed => IsGamepadBackJustPressed || IsEscapeJustPressed;

        // ------------------------------------------------------------------ //
        //  Menu navigation (all edge-triggered)                               //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Move selection down: keyboard Down OR D-Pad Down OR left-stick pushed down.
        /// Note: keyboard Down also maps to IsThrottleBack, but during menus the bike
        /// motor is disabled so the overlap is harmless.
        /// </summary>
        public bool IsMenuDownJustPressed =>
            (_currentKeys.IsKeyDown(Keys.Down) && _lastKeys.IsKeyUp(Keys.Down)) ||
            (_currentPad.DPad.Down == ButtonState.Pressed &&
             _lastPad.DPad.Down    == ButtonState.Released) ||
            (_currentPad.ThumbSticks.Left.Y < -0.5f &&
             _lastPad.ThumbSticks.Left.Y    >= -0.5f);

        /// <summary>
        /// Move selection up: keyboard Up OR D-Pad Up OR left-stick pushed up.
        /// </summary>
        public bool IsMenuUpJustPressed =>
            (_currentKeys.IsKeyDown(Keys.Up) && _lastKeys.IsKeyUp(Keys.Up)) ||
            (_currentPad.DPad.Up == ButtonState.Pressed &&
             _lastPad.DPad.Up    == ButtonState.Released) ||
            (_currentPad.ThumbSticks.Left.Y > 0.5f &&
             _lastPad.ThumbSticks.Left.Y   <= 0.5f);

        /// <summary>
        /// Confirm/activate the focused item: keyboard Enter OR gamepad A button.
        /// </summary>
        public bool IsMenuConfirmJustPressed =>
            (_currentKeys.IsKeyDown(Keys.Enter) && _lastKeys.IsKeyUp(Keys.Enter)) ||
            (_currentPad.Buttons.A == ButtonState.Pressed &&
             _lastPad.Buttons.A    == ButtonState.Released);

        // ------------------------------------------------------------------ //
        //  Touch / UI                                                         //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Returns a TouchCollection suitable for feeding into Button.Update().
        ///
        /// On mobile  – returns the real TouchPanel state.
        /// On Desktop – synthesises a single-touch point from the left mouse
        ///              button, mapping press/hold/release to the corresponding
        ///              TouchLocationState values.
        /// </summary>
        public TouchCollection GetTouches()
        {
            if (IsMobile)
                return TouchPanel.GetState();

            bool wasDown = _lastMouse.LeftButton    == ButtonState.Pressed;
            bool isDown  = _currentMouse.LeftButton == ButtonState.Pressed;

            // Mouse not involved at all → return an empty collection
            if (!wasDown && !isDown)
                return new TouchCollection(Array.Empty<TouchLocation>());

            TouchLocationState state;
            if (!wasDown && isDown)
                state = TouchLocationState.Pressed;
            else if (wasDown && isDown)
                state = TouchLocationState.Moved;
            else // wasDown && !isDown
                state = TouchLocationState.Released;

            var pos = new Vector2(_currentMouse.X, _currentMouse.Y);
            return new TouchCollection(new[] { new TouchLocation(0, state, pos) });
        }
    }
}

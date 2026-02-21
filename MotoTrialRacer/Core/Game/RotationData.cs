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

namespace MotoTrialRacer
{
    /// <summary>
    /// Provides rotation/tilt data for bike physics control.
    ///
    /// Desktop:  device = false  → Bike.cs uses keyboard (Up/Down/Left/Right).
    /// Mobile:   device = true   → set xRot from the platform accelerometer in the
    ///                             platform-specific project (Android / iOS) by overriding
    ///                             EnableRotation() there.
    /// </summary>
    public class RotationData
    {
        private static readonly object threadLock = new object();

        /// <summary>
        /// True when running on a real mobile device with an accelerometer.
        /// Detected at runtime via OperatingSystem helpers so the Core library
        /// stays platform-neutral.
        /// </summary>
        public bool device { get; protected set; } =
            OperatingSystem.IsAndroid() || OperatingSystem.IsIOS();

        /// <summary>
        /// Motor speed value driven by the accelerometer on mobile.
        /// On desktop this is written directly by Bike.cs keyboard handling.
        /// </summary>
        public float xRot;

        /// <summary>
        /// Called once when a new level starts.  Override in platform projects to
        /// start the accelerometer listener.
        /// </summary>
        public virtual void EnableRotation()
        {
            // No-op on Desktop.  Mobile platforms override this method.
        }
    }
}

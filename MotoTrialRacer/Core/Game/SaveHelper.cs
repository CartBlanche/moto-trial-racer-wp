/**
 * Copyright (c) 2011-2014 Microsoft Mobile and/or its subsidiary(-ies).
 * All rights reserved.
 *
 * For the applicable distribution terms see the license text file included in
 * the distribution.
 */

using System;
using System.IO;

namespace MotoTrialRacer
{
    /// <summary>
    /// Helper providing the per-user save directory for high scores and custom levels.
    /// On Desktop: %AppData%\MotoTrialRacer
    /// On Android / iOS: the app's persistent data directory (same path resolves correctly)
    /// </summary>
    internal static class SaveHelper
    {
        private static string _saveDirectory;

        public static string GetSaveDirectory()
        {
            if (_saveDirectory == null)
            {
                _saveDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "MotoTrialRacer");
                Directory.CreateDirectory(_saveDirectory);
            }
            return _saveDirectory;
        }
    }
}

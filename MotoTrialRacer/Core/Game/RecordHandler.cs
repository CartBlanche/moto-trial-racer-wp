/**
 * Copyright (c) 2011-2014 Microsoft Mobile and/or its subsidiary(-ies).
 * All rights reserved.
 *
 * For the applicable distribution terms see the license text file included in
 * the distribution.
 */

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace MotoTrialRacer
{
    /// <summary>
    /// The structure of the high score records.
    /// </summary>
    struct Record
    {
        public string Name;
        public int Time;
    }

    /// <summary>
    /// Handles all high scores. Persists to the user save directory.
    /// </summary>
    class RecordHandler
    {
        public List<Record> Records { get; private set; }

        private string fileName;
        private const char dataSeparator = ':';

        /// <summary>
        /// Creates a new record handler.
        /// </summary>
        /// <param name="levelName">The name of the level whose records this manages.</param>
        public RecordHandler(String levelName)
        {
            fileName = levelName + ".scr";
            Records = new List<Record>();
        }

        /// <summary>
        /// Loads the records from disk.  Creates default records if none exist.
        /// </summary>
        public void LoadRecords()
        {
            Load();
            if (Records.Count == 0)
            {
                CreateRecords();
                SaveRecords();
            }
        }

        /// <summary>
        /// Returns the leaderboard placement (1-based) for the given time, or -1 if not ranked.
        /// </summary>
        public int GetPlacement(int time)
        {
            if (time < Records[0].Time)
                return 1;

            for (int i = 0; i < Records.Count - 1; i++)
            {
                if (Records[i].Time < time && time < Records[i + 1].Time)
                    return i + 2;
            }
            return -1;
        }

        /// <summary>
        /// Saves the records to disk.
        /// </summary>
        public void SaveRecords()
        {
            string filePath = Path.Combine(SaveHelper.GetSaveDirectory(), fileName);
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (Record r in Records)
                    writer.WriteLine(r.Name + dataSeparator + r.Time);
            }
        }

        /// <summary>
        /// Inserts a new record at the given placement.
        /// </summary>
        public void SetRecord(int placement, string name, int time)
        {
            for (int i = Records.Count - 1; i > placement - 1; i--)
                Records[i] = Records[i - 1];

            Records[placement - 1] = new Record { Name = name, Time = time };
        }

        private void Load()
        {
            string filePath = Path.Combine(SaveHelper.GetSaveDirectory(), fileName);
            if (File.Exists(filePath))
            {
                using (StreamReader r = new StreamReader(filePath))
                {
                    string text = r.ReadToEnd();
                    string[] rows = text.Split(new string[] { Environment.NewLine },
                                               StringSplitOptions.RemoveEmptyEntries);
                    foreach (string row in rows)
                    {
                        string[] pieces = row.Split(dataSeparator);
                        Records.Add(new Record
                        {
                            Name = pieces[0],
                            Time = Convert.ToInt32(pieces[1])
                        });
                    }
                }
            }
        }

        private void CreateRecords()
        {
            for (int i = 0; i < 10; i++)
                Records.Add(new Record() { Name = "Racer" + (i + 1), Time = 60000 });
        }
    }
}

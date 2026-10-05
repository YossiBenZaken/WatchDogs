using System;
using System.Collections.Generic;
using WatchDogsMod.Models;

namespace WatchDogsMod.Data
{
    /// <summary>
    /// אחראי על יצירה ושמירה של פרופילים: לכל NPC נוצר פרופיל פעם אחת ונשאר קבוע.
    /// </summary>
    public class ProfileRepository
    {
        private static readonly string[] FirstNames = { "Daniel", "Maya", "Noah", "Lena", "Omar", "Sofia", "Eli", "Yara", "Liam", "Tamar", "Jake", "Nina" };
        private static readonly string[] LastNames  = { "Cohen", "Reyes", "Miller", "Haddad", "Novak", "Silva", "Brooks", "Levi", "Kim", "Moreno" };
        private static readonly string[] Jobs       = { "Barista", "Accountant", "Student", "Taxi Driver", "Nurse", "Programmer", "Waiter", "Plumber", "Teacher", "Mechanic", "Tourist", "Lawyer" };

        private readonly Random rnd = new Random();
        private readonly Dictionary<int, Profile> profiles = new Dictionary<int, Profile>();

        public Profile Get(int handle)
        {
            Profile p;
            if (!profiles.TryGetValue(handle, out p))
            {
                p = new Profile
                {
                    Name = FirstNames[rnd.Next(FirstNames.Length)] + " " + LastNames[rnd.Next(LastNames.Length)],
                    Job = Jobs[rnd.Next(Jobs.Length)],
                    Cash = rnd.Next(Config.MinCash, Config.MaxCash + 1)
                };
                profiles[handle] = p;
            }
            return p;
        }
    }
}
using System;
using System.Collections.Generic;
using GTA;
using GTA.Math;
using GTA.Native;
using WatchDogsMod.Hacking;

namespace WatchDogsMod.Reactions
{
    public enum ReactionType { Flee, CallPolice }

    /// <summary>
    /// [חדש] מחליט אם NPC שמאקקים עליו מבחין בנו, ומה הוא עושה.
    /// מאזין לאירועי HackController, ומדווח דרך האירועים Noticed / PoliceCalled.
    /// </summary>
    public class ReactionController
    {
        // שיחת משטרה שמתבצעת עכשיו: מתי היא מסתיימת
        private class PendingCall
        {
            public int Handle;
            public int FireAt;
        }

        private readonly Random rnd = new Random();
        private readonly List<PendingCall> pendingCalls = new List<PendingCall>();
        private int hackedHandle;   // מי מאוקק כרגע (0 = אף אחד)

        /// <summary>ה-NPC הבחין בנו. פרמטרים: ה-NPC והתגובה שנבחרה.</summary>
        public event Action<int, ReactionType> Noticed;

        /// <summary>ה-NPC סיים להתקשר, והמשטרה הוזעקה.</summary>
        public event Action<int> PoliceCalled;

        public ReactionController(HackController hack)
        {
            hack.Started   += h => hackedHandle = h;
            hack.Cancelled += () => hackedHandle = 0;
            hack.Completed += h => hackedHandle = 0;   // גניבה שהצליחה = ה-NPC לא הבחין
        }

        /// <summary>קוראים בכל Tick.</summary>
        public void Update(Ped player)
        {
            UpdatePendingCalls();

            if (hackedHandle != 0)
                CheckNotice(player);
        }

        // כל פריים מטילים "קובייה": הסיכוי עולה ככל שקרובים יותר
        private void CheckNotice(Ped player)
        {
            int handle = hackedHandle;
            if (!Function.Call<bool>(Hash.DOES_ENTITY_EXIST, handle))
            {
                hackedHandle = 0;
                return;
            }

            Vector3 pedPos = Function.Call<Vector3>(Hash.GET_ENTITY_COORDS, handle, true);
            float dist = player.Position.DistanceTo(pedPos);

            // 1 = צמוד, 0 = בקצה הטווח. רחוק = 20% מהסיכוי, צמוד = 100%
            float closeness = 1f - Math.Min(dist / Config.HackRange, 1f);
            float ratePerSec = Config.NoticeRatePerSec * (0.2f + 0.8f * closeness);

            float dt = Function.Call<float>(Hash.GET_FRAME_TIME);
            if (rnd.NextDouble() < ratePerSec * dt)
                Notice(player, handle);
        }

        private void Notice(Ped player, int handle)
        {
            hackedHandle = 0;   // לפני האירוע, כדי שלא נטיל קובייה שוב

            ReactionType type = rnd.NextDouble() < Config.PoliceCallChance
                ? ReactionType.CallPolice
                : ReactionType.Flee;

            if (type == ReactionType.Flee)
            {
                NpcActions.Flee(handle, player.Handle);
            }
            else
            {
                NpcActions.StartPhoneCall(handle);
                pendingCalls.Add(new PendingCall { Handle = handle, FireAt = Game.GameTime + Config.PoliceCallDelayMs });
            }

            if (Noticed != null) Noticed(handle, type);
        }

        // בודקים אם שיחות המשטרה הסתיימו. אם ה-NPC עדיין חי - מופיע כוכב והוא בורח
        private void UpdatePendingCalls()
        {
            for (int i = pendingCalls.Count - 1; i >= 0; i--)
            {
                PendingCall call = pendingCalls[i];
                if (Game.GameTime < call.FireAt) continue;

                pendingCalls.RemoveAt(i);

                bool alive = Function.Call<bool>(Hash.DOES_ENTITY_EXIST, call.Handle)
                          && !Function.Call<bool>(Hash.IS_ENTITY_DEAD, call.Handle, false);
                if (!alive) continue;   // הרגת אותו לפני שסיים - אין כוכב

                NpcActions.RaiseWantedLevel();
                NpcActions.Flee(call.Handle, Game.Player.Character.Handle);

                if (PoliceCalled != null) PoliceCalled(call.Handle);
            }
        }
    }
}
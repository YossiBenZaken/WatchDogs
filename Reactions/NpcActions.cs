using GTA;
using GTA.Native;

namespace WatchDogsMod.Reactions
{
    /// <summary>
    /// [חדש] הפעולות עצמן שנעשות על NPC או על השחקן (natives בלבד).
    /// לא מחליט מתי - רק איך. ההחלטה ב-ReactionController.
    /// </summary>
    public static class NpcActions
    {
        /// <summary>ה-NPC בורח מהשחקן.</summary>
        public static void Flee(int pedHandle, int fromHandle)
        {
            Function.Call(Hash.TASK_SMART_FLEE_PED, pedHandle, fromHandle, Config.FleeDistance, -1, false, false);
            Function.Call(Hash.SET_PED_KEEP_TASK, pedHandle, true);   // שימשיך לברוח ולא "ישכח"
        }

        /// <summary>ה-NPC שולף טלפון ומתקשר (לפרק זמן מוגדר).</summary>
        public static void StartPhoneCall(int pedHandle)
        {
            Function.Call(Hash.TASK_USE_MOBILE_PHONE_TIMED, pedHandle, Config.PoliceCallDelayMs);
        }

        /// <summary>מעלים כוכב מבוקש (רק אם אין כרגע).</summary>
        public static void RaiseWantedLevel()
        {
            int playerId = Game.Player.Handle;
            if (Function.Call<int>(Hash.GET_PLAYER_WANTED_LEVEL, playerId) >= 1) return;

            Function.Call(Hash.SET_PLAYER_WANTED_LEVEL, playerId, 1, false);
            Function.Call(Hash.SET_PLAYER_WANTED_LEVEL_NOW, playerId, false);
        }
    }
}
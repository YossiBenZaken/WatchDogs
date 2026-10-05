using System.Windows.Forms;

namespace WatchDogsMod
{
    /// <summary>
    /// כל ההגדרות של המוד במקום אחד.
    /// </summary>
    public static class Config
    {
        // ---- בקרה ----
        public const Keys StealKey = Keys.E;

        // ---- האקינג ----
        public const float HackRange = 30.0f;       // טווח גניבה מרחוק (מטרים)
        public const float AimTolerance = 0.985f;   // כמה צריך לכוון על ה-NPC (1.0 = בול)
        public const float HackTime = 1.2f;         // שניות החזקה עד שהגניבה מסתיימת
        public const int ScanIntervalMs = 200;      // כל כמה זמן סורקים NPCs

        // ---- כסף ----
        public const int MinCash = 20;
        public const int MaxCash = 400;

        // ---- תגובות NPC ----
        public const float NoticeRatePerSec = 0.25f;   // סיכוי בסיסי (לשנייה) שה-NPC מבחין. לבדיקה: 3.0
        public const float PoliceCallChance = 0.5f;    // כשמבחין: סיכוי שיתקשר למשטרה (אחרת בורח)
        public const int PoliceCallDelayMs = 4000;     // משך שיחת המשטרה עד שמופיע כוכב מבוקש
        public const float FleeDistance = 100.0f;      // כמה רחוק ה-NPC בורח
    }
}
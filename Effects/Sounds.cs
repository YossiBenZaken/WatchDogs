using GTA.Native;

namespace WatchDogsMod.Effects
{
    /// <summary>
    /// [חדש] צלילי ממשק מובנים של GTA. אפשר להחליף שם/סט צליל כאן בלי לגעת בשאר הקוד.
    /// </summary>
    public static class Sounds
    {
        public static void HackStart()    { Play("NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET"); }
        public static void HackCancel()   { Play("CANCEL", "HUD_FRONTEND_DEFAULT_SOUNDSET"); }
        public static void HackComplete() { Play("ROBBERY_MONEY_TOTAL", "HUD_FRONTEND_CUSTOM_SOUNDSET"); }

        private static void Play(string name, string soundSet)
        {
            Function.Call(Hash.PLAY_SOUND_FRONTEND, -1, name, soundSet, true);
        }
    }
}
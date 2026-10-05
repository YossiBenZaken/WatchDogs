using GTA;
using WatchDogsMod.Hacking;

namespace WatchDogsMod.Effects
{
    /// <summary>
    /// [חדש] מאזין לאירועי ההאקינג ומפעיל אנימציה וצלילים.
    /// כך HackController לא צריך לדעת כלום על אפקטים.
    /// </summary>
    public class EffectsPlayer
    {
        private readonly PhoneAnimation phone = new PhoneAnimation();

        public EffectsPlayer(HackController hack)
        {
            hack.Started   += OnStarted;
            hack.Cancelled += OnCancelled;
            hack.Completed += OnCompleted;
        }

        public void Update()
        {
            phone.Preload();
        }

        /// <summary>מנקים הכל (קוראים כשהסקריפט נסגר, כדי שלא יישאר טלפון תקוע ביד).</summary>
        public void Dispose()
        {
            phone.Stop(Game.Player.Character);
        }

        private void OnStarted(int handle)
        {
            phone.Start(Game.Player.Character);
            Sounds.HackStart();
        }

        private void OnCancelled()
        {
            phone.Stop(Game.Player.Character);
            Sounds.HackCancel();
        }

        private void OnCompleted(int handle)
        {
            phone.Stop(Game.Player.Character);
            Sounds.HackComplete();
        }
    }
}
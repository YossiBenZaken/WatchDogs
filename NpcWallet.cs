using System;
using System.Windows.Forms;
using GTA;
using WatchDogsMod.Data;
using WatchDogsMod.Effects;
using WatchDogsMod.Hacking;
using WatchDogsMod.Models;
using WatchDogsMod.Reactions;
using WatchDogsMod.Targeting;
using WatchDogsMod.UI;

namespace WatchDogsMod
{
    /// <summary>
    /// נקודת הכניסה של המוד. מחבר בין החלקים ולא מכיל לוגיקה משלו:
    /// <list type="bullet">
    /// <item><description><see cref="NpcScanner"/> - מוצא NPCs</description></item>
    /// <item><description><see cref="TargetSelector"/> - בוחר מטרה לפי כיוון המצלמה</description></item>
    /// <item><description><see cref="HackController"/> - החזקת כפתור והתקדמות</description></item>
    /// <item><description><see cref="ProfileRepository"/> - נתוני ה-NPC</description></item>
    /// <item><description><see cref="ReactionController"/> - NPC מבחין, בורח או מתקשר למשטרה</description></item>
    /// <item><description><see cref="EffectsPlayer"/> - אנימציית טלפון וצלילים</description></item>
    /// <item><description><see cref="Hud"/> - ציור על המסך</description></item>
    /// </list>
    /// </summary>
    public class WatchDogsScript : Script
    {
        private readonly NpcScanner _scanner = new NpcScanner();
        private readonly ProfileRepository _profiles = new ProfileRepository();
        private readonly HackController _hack = new HackController();
        private readonly EffectsPlayer _effects;
        private readonly ReactionController _reactions;

        public WatchDogsScript()
        {
            _effects = new EffectsPlayer(_hack);
            _reactions = new ReactionController(_hack);

            _hack.Completed += OnHackCompleted;
            _reactions.Noticed += OnNoticed;
            _reactions.PoliceCalled += OnPoliceCalled;

            Tick += OnTick;
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
            Aborted += (sender, e) =>
            {
                _effects.Dispose();
            };
            Interval = 0;
        }

        private void OnTick(object sender, EventArgs e)
        {
            try
            {
                _effects.Update();

                Ped player = Game.Player.Character;
                _reactions.Update(player);   // גם כשלא מאקקים - שיחות משטרה ממשיכות

                if (!Game.Player.CanControlCharacter || player.IsDead || player.IsInVehicle())
                {
                    _scanner.Clear();
                    _hack.Reset();
                    return;
                }

                _scanner.Update(player);

                int target = TargetSelector.Pick(player, _scanner.Candidates);

                Hud.DrawLines(player, _scanner.Candidates, target);

                if (target != 0)
                {
                    Hud.DrawProfile(_profiles.Get(target), target, _hack.Progress);

                    if (!_hack.IsKeyHeld)
                    {
                        Hud.ShowPrompt("Hold ~INPUT_CONTEXT~ to hack");
                    }
                }

                _hack.Update(target);
            }
            catch (Exception ex)
            {
                GTA.UI.Screen.ShowSubtitle("WatchDogsMod error: " + ex.Message, 2000);
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            _hack.OnKeyDown(e);
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            _hack.OnKeyUp(e);
        }

        // ההאקינג הסתיים: לוקחים את הכסף מהפרופיל ומסירים את ה-NPC מהרשימה
        private void OnHackCompleted(int handle)
        {
            Profile profile = _profiles.Get(handle);

            Game.Player.Money += profile.Cash;
            _scanner.Exclude(handle);

            Hud.Notify("~g~Hacked " + profile.Name + " ~w~- stole ~g~$" + profile.Cash);
        }

        // ה-NPC הבחין: מפסיקים את ההאקינג, והוא כבר לא יעד
        private void OnNoticed(int handle, ReactionType type)
        {
            Profile profile = _profiles.Get(handle);

            _hack.Interrupt();
            _scanner.Exclude(handle);

            if (type == ReactionType.Flee)
            {
                Hud.Notify("~r~" + profile.Name + " ~w~noticed you and ran away!");
            }
            else
            {
                Hud.Notify("~r~" + profile.Name + " ~w~noticed you and is calling the police...");
            }
        }

        private void OnPoliceCalled(int handle)
        {
            Hud.Notify("~r~The police have been called!");
        }
    }
}
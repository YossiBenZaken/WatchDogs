using System;
using System.Windows.Forms;
using GTA.Native;

namespace WatchDogsMod.Hacking
{
    /// <summary>
    /// אחראי על מצב ההאקינג: החזקת כפתור, פס התקדמות, ואירועים.
    /// לא יודע כלום על כסף, אנימציות, צלילים או תגובות - רק מדווח דרך האירועים.
    /// </summary>
    public class HackController
    {
        private bool keyHeld;
        private bool needRelease;    // אחרי גניבה / הפרעה חייבים לשחרר את הכפתור לפני הבאה
        private bool isHacking;
        private int hackingHandle;

        /// <summary>התקדמות 0..1</summary>
        public float Progress { get; private set; }

        public bool IsKeyHeld { get { return keyHeld; } }

        /// <summary>ההאקינג התחיל (הפרמטר: ה-NPC).</summary>
        public event Action<int> Started;

        /// <summary>ההאקינג הופסק באמצע (שחרור כפתור, איבוד מטרה, נכנסנו לרכב, הופרע...).</summary>
        public event Action Cancelled;

        /// <summary>ההאקינג הושלם (הפרמטר: ה-NPC).</summary>
        public event Action<int> Completed;

        public void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Config.StealKey) keyHeld = true;
        }

        public void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Config.StealKey)
            {
                keyHeld = false;
                needRelease = false;
                Progress = 0f;
            }
        }

        public void Reset()
        {
            Progress = 0f;
            StopHacking();
        }

        /// <summary>
        /// [חדש] מפסיקים את ההאקינג מבחוץ (למשל ה-NPC הבחין בנו).
        /// חייבים לשחרר ולהחזיק את הכפתור שוב כדי להתחיל מחדש.
        /// </summary>
        public void Interrupt()
        {
            Progress = 0f;
            needRelease = true;
            StopHacking();
        }

        /// <summary>קוראים בכל Tick עם המטרה הנוכחית (0 = אין מטרה).</summary>
        public void Update(int targetHandle)
        {
            if (targetHandle == 0)
            {
                Progress = 0f;
                StopHacking();
                return;
            }

            if (hackingHandle != targetHandle)
            {
                hackingHandle = targetHandle;
                Progress = 0f;
                StopHacking();
            }

            if (keyHeld && !needRelease)
            {
                if (!isHacking)
                {
                    isHacking = true;
                    if (Started != null) Started(targetHandle);
                }

                Progress += Function.Call<float>(Hash.GET_FRAME_TIME) / Config.HackTime;

                if (Progress >= 1f)
                {
                    Progress = 0f;
                    needRelease = true;
                    isHacking = false;
                    if (Completed != null) Completed(targetHandle);
                }
            }
            else
            {
                Progress = 0f;
                StopHacking();
            }
        }

        private void StopHacking()
        {
            if (!isHacking) return;
            isHacking = false;
            if (Cancelled != null) Cancelled();
        }
    }
}
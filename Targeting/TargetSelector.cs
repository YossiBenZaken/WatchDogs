using System;
using System.Collections.Generic;
using GTA;
using GTA.Math;
using GTA.Native;

namespace WatchDogsMod.Targeting
{
    /// <summary>
    /// אחראי על בחירת המטרה מתוך הרשימה: ה-NPC שהמצלמה מכוונת אליו, עם קו ראייה.
    /// </summary>
    public static class TargetSelector
    {
        /// <returns>handle של המטרה, או 0 אם אין.</returns>
        public static int Pick(Ped player, IReadOnlyList<int> candidates)
        {
            Vector3 camPos = Function.Call<Vector3>(Hash.GET_GAMEPLAY_CAM_COORD);
            Vector3 camRot = Function.Call<Vector3>(Hash.GET_GAMEPLAY_CAM_ROT, 2);

            // הופכים זוויות (מעלות) לווקטור כיוון
            float rx = camRot.X * (float)Math.PI / 180f;
            float rz = camRot.Z * (float)Math.PI / 180f;
            Vector3 camDir = new Vector3(
                -(float)Math.Sin(rz) * (float)Math.Cos(rx),
                 (float)Math.Cos(rz) * (float)Math.Cos(rx),
                 (float)Math.Sin(rx));

            int best = 0;
            float bestDot = Config.AimTolerance;

            foreach (int handle in candidates)
            {
                if (!Function.Call<bool>(Hash.DOES_ENTITY_EXIST, handle)) continue;

                Vector3 pedPos = Function.Call<Vector3>(Hash.GET_ENTITY_COORDS, handle, true);
                Vector3 toPed = pedPos - camPos;
                float len = toPed.Length();
                if (len < 0.1f) continue;
                toPed = toPed / len;

                // dot קרוב ל-1 = ה-NPC במרכז המסך
                float dot = camDir.X * toPed.X + camDir.Y * toPed.Y + camDir.Z * toPed.Z;
                if (dot > bestDot)
                {
                    bestDot = dot;
                    best = handle;
                }
            }

            // צריך קו ראייה - לא גונבים דרך קירות
            if (best != 0 && !Function.Call<bool>(Hash.HAS_ENTITY_CLEAR_LOS_TO_ENTITY, player.Handle, best, 17))
                return 0;

            return best;
        }
    }
}
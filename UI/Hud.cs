using System;
using System.Collections.Generic;
using GTA;
using GTA.Math;
using GTA.Native;
using WatchDogsMod.Models;

namespace WatchDogsMod.UI
{
    /// <summary>
    /// כל מה שמצויר או מוצג על המסך: קווים, פרופיל, פס התקדמות, הודעות.
    /// לא מכיל לוגיקה של המשחק - רק מציג מה שנותנים לו.
    /// </summary>
    public static class Hud
    {
        /// <summary>קו בהיר למטרה, קווים עמומים לשאר.</summary>
        public static void DrawLines(Ped player, IReadOnlyList<int> candidates, int targetHandle)
        {
            Vector3 from = player.Position + new Vector3(0f, 0f, 0.3f);

            foreach (int handle in candidates)
            {
                if (!Function.Call<bool>(Hash.DOES_ENTITY_EXIST, handle)) continue;

                Vector3 to = Function.Call<Vector3>(Hash.GET_ENTITY_COORDS, handle, true) + new Vector3(0f, 0f, 0.3f);

                bool isTarget = (handle == targetHandle);
                int r = isTarget ? 0 : 160;
                int g = isTarget ? 255 : 160;
                int b = isTarget ? 80 : 160;
                int a = isTarget ? 230 : 50;

                Function.Call(Hash.DRAW_LINE, from.X, from.Y, from.Z, to.X, to.Y, to.Z, r, g, b, a);
            }
        }

        /// <summary>שם / מקצוע / סכום מעל ה-NPC, ופס התקדמות כש-progress גדול מ-0.</summary>
        public static void DrawProfile(Profile p, int handle, float progress)
        {
            Vector3 head = Function.Call<Vector3>(Hash.GET_ENTITY_COORDS, handle, true) + new Vector3(0f, 0f, 1.1f);

            var sx = new OutputArgument();
            var sy = new OutputArgument();
            if (!Function.Call<bool>(Hash.GET_SCREEN_COORD_FROM_WORLD_COORD, head.X, head.Y, head.Z, sx, sy))
                return;

            float x = sx.GetResult<float>();
            float y = sy.GetResult<float>();

            DrawText(p.Name,       x, y - 0.075f, 0.40f, 255, 255, 255, 255);
            DrawText(p.Job,        x, y - 0.050f, 0.30f, 200, 200, 200, 230);
            DrawText("$" + p.Cash, x, y - 0.030f, 0.38f, 80, 255, 120, 255);

            if (progress > 0f)
            {
                const float w = 0.08f, h = 0.008f;
                float by = y - 0.005f;
                Function.Call(Hash.DRAW_RECT, x, by, w, h, 0, 0, 0, 180);
                float fill = w * Math.Min(progress, 1f);
                Function.Call(Hash.DRAW_RECT, x - w / 2f + fill / 2f, by, fill, h, 80, 255, 120, 255);
            }
        }

        /// <summary>הודעת עזרה בפינה השמאלית העליונה (צריך לקרוא לה בכל פריים).</summary>
        public static void ShowPrompt(string text)
        {
            Function.Call(Hash.BEGIN_TEXT_COMMAND_DISPLAY_HELP, "STRING");
            Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, text);
            Function.Call(Hash.END_TEXT_COMMAND_DISPLAY_HELP, 0, false, true, -1);
        }

        /// <summary>הודעה קופצת (Ticker) בצד המסך.</summary>
        public static void Notify(string text)
        {
            GTA.UI.Notification.PostTicker(text, false);
        }

        private static void DrawText(string text, float x, float y, float scale, int r, int g, int b, int a)
        {
            Function.Call(Hash.SET_TEXT_FONT, 4);
            Function.Call(Hash.SET_TEXT_SCALE, 0.0f, scale);
            Function.Call(Hash.SET_TEXT_COLOUR, r, g, b, a);
            Function.Call(Hash.SET_TEXT_CENTRE, true);
            Function.Call(Hash.SET_TEXT_OUTLINE);
            Function.Call(Hash.BEGIN_TEXT_COMMAND_DISPLAY_TEXT, "STRING");
            Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, text);
            Function.Call(Hash.END_TEXT_COMMAND_DISPLAY_TEXT, x, y, 0);
        }
    }
}
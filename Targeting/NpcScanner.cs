using System.Collections.Generic;
using System.Runtime.InteropServices;
using GTA;
using GTA.Math;
using GTA.Native;

namespace WatchDogsMod.Targeting
{
    /// <summary>
    /// אחראי על מציאת NPCs קרובים שאפשר לגנוב מהם ושמירת הרשימה.
    /// משתמש ב-natives בלבד (פונקציות הזיכרון של SHVDN לא עובדות ב-GTA V Enhanced).
    /// </summary>
    public class NpcScanner
    {
        private const int MaxPeds = 30;

        private readonly List<int> candidates = new List<int>();
        private readonly HashSet<int> excluded = new HashSet<int>();   // NPCs שכבר גנבנו מהם
        private int nextScan;

        public IReadOnlyList<int> Candidates { get { return candidates; } }

        /// <summary>קוראים בכל Tick. הסריקה עצמה רצה רק כל ScanIntervalMs.</summary>
        public void Update(Ped player)
        {
            if (Game.GameTime < nextScan) return;
            nextScan = Game.GameTime + Config.ScanIntervalMs;
            Scan(player);
        }

        public void Clear()
        {
            candidates.Clear();
        }

        /// <summary>מסירים NPC מהרשימה ולא מציגים אותו שוב.</summary>
        public void Exclude(int handle)
        {
            excluded.Add(handle);
            candidates.Remove(handle);
        }

        public static bool IsValidTarget(int handle)
        {
            if (!Function.Call<bool>(Hash.DOES_ENTITY_EXIST, handle)) return false;
            if (!Function.Call<bool>(Hash.IS_PED_HUMAN, handle)) return false;
            if (Function.Call<bool>(Hash.IS_ENTITY_DEAD, handle, false)) return false;
            if (Function.Call<bool>(Hash.IS_PED_IN_ANY_VEHICLE, handle, false)) return false;
            return true;
        }

        private void Scan(Ped player)
        {
            candidates.Clear();

            System.IntPtr buf = Marshal.AllocHGlobal((MaxPeds + 1) * 8);
            try
            {
                for (int i = 0; i < (MaxPeds + 1) * 2; i++)
                    Marshal.WriteInt32(buf, i * 4, 0);
                Marshal.WriteInt32(buf, 0, MaxPeds);

                int count = Function.Call<int>(Hash.GET_PED_NEARBY_PEDS, player.Handle, buf, -1);
                Vector3 pos = player.Position;

                for (int i = 0; i < count && i < MaxPeds; i++)
                {
                    int handle = Marshal.ReadInt32(buf, (i + 1) * 8);
                    if (handle == 0 || handle == player.Handle) continue;
                    if (excluded.Contains(handle)) continue;
                    if (!IsValidTarget(handle)) continue;

                    Vector3 pedPos = Function.Call<Vector3>(Hash.GET_ENTITY_COORDS, handle, true);
                    if (pos.DistanceTo(pedPos) > Config.HackRange) continue;

                    candidates.Add(handle);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buf);
            }
        }
    }
}
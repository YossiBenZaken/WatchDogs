using System;
using GTA;
using GTA.Math;
using GTA.Native;

namespace WatchDogsMod.Effects
{
    /// <summary>
    /// אנימציית "מקליד בטלפון" + אובייקט טלפון ביד.
    /// הטלפון נוצר פעם אחת ונשאר קיים. [עודכן] במקום רק להסתיר אותו בסיום,
    /// מנתקים אותו, מקפיאים ומעבירים מתחת למפה. בהאקינג הבא מחזירים אותו ליד.
    /// </summary>
    public class PhoneAnimation
    {
        private const string AnimDict = "cellphone@";
        private const string AnimName = "cellphone_text_read_base";
        private const string PhoneModel = "prop_npc_phone_02";
        private const int HandBone = 28422;   // PH_R_Hand (יד ימין)
        private const float StoragePosZ = -500f;   // [חדש] איפה "מאחסנים" את הטלפון כשהוא לא בשימוש

        // מיקום וזווית הטלפון ביד. אם הוא נראה עקום - משחקים עם המספרים האלה
        private static readonly Vector3 PhoneOffset   = new Vector3(0.0f, 0.0f, 0.0f);
        private static readonly Vector3 PhoneRotation = new Vector3(0.0f, 0.0f, 0.0f);

        private int phoneModelHash;
        private int phoneProp;          // 0 = אין טלפון
        private int phoneOwnerHandle;   // לאיזו דמות הוא שייך
        private bool requested;
        private bool playing;
        private bool warned;

        public void Preload()
        {
            if (requested) return;
            requested = true;

            phoneModelHash = Function.Call<int>(Hash.GET_HASH_KEY, PhoneModel);
            Function.Call(Hash.REQUEST_ANIM_DICT, AnimDict);
            Function.Call(Hash.REQUEST_MODEL, phoneModelHash);
        }

        public void Start(Ped player)
        {
            if (playing) return;

            if (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, AnimDict))
            {
                Function.Call(Hash.REQUEST_ANIM_DICT, AnimDict);
                return;
            }

            // flag 49 = לולאה + פלג גוף עליון + מאפשר תנועה
            Function.Call(Hash.TASK_PLAY_ANIM, player.Handle, AnimDict, AnimName,
                4.0f, -4.0f, -1, 49, 0.0f, false, false, false);
            playing = true;

            ShowPhone(player);
        }

        public void Stop(Ped player)
        {
            if (!playing) return;
            playing = false;

            Function.Call(Hash.STOP_ANIM_TASK, player.Handle, AnimDict, AnimName, 3.0f);
            HidePhone();
        }

        /// <summary>מוחק את הטלפון לגמרי (קוראים כשהסקריפט נסגר).</summary>
        public void Dispose(Ped player)
        {
            Stop(player);
            DeletePhone();
        }

        // [עודכן] מציגים: מוודאים שיש טלפון, מחזירים אותו מהאחסון, ומצמידים ליד
        private void ShowPhone(Ped player)
        {
            try
            {
                if (!EnsurePhone(player)) return;

                // מחזירים מהאחסון
                Function.Call(Hash.FREEZE_ENTITY_POSITION, phoneProp, false);

                int bone = Function.Call<int>(Hash.GET_PED_BONE_INDEX, player.Handle, HandBone);
                Function.Call(Hash.ATTACH_ENTITY_TO_ENTITY, phoneProp, player.Handle, bone,
                    PhoneOffset.X, PhoneOffset.Y, PhoneOffset.Z,
                    PhoneRotation.X, PhoneRotation.Y, PhoneRotation.Z,
                    true, true, false, true, 1, true, true);

                if (!Function.Call<bool>(Hash.IS_ENTITY_ATTACHED_TO_ENTITY, phoneProp, player.Handle))
                {
                    if (!warned)
                    {
                        warned = true;
                        GTA.UI.Screen.ShowSubtitle("Phone attach FAILED", 3000);
                    }
                    HidePhone();
                    return;
                }

                Function.Call(Hash.SET_ENTITY_VISIBLE, phoneProp, true, false);
            }
            catch (Exception)
            {
                HidePhone();
            }
        }

        // [עודכן] מסתירים בצורה מוחלטת: בלתי נראה + מנותק + מוקפא + מתחת למפה
        private void HidePhone()
        {
            if (phoneProp == 0) return;

            try
            {
                if (!Function.Call<bool>(Hash.DOES_ENTITY_EXIST, phoneProp))
                {
                    phoneProp = 0;
                    return;
                }

                Function.Call(Hash.SET_ENTITY_VISIBLE, phoneProp, false, false);
                Function.Call(Hash.DETACH_ENTITY, phoneProp, false, false);
                Function.Call(Hash.FREEZE_ENTITY_POSITION, phoneProp, true);
                Function.Call(Hash.SET_ENTITY_COORDS_NO_OFFSET, phoneProp, 0f, 0f, StoragePosZ, false, false, false);
            }
            catch (Exception) { }
        }

        // יוצר את הטלפון אם אין (או אם הדמות התחלפה). נוצר בלתי נראה ובלי התנגשות
        private bool EnsurePhone(Ped player)
        {
            if (phoneProp != 0
                && phoneOwnerHandle == player.Handle
                && Function.Call<bool>(Hash.DOES_ENTITY_EXIST, phoneProp))
                return true;

            DeletePhone();

            if (!Function.Call<bool>(Hash.HAS_MODEL_LOADED, phoneModelHash))
            {
                Function.Call(Hash.REQUEST_MODEL, phoneModelHash);
                return false;
            }

            Vector3 pos = player.Position;
            phoneProp = Function.Call<int>(Hash.CREATE_OBJECT, phoneModelHash,
                pos.X, pos.Y, pos.Z, false, false, false);
            if (phoneProp == 0) return false;

            Function.Call(Hash.SET_ENTITY_COLLISION, phoneProp, false, false);
            Function.Call(Hash.SET_ENTITY_VISIBLE, phoneProp, false, false);
            phoneOwnerHandle = player.Handle;
            return true;
        }

        private void DeletePhone()
        {
            if (phoneProp == 0) return;

            HidePhone();   // [חדש] קודם מרחיקים, ואז מנסים למחוק - אם המחיקה נכשלת לא רואים כלום

            try
            {
                if (Function.Call<bool>(Hash.DOES_ENTITY_EXIST, phoneProp))
                    Function.Call(Hash.DELETE_ENTITY, new OutputArgument(phoneProp));
            }
            catch (Exception) { }

            phoneProp = 0;
        }
    }
}
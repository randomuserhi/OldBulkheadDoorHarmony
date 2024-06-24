using API;
using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using LevelGeneration;
using System.Collections;
using UnityEngine;

namespace OldBulkheadDoorHarmony {
    [HarmonyPatch]
    internal static class Sound {
        private static IEnumerator PlaySound(CellSoundPlayer player, uint soundID, float delay) {
            yield return new WaitForSeconds(delay);
            player?.Post(soundID, isGlobal: false);
        }

        private static bool TryGetValidBulkheadAnim(LG_SecurityDoor door, out LG_SecurityDoor_Anim doorAnim) {
            doorAnim = null;
            iLG_Door_Animation anim = door.m_anim;
            if (anim == null) {
                return false;
            }
            LG_SecurityDoor_Anim lG_SecurityDoor_Anim = ((Il2CppObjectBase)anim).TryCast<LG_SecurityDoor_Anim>();
            if (lG_SecurityDoor_Anim == null) {
                return false;
            }
            if (!lG_SecurityDoor_Anim.m_isBulkheadDoor) {
                return false;
            }
            if (lG_SecurityDoor_Anim.m_bulkheadLightStrips == null) {
                return false;
            }
            doorAnim = lG_SecurityDoor_Anim;
            return true;
        }

        private static IEnumerator TurnOnStrip(Renderer strip, float delay) {
            yield return new WaitForSeconds(delay);
            if (strip != null) {
                strip.material.SetColor("_EmissiveColor", Color.white);
            }
        }

        [HarmonyPatch(typeof(LG_SecurityDoor), nameof(LG_SecurityDoor.OnDoorIsOpened))]
        [HarmonyPostfix]
        private static void Postfix_OnDoorIsOpened(LG_SecurityDoor __instance) {
            try {
                LG_SecurityDoor lG_SecurityDoor = __instance;
                if (lG_SecurityDoor.m_securityDoorType != eSecurityDoorType.Bulkhead) {
                    return;
                }
                switch (lG_SecurityDoor.LinksToLayerType) {
                case LG_LayerType.MainLayer:
                    MonoBehaviourExtensions.StartCoroutine((MonoBehaviour)(object)lG_SecurityDoor, PlaySound(lG_SecurityDoor.m_sound, 2600464124u, 2.5f));
                    break;
                case LG_LayerType.SecondaryLayer:
                    MonoBehaviourExtensions.StartCoroutine((MonoBehaviour)(object)lG_SecurityDoor, PlaySound(lG_SecurityDoor.m_sound, 2059166368u, 2.5f));
                    break;
                case LG_LayerType.ThirdLayer:
                    MonoBehaviourExtensions.StartCoroutine((MonoBehaviour)(object)lG_SecurityDoor, PlaySound(lG_SecurityDoor.m_sound, 1528229518u, 2.5f));
                    break;
                }
                if (TryGetValidBulkheadAnim(lG_SecurityDoor, out var doorAnim)) {
                    for (int i = 0; i < ((Il2CppArrayBase<Renderer>)(object)doorAnim.m_bulkheadLightStrips).Length; i++) {
                        Renderer strip = ((Il2CppArrayBase<Renderer>)(object)doorAnim.m_bulkheadLightStrips)[i];
                        MonoBehaviourExtensions.StartCoroutine((MonoBehaviour)(object)lG_SecurityDoor, TurnOnStrip(strip, 1f + 0.2f * (float)i));
                    }
                }
            } catch (Exception data) {
                APILogger.Error((object)data);
            }
        }


        private static CellSoundPlayer? _TempPlayer;

        private static CellSoundPlayer _EmptySoundPlayer {
            get {
                if (_TempPlayer == null) {
                    _TempPlayer = new CellSoundPlayer();
                    _TempPlayer.UpdatePosition(new Vector3(0f, -9999999f, 0f));
                }
                return _TempPlayer;
            }
        }

        private static CellSoundPlayer sound;
        [HarmonyPatch(typeof(LG_SecurityDoor), nameof(LG_SecurityDoor.OnDoorOpenStarted))]
        [HarmonyPrefix]
        private static void Prefix_OnDoorOpenStarted(LG_SecurityDoor __instance) {
            LG_SecurityDoor lG_SecurityDoor = __instance;
            sound = lG_SecurityDoor.m_sound;
            lG_SecurityDoor.m_sound = _EmptySoundPlayer;
        }
        [HarmonyPatch(typeof(LG_SecurityDoor), nameof(LG_SecurityDoor.OnDoorOpenStarted))]
        [HarmonyPostfix]
        private static void Postfix_OnDoorOpenStarted(LG_SecurityDoor __instance) {
            LG_SecurityDoor lG_SecurityDoor = __instance;
            lG_SecurityDoor.m_sound = sound;
            try {
                if (!TryGetValidBulkheadAnim(lG_SecurityDoor, out var doorAnim)) {
                    return;
                }
                foreach (Renderer item in (Il2CppArrayBase<Renderer>)(object)doorAnim.m_bulkheadLightStrips) {
                    item.material.SetColor("_EmissiveColor", Color.black);
                }
            } catch (Exception data) {
                APILogger.Error((object)data);
            }
        }
    }
}

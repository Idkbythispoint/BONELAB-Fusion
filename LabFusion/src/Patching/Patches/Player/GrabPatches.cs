﻿using HarmonyLib;

using LabFusion.Utilities;
using LabFusion.Grabbables;
using LabFusion.Network;
using LabFusion.Entities;
using LabFusion.Player;
using LabFusion.Data;
using LabFusion.Marrow;

using Il2CppSLZ.Marrow;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(ForcePullGrip), nameof(ForcePullGrip.OnFarHandHoverUpdate))]
public static class ForcePullPatches
{
    public static bool Prefix(ForcePullGrip __instance, ref bool __state, Hand hand)
    {
        __state = __instance.pullCoroutine != null;

        if (NetworkInfo.HasServer && NetworkPlayerManager.HasExternalPlayer(hand.manager))
        {
            return false;
        }

        return true;
    }

    public static void Postfix(ForcePullGrip __instance, ref bool __state, Hand hand)
    {
        if (!(__instance.pullCoroutine != null && !__state))
            return;

        GrabHelper.SendObjectForcePull(hand, __instance._grip);
    }
}

[HarmonyPatch(typeof(Grip))]
public static class GripPatches
{
    public static readonly ComponentHashTable<Grip> HashTable = new();

    [HarmonyPatch(nameof(Grip.Awake))]
    [HarmonyPrefix]
    private static void Awake(Grip __instance)
    {
        // Only hash grips which don't have an entity (aka static grips)
        if (__instance._marrowEntity != null)
        {
            return;
        }

        var hash = GameObjectHasher.GetHierarchyHash(__instance.gameObject);

        var index = HashTable.AddComponent(hash, __instance);

#if DEBUG
        if (index > 0)
        {
            FusionLogger.Log($"Grip {__instance.name} had a conflicting hash {hash} and has been added at index {index}.");
        }
#endif
    }

    [HarmonyPatch(nameof(Grip.OnDestroy))]
    [HarmonyPrefix]
    private static void OnDestroy(Grip __instance)
    {
        HashTable.RemoveComponent(__instance);
    }

    [HarmonyPatch(nameof(Grip.OnAttachedToHand))]
    [HarmonyPostfix]
    private static void OnAttachedToHand(Grip __instance, Hand hand)
    {
        // Make sure this is the local player
        if (!hand.manager.IsLocalPlayer())
        {
            return;
        }

        GrabHelper.SendObjectAttach(hand, __instance);

        try
        {
            LocalPlayer.OnGrab?.Invoke(hand, __instance);
        }
        catch (Exception e)
        {
            FusionLogger.LogException("running LocalPlayer.OnGrab", e);
        }
    }

    [HarmonyPatch(nameof(Grip.OnDetachedFromHand))]
    [HarmonyPostfix]
    private static void OnDetachedFromHand(Grip __instance, Hand hand)
    {
        // Make sure this is the local player
        if (!hand.manager.IsLocalPlayer())
        {
            return;
        }

        GrabHelper.SendObjectDetach(hand);

        try
        {
            LocalPlayer.OnRelease?.Invoke(hand, __instance);
        }
        catch (Exception e)
        {
            FusionLogger.LogException("running LocalPlayer.OnRelease", e);
        }
    }
}
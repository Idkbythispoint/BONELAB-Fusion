﻿using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Entities;
using LabFusion.Senders;
using LabFusion.Marrow.Extenders;

using UnityEngine;

namespace LabFusion.Utilities;

public static class ImpactUtilities
{
    public static void OnHitRigidbody(Rigidbody rb)
    {
        var go = rb.gameObject;

        var marrowBody = MarrowBody.Cache.Get(go);

        if (marrowBody == null)
        {
            return;
        }

        // Check if the body already has an entity attached
        if (MarrowBodyExtender.Cache.TryGet(marrowBody, out var entity))
        {
            if (entity.IsOwnerLocked)
            {
                return;
            }

            var gripExtender = entity.GetExtender<GripExtender>();

            if (gripExtender != null && gripExtender.CheckHeld())
            {
                return;
            }

            // Transfer ownership
            NetworkEntityManager.TakeOwnership(entity);
        }
        // Create a new network entity
        else
        {
            DelayUtilities.InvokeDelayed(() => { PropSender.SendPropCreation(marrowBody.Entity); }, 4);
        }
    }
}

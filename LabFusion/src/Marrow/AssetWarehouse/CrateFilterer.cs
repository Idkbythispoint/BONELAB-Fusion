﻿using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.Marrow;

public static class CrateFilterer
{
    public static PalletManifest GetManifest(Pallet pallet)
    {
        var manifests = AssetWarehouse.Instance.palletManifests;
        var barcode = pallet.Barcode;

        if (!manifests.ContainsKey(barcode))
        {
            return null;
        }

        return manifests[barcode];
    }

    public static bool HasCrate<TCrate>(Barcode barcode) where TCrate : Crate
    {
        var warehouse = AssetWarehouse.Instance;

        if (!warehouse._crateRegistry.ContainsKey(barcode))
        {
            return false;
        }

        return warehouse._crateRegistry[barcode].TryCast<TCrate>() != null;
    }

    public static TCrate GetCrate<TCrate>(Barcode barcode) where TCrate : Crate
    {
        var warehouse = AssetWarehouse.Instance;

        if (!warehouse._crateRegistry.ContainsKey(barcode))
        {
            return null;
        }

        return warehouse._crateRegistry[barcode].TryCast<TCrate>();
    }

    public static TCrate[] FilterByTags<TCrate>(Pallet pallet, params string[] tags) where TCrate : Crate
    {
        List<TCrate> filtered = new();

        foreach (var crate in pallet.Crates)
        {
            var genericCrate = crate.TryCast<TCrate>();

            if (!genericCrate)
            {
                continue;
            }

            if (!HasTags(crate, tags))
            {
                continue;
            }

            filtered.Add(genericCrate);
        }

        return filtered.ToArray();
    }

    public static TCrate[] FilterByTags<TCrate>(params string[] tags) where TCrate : Crate
    {
        var crates = AssetWarehouse.Instance.GetCrates<TCrate>();
        List<TCrate> filtered = new();

        foreach (var crate in crates) 
        {
            if (!HasTags(crate, tags))
            {
                continue;
            }

            filtered.Add(crate);
        }

        return filtered.ToArray();
    }

    public static bool HasTags(Crate crate, params string[] tags)
    {
        foreach (var tag in tags)
        {
            if (!crate.Tags.Contains(tag))
            {
                return false;
            }
        }

        return true;
    }
}
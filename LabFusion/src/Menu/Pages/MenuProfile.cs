﻿using LabFusion.Data;
using LabFusion.Marrow;
using LabFusion.Marrow.Proxies;
using LabFusion.Player;
using LabFusion.Preferences.Client;

using UnityEngine;

namespace LabFusion.Menu;

public static class MenuProfile
{
    public static PlayerElement ProfileElement { get; private set; }

    private static string PreviousAvatarTitle { get; set; } = null;

    private static void UpdatePlayerIcon(PlayerElement element)
    {
        var avatarCrate = RigData.Refs.RigManager.AvatarCrate.Crate;
        var avatarTitle = avatarCrate.Title;

        // No need to update the icon if its the same avatar
        if (avatarTitle == PreviousAvatarTitle)
        {
            return;
        }

        PreviousAvatarTitle = avatarTitle;

        ElementIconHelper.SetProfileIcon(element, avatarTitle, CrateFilterer.GetModId(avatarCrate.Pallet));
    }

    public static void RefreshIcon()
    {
        UpdatePlayerIcon(ProfileElement);
    }

    public static void PopulateProfile(GameObject profilePage)
    {
        // Reset the stored avatar
        PreviousAvatarTitle = null;

        // Update the profile grid
        ProfileElement = profilePage.transform.Find("panel_Profile").GetComponent<PlayerElement>();

        ProfileElement.GetElements();

        ProfileElement.UsernameElement
            .WithTitle(LocalPlayer.Username);

        ProfileElement.NicknameElement
            .WithTitle("Nickname")
            .AsPref(ClientSettings.Nickname);

        ProfileElement.DescriptionElement
            .WithTitle("Description")
            .AsPref(ClientSettings.Description);

        // Update the icon
        ProfileElement.PlayerPage.OnShown += () =>
        {
            RefreshIcon();
        };

        RefreshIcon();

        // Disable unnecessary elements
        ProfileElement.ActionsGrid.SetActive(false);

        ProfileElement.VolumeElement.gameObject.SetActive(false);
        ProfileElement.PermissionsElement.gameObject.SetActive(false);
    }
}
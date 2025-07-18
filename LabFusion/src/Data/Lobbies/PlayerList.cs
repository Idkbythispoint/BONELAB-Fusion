﻿using LabFusion.Player;

using System.Text.Json.Serialization;

namespace LabFusion.Data;

[Serializable]
public class PlayerList
{
    [JsonPropertyName("players")]
    public PlayerInfo[] Players { get; set; }

    public void WritePlayers()
    {
        // Create player info array from all players
        Players = new PlayerInfo[PlayerIDManager.PlayerCount];
        int index = 0;

        foreach (var player in PlayerIDManager.PlayerIDs)
        {
            Players[index++] = new PlayerInfo(player);
        }
    }
}

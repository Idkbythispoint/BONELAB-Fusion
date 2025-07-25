﻿using LabFusion.Menu;

using UnityEngine;

namespace LabFusion.SDK.Achievements
{
    public class WarmWelcome : Achievement
    {
        public override string Title => "Warm Welcome";

        public override string Description => "Join a server.";

        public override int BitReward => 50;
    }
}

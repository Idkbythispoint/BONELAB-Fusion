using UnityEngine;

#if MELONLOADER
using MelonLoader;
#endif

namespace LabFusion.Marrow.Integration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class GamemodePlayerSettings : MonoBehaviour
    {
#if MELONLOADER
        public GamemodePlayerSettings(IntPtr intPtr) : base(intPtr) { }

        public static GamemodePlayerSettings Instance { get; set; } = null;

        private string _avatarOverride = null;
        public string AvatarOverride => _avatarOverride;

        private float? _vitalityOverride = null;
        public float? VitalityOverride => _vitalityOverride;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void SetAvatar(string avatarBarcode)
        {
            _avatarOverride = avatarBarcode;
        }

        public void SetVitality(float vitality)
        {
            _vitalityOverride = vitality;
        }
#else
        public void SetAvatar(string avatarBarcode)
        {
        }

        public void SetVitality(float vitality)
        {
        }
#endif
    }
}
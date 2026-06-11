using System;
using System.Linq;
using UnityEngine;
namespace EFT
{
    // Token: 0x02001244 RID: 4676
    [CreateAssetMenu]
    public class SoundBank : ScriptableObject
    {
        // Token: 0x04007B14 RID: 31508
        public float CustomLength;

        // Token: 0x04007B15 RID: 31509
        public float BaseVolume = 1f;

        // Token: 0x04007B16 RID: 31510
        public float BasePitch = 1f;

        // Token: 0x04007B17 RID: 31511
        public float DeltaVolume;

        // Token: 0x04007B18 RID: 31512
        public float DeltaPitch;

        // Token: 0x04007B19 RID: 31513
        public float Rolloff = 100f;

        // Token: 0x04007B1A RID: 31514
        public bool IgnoreOcclusion;

        // Token: 0x04007B1B RID: 31515
        public bool Physical;

        // Token: 0x04007B1C RID: 31516
        public bool HasEnvironment;

        // Token: 0x04007B1D RID: 31517
        public BetterAudio.AudioSourceGroupType SourceType = BetterAudio.AudioSourceGroupType.Environment;

        // Token: 0x04007B1E RID: 31518
        public bool AllowLimitedPlay;

        // Token: 0x04007B1F RID: 31519
        [SerializeField]
        private float _clipLiength;

        // Token: 0x04007B20 RID: 31520
        public EnvironmentVariety[] Environments = new EnvironmentVariety[Enum.GetNames(typeof(EnvironmentType)).Length];

        // Token: 0x04007B21 RID: 31521
        public DistanceBlendOptions BlendOptions;

        // Token: 0x04007B22 RID: 31522
        private byte[] _shuffle;

        // Token: 0x04007B23 RID: 31523
        private byte _shuffleIndex;
    }
}
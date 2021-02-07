#if UNITY_POST_PROCESSING_STACK_V2

using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace FoW
{
    public class FogOfWarTestPPSv2 : FogOfWarTestPlatform
    {
        public PostProcessVolume volume;

        public override void SetCameraTeam(Camera cam, int team)
        {
            PostProcessProfile profile = volume.sharedProfile;
            if (profile != null)
            {
                FogOfWarPPSv2 fow = profile.GetSetting<FogOfWarPPSv2>();
                fow.team.value = team;
            }
        }
    }
}

#endif

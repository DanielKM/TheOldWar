using UnityEngine;

namespace FoW
{
    public class FogOfWarTestLegacy : FogOfWarTestPlatform
    {
        public override void SetCameraTeam(Camera cam, int team)
        {
            FogOfWarLegacy fow = cam.GetComponent<FogOfWarLegacy>();
            if (fow != null)
                fow.team = team;
        }
    }
}

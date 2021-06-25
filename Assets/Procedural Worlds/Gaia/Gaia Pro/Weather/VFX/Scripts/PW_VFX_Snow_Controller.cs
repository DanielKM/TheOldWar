using UnityEngine;

namespace Gaia
{
    [ExecuteInEditMode]
    public class PW_VFX_Snow_Controller : MonoBehaviour
    {
        public ParticleSystem PW_Snow_Particles;
        public Vector3 SnowWindDir;
        ParticleSystem.VelocityOverLifetimeModule VelocityOverLifetime;

        // Update is called once per frame
        void Update()
        {
            if (PW_Snow_Particles == null)
            {
                PW_Snow_Particles = gameObject.GetComponent<ParticleSystem>();
            }

            if (PW_Snow_Particles != null)
            {
                VelocityOverLifetime = PW_Snow_Particles.velocityOverLifetime;
            }

            VelocityOverLifetime.enabled = true;
            VelocityOverLifetime.space = ParticleSystemSimulationSpace.World;

            VelocityOverLifetime.x = SnowWindDir.x;
            VelocityOverLifetime.y = SnowWindDir.y;
            VelocityOverLifetime.z = SnowWindDir.z;
        }
    }
}
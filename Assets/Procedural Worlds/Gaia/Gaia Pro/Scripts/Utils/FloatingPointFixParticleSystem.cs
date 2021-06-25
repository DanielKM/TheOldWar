using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if GAIA_PRO_PRESENT
namespace Gaia
{

    [RequireComponent(typeof(ParticleSystem))]
    public class FloatingPointFixParticleSystem : FloatingPointFixMember
    {

        protected new void OnEnable()
        {
            ParticleSystem ps = GetComponent<ParticleSystem>();
            //Register as Particle system
            if (ps && ps.main.simulationSpace == ParticleSystemSimulationSpace.World)
                FloatingPointFix.Instance.AddWorldSpaceParticleSystem(ps);
        }

        protected new void OnDestroy()
        {
            if (FloatingPointFix.IsActive)
            {
                ParticleSystem ps = GetComponent<ParticleSystem>();
                if (ps)
                    FloatingPointFix.Instance.RemoveWorldSpaceParticleSystem(ps);
            }
        }


        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
#endif

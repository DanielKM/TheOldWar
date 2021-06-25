using System.Collections;
using UnityEngine;
namespace GeNa.Core
{
    //TODO : Manny : Temporarily Disabled Particles
    [ExecuteAlways]
    public class GeNaAnimatorDecorator : MonoBehaviour, IDecorator
    {
        #region Variables
        [SerializeField] protected Animator[] m_animators;
        // [SerializeField] protected ParticleSystem[] m_particles;
        [SerializeField] protected bool m_updateAnimations = true;
        [SerializeField] protected float m_timer = 0f;
        #endregion
        #region Properties
        // Should GeNa Unpack the Prefab that this Decorator is attached to? 
        public bool UnpackPrefab => false;
        public Animator[] Animators
        {
            get => m_animators;
            set => m_animators = value;
        }
        public bool UpdateAnimations
        {
            get => m_updateAnimations;
            set => m_updateAnimations = value;
        }
        // public ParticleSystem[] Particles
        // {
        //     get => m_particles;
        //     set => m_particles = value;
        // }
        #endregion
        #region Methods
        #region Events
        private void OnEnable()
        {
            m_animators = GetComponentsInChildren<Animator>();
            foreach (var animator in m_animators)
            {
                if (animator != null)
                {
                    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                    animator.Play(stateInfo.shortNameHash,0,1f);
                    // animator.playbackTime = 0f;
                    animator.Update(0f);
                }
            }
        }
        // Called when Decorator is Ingested into GeNa
        public void OnIngest(Resource resource)
        {
        }
        // Runs once this Decorator is Spawned
        public IEnumerator OnSelfSpawned(Resource resource)
        {
            yield break;
        }
        #endregion
        #region External
        public void UpdateState()
        {
            if (!enabled)
                return;
            if (!UpdateAnimations)
                return;
            m_animators = GetComponentsInChildren<Animator>();
            foreach (var animator in m_animators)
                animator.Update(Time.deltaTime);
            // m_particles = GetComponentsInChildren<ParticleSystem>();
            // if(m_particles.Length > 0) 
            //     m_timer += Time.deltaTime;
            // foreach (var particle in m_particles)
            //     if (particle != null)
            //     {
            //         // particle.Simulate(timeline, true, true);
            //         // particle.Pause(true);
            //         // particle.time = timeline;
            //         // particle.Stop();
            //         // // particle.useAutoRandomSeed = false;
            //         // particle.Simulate(m_timer, true, true);
            //         particle.Pause(true);
            //         particle.time = m_timer;
            //         if (m_timer >= particle.main.duration)
            //             particle.Stop();
            //     }
        }
        // Runs directly after Spawning Children Decorators
        public void OnChildrenSpawned(Resource resource)
        {
        }
        public void LoadReferences(Palette palette)
        {
        }
        #endregion
        #endregion
    }
}
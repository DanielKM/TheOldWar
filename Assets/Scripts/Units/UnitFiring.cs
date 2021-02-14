using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class UnitFiring : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private Animator animator = null;
    [SerializeField] private GameObject projectilePrefab = null;
    [SerializeField] private Transform projectileSpawnPoint = null;
    [SerializeField] AudioClip attackSound;
    [SerializeField] AudioSource unitAudioSource;

    [Header("Settings")]
    [SerializeField] private float fireRange = 5f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float rotationSpeed = 20f;

    private UnitInformation unitInformation;
    private PooledGameobjects pooledGameobjects;
    private List<GameObject> projectiles;

    private float lastFireTime;
    ResourceGatherer gatherer;
    NavMeshAgent agent;

    private void Start()
    {
        // Setting clones
        unitInformation = gameObject.GetComponent<UnitInformation>();
        gatherer = gameObject.GetComponent<ResourceGatherer>();
        agent = gameObject.GetComponent<NavMeshAgent>();

        if(unitInformation.owner != null)
        {
            pooledGameobjects = unitInformation.owner.gameObject.GetComponent<PooledGameobjects>();
            
            if(pooledGameobjects.isComputerAI) { return; } 

            // CheckForProjectileLists();
        }
    }

    void CheckForProjectileLists()
    {
        if(projectilePrefab.name + "(Clone)" == pooledGameobjects.arrows[0].name) 
        {
            projectiles = pooledGameobjects.arrows;
        } else if (projectilePrefab.name + "(Clone)" == pooledGameobjects.pickaxes[0].name)
        {
            projectiles = pooledGameobjects.pickaxes;
        } else if (projectilePrefab.name + "(Clone)" == pooledGameobjects.swords[0].name)
        {
            projectiles = pooledGameobjects.swords;
        } else if (projectilePrefab.name + "(Clone)" == pooledGameobjects.fireballs[0].name)
        {
            projectiles = pooledGameobjects.fireballs;
        }
    }

    [ServerCallback]
    private void Update() 
    {
        Targetable target = targeter.GetTarget();

        if(target == null) { return; }

        if (target.gameObject.GetComponent<Health>().currentHealth <= 0) 
        { 
            targeter.ClearTarget(); 
            return;
        }

        UnitTask unitTask = gameObject.GetComponent<UnitTask>();

        // Check for resource target
        if (target.gameObject.TryGetComponent<ResourceNode>(out ResourceNode resourceNode)) 
        { 
            if(unitTask.GetTask() != ActionList.Gathering && unitTask.GetTask() != ActionList.Harvesting) 
            {
                unitTask.SetTask(ActionList.Gathering);    
            }

            if(gatherer) 
            {
                if(gatherer.heldResources == gatherer.maxHeldResources) 
                {
                    targeter.TargetClosestDropOff();

                    return;
                }
            }
        }

        // Check for building target
        if(gatherer) 
        {
            if (target.gameObject.TryGetComponent<Foundation>(out Foundation foundation)) 
            { 
                if(unitTask.GetTask() != ActionList.Construction && unitTask.GetTask() != ActionList.Building) 
                {
                    unitTask.SetTask(ActionList.Construction);    
                }
            }
        }

        // Set as enemy and attack if not resource or building
        if(unitTask.GetTask() != ActionList.Construction 
        && unitTask.GetTask() != ActionList.Gathering 
        && unitTask.GetTask() != ActionList.Attacking
        && unitTask.GetTask() != ActionList.Building
        && unitTask.GetTask() != ActionList.Harvesting
        && unitTask.GetTask() != ActionList.Fighting)
        {
            unitTask.SetTask(ActionList.Attacking);    
        }

        if(!CanFireAtTarget()) { return; }

        if(unitTask.GetTask() != ActionList.Fighting && unitTask.GetTask() != ActionList.Building && unitTask.GetTask() != ActionList.Harvesting) { 
            // if(pooledGameobjects.isComputerAI) { CheckForProjectileLists(); } 
            if(unitTask.GetTask() == ActionList.Gathering)
            {   
                unitTask.SetUnitTask(ActionList.Harvesting); 
            } else if (unitTask.GetTask() == ActionList.Attacking)
            {
                unitTask.SetUnitTask(ActionList.Fighting); 
            } else if (unitTask.GetTask() == ActionList.Construction)
            {
                unitTask.SetUnitTask(ActionList.Building); 
            }
        }
        
        Quaternion targetRotation = 
            Quaternion.LookRotation(target.transform.position - transform.position);

        transform.rotation = 
            Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        if(Time.time > (fireRate) + lastFireTime + 0.02) 
        {   
            agent.ResetPath();

            StartCoroutine(FireProjectile(target));
        }
    }
    
    private IEnumerator FireProjectile(Targetable target)
    {
        lastFireTime = Time.time;

        // animator.SetBool("isFiring", true);
        //Check attackSpeed & make animation follow this speed
        animator.speed = 1/fireRate;

        yield return new WaitForSeconds(fireRate);

        float attackFrequency = 1/fireRate;

        //Start animation  
        // if( animator.GetCurrentAnimatorSatateInfo(Layer).IsName("yourAttackAnimation"))
        // {
        //    animator.speed = animSpeed;
        // }

        Quaternion projectileRotation = 
            Quaternion.LookRotation(target.GetAimAtPoint().position - projectileSpawnPoint.position);

        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileRotation);

        NetworkServer.Spawn(projectile, connectionToClient);

        UnitProjectile projectileAttributes = projectile.GetComponent<UnitProjectile>();
        float damageModifier = CheckWeaponDamageModifiers();

        projectileAttributes.damageToDeal = projectileAttributes.savedDamage;
        projectileAttributes.damageToDeal = projectileAttributes.damageToDeal * damageModifier;
        projectileAttributes.projectileFirer = this;

        unitAudioSource.clip = attackSound;
        unitAudioSource.Play();

        // // Pooled Projectiles
        // for(int i = 0; i<projectiles.Count; i++)
        // {
        //     if(!projectiles[i].activeInHierarchy)
        //     {
        //         projectiles[i].transform.position = projectileSpawnPoint.position;
        //         projectiles[i].transform.rotation = projectileRotation;
        //         UnitProjectile projectileAttributes = projectiles[i].GetComponent<UnitProjectile>();
        //         float damageModifier = CheckWeaponDamageModifiers();

        //         projectileAttributes.damageToDeal = projectileAttributes.savedDamage;

        //         projectileAttributes.damageToDeal = projectileAttributes.damageToDeal * damageModifier;
                
        //         projectileAttributes.projectileFirer = this;

        //         projectiles[i].SetActive(true);

        //         unitAudioSource.clip = attackSound;
        
        //         unitAudioSource.Play();

        //         lastFireTime = Time.time;
        //         break;
        //     }
        // }
    }

    [Server]
    private float CheckWeaponDamageModifiers()
    {
        float damageModifier = 1f;

        switch (unitInformation.unitWeapon)
        {
            case WeaponType.None:
                damageModifier = 1f;
                return damageModifier;
            case WeaponType.Pickaxe:
                damageModifier = 2f;
                return damageModifier;
            case WeaponType.ShortSword:
                damageModifier = 3f;
                return damageModifier;
            case WeaponType.HeavySword:
                damageModifier = 4f;
                return damageModifier;
            case WeaponType.Bow:
                damageModifier = 2f;
                return damageModifier;
            case WeaponType.Warhammer:
                damageModifier = 5f;
                return damageModifier;
            case WeaponType.Magic:
                damageModifier = 5f;
                return damageModifier;
            default:
                break;
        }

        return damageModifier;
    }

    [Server]
    private bool CanFireAtTarget()
    {
        return (targeter.GetTarget().transform.position - transform.position).sqrMagnitude 
            <= fireRange * fireRange;
    }
}

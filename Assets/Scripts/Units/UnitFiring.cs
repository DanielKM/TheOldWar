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
    [SerializeField] public GameObject projectilePrefab = null;
    [SerializeField] private Transform projectileSpawnPoint = null;
    [SerializeField] AudioClip attackSound;
    [SerializeField] AudioSource unitAudioSource;
    UnitTask unitTask = null;

    [Header("Settings")]
    [SerializeField] private float fireRange = 5f;
    [SerializeField] public float fireRate = 1f;
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
        unitTask = gameObject.GetComponent<UnitTask>();

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

        if(target == null) {
            var actuallyNull = object.ReferenceEquals(target, null);
            if(actuallyNull) { 
                return; 
            }
            else if (unitTask.GetTask() == ActionList.Harvesting)
            {
                target = targeter.GetClosestResource(targeter.GetResourceID(gatherer.heldResourcesType));

                targeter.CmdSetTarget(target.gameObject);

                return;
            } else {
                return; 
            }
        }

        if (target.gameObject.GetComponent<Health>().currentHealth <= 0) 
        { 
            if(!gatherer) 
            {
                targeter.ClearTarget(); 
                return;
            }
        }

        // Check for resource target
        if (target.gameObject.TryGetComponent<ResourceNode>(out ResourceNode resourceNode)) 
        { 
            if(resourceNode.enabled) 
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
            if (target.gameObject.TryGetComponent<Corpse>(out Corpse corpse)) 
            { 
                if(unitTask.GetTask() != ActionList.ClearingDead && unitTask.GetTask() != ActionList.Destroying) 
                {
                    unitTask.SetTask(ActionList.ClearingDead);    
                }
            }
            if (target.gameObject.TryGetComponent<Building>(out Building building)) 
            { 
                if(unitTask.GetTask() == ActionList.Repairing || unitTask.GetTask() == ActionList.RepairDuty) 
                {
                    if(target.GetComponent<Health>().currentHealth >= target.GetComponent<Health>().maxHealth)
                    {
                        targeter.ClearTarget();

                        targeter.TargetClosestRepairBuilding();

                        return;
                    }
                }
            }
        }

        // Set as enemy and attack if not resource or building
        if(unitTask.GetTask() != ActionList.Construction 
        && unitTask.GetTask() != ActionList.Gathering 
        && unitTask.GetTask() != ActionList.Attacking
        && unitTask.GetTask() != ActionList.Building
        && unitTask.GetTask() != ActionList.Harvesting
        && unitTask.GetTask() != ActionList.Fighting
        && unitTask.GetTask() != ActionList.Delivering
        && unitTask.GetTask() != ActionList.ClearingDead
        && unitTask.GetTask() != ActionList.Destroying
        && unitTask.GetTask() != ActionList.Repairing
        && unitTask.GetTask() != ActionList.RepairDuty)
        {
            unitTask.SetTask(ActionList.Attacking);    
        }

        if(!CanFireAtTarget()) { return; }

        if(unitTask.GetTask() != ActionList.Fighting && unitTask.GetTask() != ActionList.Building && unitTask.GetTask() != ActionList.Harvesting && unitTask.GetTask() != ActionList.Destroying) { 
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
            } else if (unitTask.GetTask() == ActionList.ClearingDead)
            {
                unitTask.SetUnitTask(ActionList.Destroying); 
            } else if (unitTask.GetTask() == ActionList.RepairDuty)
            {
                unitTask.SetUnitTask(ActionList.Repairing); 
            }
            agent.ResetPath();
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
        animator.speed = 1/fireRate;

        yield return new WaitForSeconds(fireRate);

        float attackFrequency = 1/fireRate;

        if(target.GetAimAtPoint())
        {
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
        }


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

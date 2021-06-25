using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMaterialSelector : MonoBehaviour
{
    public List<Material> materials = new List<Material>();

    private void OnEnable()
    {
        int random = Random.Range(1, materials.Count);

        ParticleSystemRenderer particleSystem = GetComponent<ParticleSystemRenderer>();
        particleSystem.material = materials[random];
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting;



[ExecuteInEditMode]
public class PW_Terrain_Grass_Manager : MonoBehaviour
{


    public Texture2D PW_Splat_Grass;
    public Terrain CurrentTerrain;

    public Color TerrainGrassColorOverride = Color.white;

    public Vector3 TerrainPosition;
    public float TerrainTexResolutionX = 2048;
    public float TerrainTexResolutionY = 2048;


    // Start is called before the first frame update
    void Start()
    {
       // StartCoroutine(wait());



    }


    IEnumerator wait()
    {
        yield return new WaitForSeconds(1f);
      //  Debug.Log("Coroutine is work");
        TerrainDataRecheck();
        StartCoroutine(wait());
    }

    // Update is called once per frame
    void Update()
    {
        TerrainDataRecheck();

    
       
    }

    private void TerrainDataRecheck()
    {

        Shader.SetGlobalColor("PW_Grass_Color", TerrainGrassColorOverride);
        if(CurrentTerrain != null)
        {
            TerrainPosition = CurrentTerrain.transform.position;
        }

        
        Shader.SetGlobalFloat("TerrainTexResolutionX", TerrainTexResolutionX);
        Shader.SetGlobalFloat("TerrainTexResolutionY", TerrainTexResolutionY);

        Shader.SetGlobalTexture("PW_Splat_Grass", PW_Splat_Grass);
        // if (CurrentTerrain != null)
        //  {
        //     TerrainGrassColor = CurrentTerrain.terrainData.wavingGrassTint;

        // }
    }
}

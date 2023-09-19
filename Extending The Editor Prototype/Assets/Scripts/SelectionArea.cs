using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SelectionType
{
    Plateau

}
public enum PlateauType
{
    Circular
}

public class SelectionArea : MonoBehaviour
{
    [SerializeField]
    public Terrain terrain;


    [HideInInspector]
    public SelectionType selectionType;
    [HideInInspector]
    public PlateauType plateauType;

    [HideInInspector]
    public float radius = 1;
    [HideInInspector]
    public Vector3[] circleArray;



    public void Start()
    {
        ChangeTerrainHeight();
    }

    public Vector3 WorldPointToTerrainPoint(Vector3 worldPos)
    {
        Vector3 relativePos = worldPos - terrain.transform.position;
        Vector3 relativePos01 = new Vector3(relativePos.x / terrain.terrainData.size.x, relativePos.y / terrain.terrainData.size.y, relativePos.z / terrain.terrainData.size.z);
        Vector3 terrainPos = relativePos01 * terrain.terrainData.heightmapResolution;
        terrainPos.y = relativePos01.y;

        Debug.Log(terrainPos);

        return terrainPos;
    }



    public void ChangeTerrainHeight()
    {
        int terrainResolution = terrain.terrainData.heightmapResolution;
        float[,] newHeights = terrain.terrainData.GetHeights(0, 0, terrainResolution, terrainResolution);
        Debug.Log(terrainResolution);


        Vector3 affectedAreaInTerrain = WorldPointToTerrainPoint(transform.position);

        // change the height data
        for (int x = 0; x < terrainResolution; x++)
        {
            for (int y = 0; y < terrainResolution; y++)
            {
                if (x <= affectedAreaInTerrain.z + (radius * ((float)terrainResolution / 100)) 
                 && x >= affectedAreaInTerrain.z - (radius * ((float)terrainResolution / 100))
                 && y <= affectedAreaInTerrain.x + (radius * ((float)terrainResolution / 100))
                 && y >= affectedAreaInTerrain.x - (radius * ((float)terrainResolution / 100)))
                {
                    newHeights[x, y] = affectedAreaInTerrain.y;
                }
                else
                {
                    newHeights[x, y] = 0f;
                }
            }
        }

        // sets the terrain height to the changed heights
        terrain.terrainData.SetHeights(0, 0, newHeights);
    }
}

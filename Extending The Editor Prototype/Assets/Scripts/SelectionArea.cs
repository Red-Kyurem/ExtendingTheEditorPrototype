using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SelectionType
{
    Plateau,
    RampTEST
}
public enum PlateauType
{
    Circular,
    Rectangular
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
    public float width = 1;
    [HideInInspector]
    public float height = 1;
    [HideInInspector]
    public float depth = 1;
    [HideInInspector]
    public Vector3[] listArray;



    public void Start()
    {
        ChangeTerrainHeight();
    }

    public Vector3 WorldPointToTerrainPoint(Vector3 worldPos)
    {
        // removes any offset from the terrain's position 
        Vector3 relativePos = worldPos - terrain.transform.position;
        // sets relativePos values to be between 0 and 1
        Vector3 relativePos01 = new Vector3(relativePos.x / terrain.terrainData.size.x, relativePos.y / terrain.terrainData.size.y, relativePos.z / terrain.terrainData.size.z);
        // multiplies by the heightmapResolution to get where the world position is on the terrain
        Vector3 terrainPos = relativePos01 * terrain.terrainData.heightmapResolution;

        // terrainPos.y would be unused, so its storing how high the selection is compared to the floor (0) and the maximum terrain height (1)
        terrainPos.y = relativePos01.y;

        return terrainPos;
    }



    public void ChangeTerrainHeight()
    {
        int terrainResolution = terrain.terrainData.heightmapResolution;
        float[,] newHeights = terrain.terrainData.GetHeights(0, 0, terrainResolution, terrainResolution);

        Vector3 affectedAreaInTerrain = WorldPointToTerrainPoint(transform.position);

        // change the height data of each vertice
        for (int x = 0; x < terrainResolution; x++)
        {
            for (int y = 0; y < terrainResolution; y++)
            {
                bool heightChanged = false;

                // if the vertice is within the affected area
                if (x <= affectedAreaInTerrain.z + (radius * ((float)terrainResolution / 100)) 
                 && x >= affectedAreaInTerrain.z - (radius * ((float)terrainResolution / 100))
                 && y <= affectedAreaInTerrain.x + (radius * ((float)terrainResolution / 100))
                 && y >= affectedAreaInTerrain.x - (radius * ((float)terrainResolution / 100)))
                {
                    // if the vertice's distance is within the radius 
                    if (Vector2.Distance(new Vector2(x,y),new Vector2(affectedAreaInTerrain.z, affectedAreaInTerrain.x)) <= (radius * ((float)terrainResolution / 100)))
                    {
                        // sets the new height of the vertice
                        newHeights[x, y] = affectedAreaInTerrain.y;
                        heightChanged = true;
                    }
                }
                // set height to 0 if the vertice's hight has not changed
                if (!heightChanged)
                {
                    newHeights[x, y] = 0f;
                }
            }
        }

        // sets the terrain height to the changed heights
        terrain.terrainData.SetHeights(0, 0, newHeights);
    }
}

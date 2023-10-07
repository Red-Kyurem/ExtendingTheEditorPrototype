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
    [SerializeField]
    public LayerMask layers;


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
    
    public Vector3[] lineListArray;



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

        Vector3 affectedPoint = WorldPointToTerrainPoint(transform.position);

        // change the height data of each vertice
        for (int x = 0; x < terrainResolution; x++)
        {
            for (int y = 0; y < terrainResolution; y++)
            {
                if (plateauType == PlateauType.Circular)
                {
                    // checks if the vertice is within the affected area
                    if (VerticeInAffectedArea(x, y, affectedPoint, terrainResolution, radius, radius))
                    {
                        // if the vertice's distance is within the radius 
                        if (Vector2.Distance(new Vector2(x, y), new Vector2(affectedPoint.z, affectedPoint.x)) <= (radius * ((float)terrainResolution / 100)))
                        {
                            // sets the new height of the vertice
                            newHeights[x, y] = affectedPoint.y;
                        }
                    }
                }
                else if (plateauType == PlateauType.Rectangular)
                {
                    // checks if the vertice is within the affected area
                    if (VerticeInAffectedArea(x, y, affectedPoint, terrainResolution, width, depth))
                    {
                        // sets the new height of the vertice
                        newHeights[x, y] = affectedPoint.y;
                    }
                }
            }
        }

        // sets the terrain height to the changed heights
        terrain.terrainData.SetHeights(0, 0, newHeights);
    }

    bool VerticeInAffectedArea(int x, int y, Vector3 affectedPoint, int terrainRes, float width, float depth)
    {
        // if the vertice is within the brush's affected area
        if (x <= affectedPoint.z + (width * ((float)terrainRes / 100))
         && x >= affectedPoint.z - (width * ((float)terrainRes / 100))
         && y <= affectedPoint.x + (depth * ((float)terrainRes / 100))
         && y >= affectedPoint.x - (depth * ((float)terrainRes / 100)))
        {
            return true;
        }
        return false;
    }

}

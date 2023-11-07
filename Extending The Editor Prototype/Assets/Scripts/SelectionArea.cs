using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BrushType
{
    Plateau,
    Ramp
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
    public BrushType brushType;
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
    public Vector3[] gizmoArray;
    [HideInInspector]
    public Vector3[] verticeArray;


    public void Start()
    {
        ClearTerrainHeight();
        ChangeTerrainHeight();
    }
    void ClearTerrainHeight()
    {
        int terrainResolution = terrain.terrainData.heightmapResolution;
        float[,] newHeights = terrain.terrainData.GetHeights(0, 0, terrainResolution, terrainResolution);

        for (int x = 0; x < terrainResolution; x++)
        {
            for (int y = 0; y < terrainResolution; y++)
            {
                // sets the height of the vertice to 0 (flat)
                newHeights[x, y] = 0;
            }
        }
        // sets the terrain height to the changed heights
        terrain.terrainData.SetHeights(0, 0, newHeights);
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
                if (brushType == BrushType.Plateau)
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
                if (brushType == BrushType.Ramp)
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

    bool VerticeInAffectedArea(int terrainX, int terrainY, Vector3 affectedPoint, int terrainRes, float width, float depth)
    {

        float scaledWidth = width * ((float)terrainRes / 100);
        float scaledDepth = depth * ((float)terrainRes / 100);

        // if the vertice is within the brush's affected area
        if (terrainX <= (affectedPoint.z + scaledWidth)
         && terrainX >= (affectedPoint.z - scaledWidth)
         && terrainY <= (affectedPoint.x + scaledDepth)
         && terrainY >= (affectedPoint.x - scaledDepth))
        {
            return true;
        }
        return false;

    }

}

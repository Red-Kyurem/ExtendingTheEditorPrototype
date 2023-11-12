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
public enum RampDirectionType
{
    North,      //0
    West,       //1
    South,      //2
    East        //3
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
    public RampDirectionType rampDirection;

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
        IdentifyTerrainBrush();
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

    public void IdentifyTerrainBrush()
    {
        int terrainRes = terrain.terrainData.heightmapResolution;

        Vector3 affectedPoint = WorldPointToTerrainPoint(transform.position);

        float scaledWidth = width * ((float)terrainRes / 100);
        float scaledDepth = depth * ((float)terrainRes / 100);
        float scaledRadius = radius * ((float)terrainRes / 100);

        if (brushType == BrushType.Plateau)
        {
                    
            if (plateauType == PlateauType.Circular)
            {
                ChangeTerrainHeightPlateau(affectedPoint, scaledRadius*2, scaledRadius*2, true);
            }
            else if (plateauType == PlateauType.Rectangular)
            {
                ChangeTerrainHeightPlateau(affectedPoint, scaledWidth, scaledDepth, false);
            }
           
        }
        if (brushType == BrushType.Ramp)
        {
            // the height to be added to set the new height of the vertice
            // must convert to terrain values since height on a terrain map is between 0 and 1
            float addedHeight = WorldPointToTerrainPoint(Vector3.up * height).y;

            ChangeTerrainHeightRamp(affectedPoint, scaledWidth, scaledDepth, rampDirection);


        }

        
    }


    public void ChangeTerrainHeightPlateau(Vector3 affectedPoint, float scaledWidth, float scaledDepth, bool isCircle = false)
    {
        Debug.Log("heloo!");
        int terrainRes = terrain.terrainData.heightmapResolution;
        float[,] newHeights = terrain.terrainData.GetHeights(0, 0, terrainRes, terrainRes);

        // the edge of the brush that's closest to 0 on the terrain's x-axis
        float minBrushX = Mathf.Ceil(affectedPoint.z - scaledWidth);
        // the edge of the brush that's furthest from 0 on the terrain's x-axis
        float maxBrushX = Mathf.Ceil(affectedPoint.z + scaledWidth);

        // the edge of the brush that's closest to 0 on the terrain's y-axis
        float minBrushY = Mathf.Ceil(affectedPoint.x - scaledDepth);
        // the edge of the brush that's furthest from 0 on the terrain's y-axis
        float maxBrushY = Mathf.Ceil(affectedPoint.x + scaledDepth);

        // change the height data of each vertice
        for (int x = Mathf.Max(Mathf.RoundToInt(minBrushX), 0); x < Mathf.Min(maxBrushX, terrainRes); x++)
        {
            for (int y = Mathf.Max(Mathf.RoundToInt(minBrushY), 0); y < Mathf.Min(maxBrushY, terrainRes); y++)
            {
                // checks if the vertice is within the affected area
                if (VerticeInAffectedArea(x, y, affectedPoint, scaledWidth, scaledDepth))
                {
                    if (isCircle)
                    {
                        float distance = Vector2.Distance(new Vector2(x, y), new Vector2(affectedPoint.z, affectedPoint.x));
                        // if the vertice's distance is within the radius 
                        if (distance <= (radius * ((float)terrainRes / 100))*2)
                        {
                            // sets the new height of the vertice
                            newHeights[x, y] = affectedPoint.y;
                        }
                    }
                    else 
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


    public void ChangeTerrainHeightRamp(Vector3 affectedPoint, float scaledWidth, float scaledDepth, RampDirectionType rampDirection)
    {
        //direction is where the ramp is facing
        // 0 == North, 1 == West, 2 == South, 3 == East
        if ((int)rampDirection % 2 == 1)
        {
            float storeWidth = scaledWidth;
            scaledWidth = scaledDepth;
            scaledDepth = storeWidth;
        }

        int reverseDir = 0;
        // reverses the direction of the slope if facing West or South
        if (rampDirection == RampDirectionType.West || rampDirection == RampDirectionType.South)
        {
            reverseDir = -1;
        }

        Debug.Log("heloo2!");
        int terrainRes = terrain.terrainData.heightmapResolution;
        float[,] newHeights = terrain.terrainData.GetHeights(0, 0, terrainRes, terrainRes);

        // the edge of the brush that's closest to 0 on the terrain's x-axis
        float minBrushX = Mathf.Ceil(affectedPoint.z - scaledWidth);
        // the edge of the brush that's furthest from 0 on the terrain's x-axis
        float maxBrushX = Mathf.Ceil(affectedPoint.z + scaledWidth);

        // the edge of the brush that's closest to 0 on the terrain's y-axis
        float minBrushY = Mathf.Ceil(affectedPoint.x - scaledDepth);
        // the edge of the brush that's furthest from 0 on the terrain's y-axis
        float maxBrushY = Mathf.Ceil(affectedPoint.x + scaledDepth);

        // change the height data of each vertice
        for (int x = Mathf.Max(Mathf.RoundToInt(minBrushX), 0); x < Mathf.Min(maxBrushX, terrainRes); x++)
        {
            for (int y = Mathf.Max(Mathf.RoundToInt(minBrushY), 0); y < Mathf.Min(maxBrushY, terrainRes); y++)
            {
                // checks if the vertice is within the affected area
                if (VerticeInAffectedArea(x, y, affectedPoint, scaledWidth, scaledDepth))
                {
                    // how far the value 'x' is on the ramp
                    // subtracts by the starting edge of the ramp on the terrain
                    float terrainDistOnRamp;
                    // how long is the terrain from edge to edge
                    float terrainRampTotalDist;

                    if (rampDirection == RampDirectionType.West || rampDirection == RampDirectionType.East)
                    {
                        terrainDistOnRamp = y - minBrushY;
                        terrainRampTotalDist = maxBrushY - minBrushY;
                    }
                    else
                    {
                        terrainDistOnRamp = x - minBrushX;
                        terrainRampTotalDist = maxBrushX - minBrushX;
                    }

                    // the height to be added to set the new height of the vertice
                    // must convert to terrain values since height on a terrain map is between 0 and 1
                    float addedHeight = WorldPointToTerrainPoint(Vector3.up * height).y;

                    // multiply addedHeight by fraction to get slope
                    // reverseDir will reverse the direction of the slope (low will be high and vice versa) if the direction is South or West
                    // removes any negative values from reverseDir using Mathf.Abs
                    float slopedHeight = addedHeight * Mathf.Abs(reverseDir + (terrainDistOnRamp / terrainRampTotalDist));

                    // sets the new height of the vertice
                    newHeights[x, y] = affectedPoint.y + slopedHeight;

                }
            }
        }

        // sets the terrain height to the changed heights
        terrain.terrainData.SetHeights(0, 0, newHeights);
    }


    
    bool VerticeInAffectedArea(int terrainX, int terrainY, Vector3 affectedPoint, float scaledWidth, float scaledDepth)
    {
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

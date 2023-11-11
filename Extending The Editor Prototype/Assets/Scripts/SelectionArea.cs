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
    North,
    West,
    South,
    East
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

        float scaledWidth = width * ((float)terrainResolution / 100);
        float scaledDepth = depth * ((float)terrainResolution / 100);
        float scaledRadius = radius * ((float)terrainResolution / 100);

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
                        if (VerticeInAffectedArea(x, y, affectedPoint, scaledRadius, scaledRadius))
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
                        if (VerticeInAffectedArea(x, y, affectedPoint, scaledWidth, scaledDepth))
                        {
                            // sets the new height of the vertice
                            newHeights[x, y] = affectedPoint.y;
                        }
                    }
                }
                if (brushType == BrushType.Ramp)
                {
                    // the height to be added to set the new height of the vertice
                    // must convert to terrain values since height on a terrain map is between 0 and 1
                    float addedHeight = WorldPointToTerrainPoint(Vector3.up * height).y;

                    if (rampDirection == RampDirectionType.North || rampDirection == RampDirectionType.South)
                    {
                        // checks if the vertice is within the affected area
                        if (VerticeInAffectedArea(x, y, affectedPoint, scaledWidth, scaledDepth))
                        {
                            // how far the value 'x' is on the ramp
                            // subtracts by the starting edge of the ramp on the terrain
                            float terrainDistOnRamp = x - Mathf.CeilToInt(affectedPoint.z - scaledWidth);
                            // how long is the terrain from edge to edge
                            float terrainRampTotalDist = (affectedPoint.z + scaledWidth) - (affectedPoint.z - scaledWidth);

                            if (rampDirection == RampDirectionType.North)
                            {
                                // sets the new height of the vertice
                                // multiply addedHeight by fraction to get slope
                                newHeights[x, y] = affectedPoint.y + (addedHeight * (terrainDistOnRamp / terrainRampTotalDist));
                            }
                            else if (rampDirection == RampDirectionType.South)
                            {
                                // sets the new height of the vertice
                                // multiply addedHeight by fraction to get slope
                                newHeights[x, y] = affectedPoint.y + (addedHeight * (1-(terrainDistOnRamp / terrainRampTotalDist)));
                            }



                        }
                    }
                    else if (rampDirection == RampDirectionType.West || rampDirection == RampDirectionType.East)
                    {
                        // checks if the vertice is within the affected area
                        // scaledDepth and scaledWidth are flipped to rotate the ramp
                        if (VerticeInAffectedArea(x, y, affectedPoint, scaledDepth, scaledWidth))
                        {
                            // how far the value 'x' is on the ramp
                            // subtracts by the starting edge of the ramp on the terrain
                            float terrainDistOnRamp = y - (affectedPoint.x - (scaledWidth));
                            // how long is the terrain from edge to edge
                            float terrainRampTotalDist = (affectedPoint.x + scaledWidth) - (affectedPoint.x - scaledWidth);

                            if (rampDirection == RampDirectionType.East)
                            {
                                // sets the new height of the vertice
                                // multiply addedHeight by fraction to get slope
                                newHeights[x, y] = affectedPoint.y + (addedHeight * (terrainDistOnRamp / terrainRampTotalDist));
                            }
                            else if (rampDirection == RampDirectionType.West)
                            {
                                // sets the new height of the vertice
                                // multiply addedHeight by fraction to get slope
                                newHeights[x, y] = affectedPoint.y + (addedHeight * (1-(terrainDistOnRamp / terrainRampTotalDist)));
                            }
                        }
                    }
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

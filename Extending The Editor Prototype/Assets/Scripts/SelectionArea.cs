using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// SelectionArea.cs is responsible for
// - modifying the height of the terrain within an area
// - storing the variables changed from the editor script

// note: this script cannot communicate with its Editor script, as any references to an editor script will break projects once built

public enum BrushType
{
    Plateau,
    Ramp,
    Bell
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

public class BrushData
{
    public Vector3 affectedPoint;
    public float scaledWidth = 1;
    public float scaledDepth = 1;
    public float height = 1;
    
}

public class SelectionArea : MonoBehaviour
{
    [SerializeField]
    public Terrain terrain;

    // most variables are public yet hidden in the inspector as the Editor script is responsible for displaying the relavent variables for the selected brush type

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
    public AnimationCurve bellCurve;

    [HideInInspector]
    public Vector3[] gizmoArray;
    [HideInInspector]
    public Vector3[] verticeArray;

    [HideInInspector]
    public int index = 0;

    // world to terrain units conversion
    private float terrainScaler;
    private int terrainRes;

    public void Start()
    {
        BrushOrderController.AddBrushToList(this);
    }

    // called by BrushOrderController.cs
    public void StartBrush()
    {
        // attempts to get the terrain data to set the terrain resolution
        // if it fails due to the terrain field being empty, it will be caught and logged as a warning in the console
        try
        {
            int terrainRes = terrain.terrainData.heightmapResolution;
            terrainScaler = (float)terrainRes / 100;

            IdentifyTerrainBrush();
        }
        catch 
        {
            Debug.LogWarningFormat("The Terrain field on object \'"+ name + "\' has not been filled in. This brush will not modify any terrain until a terrain to modify has been selected.");
        }

        // runs the next brush in the cue in BrushOrderController.cs
        BrushOrderController.RunNextBrush();
    }

    void ClearTerrainHeight()
    {
        terrainRes = terrain.terrainData.heightmapResolution;
        float[,] newHeights = terrain.terrainData.GetHeights(0, 0, terrainRes, terrainRes);

        for (int x = 0; x < terrainRes; x++)
        {
            for (int y = 0; y < terrainRes; y++)
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
        // creates a new brush data and sets the affected point
        BrushData brushData = new BrushData();
        brushData.affectedPoint = WorldPointToTerrainPoint(transform.position);

        switch (brushType)
        {
            case BrushType.Plateau:
                switch (plateauType)
                {
                    case PlateauType.Circular:
                        // sets the other brush data used for this type of brush
                        brushData.scaledWidth = radius * terrainScaler * 2;
                        brushData.scaledDepth = radius * terrainScaler * 2;
                        brushData.height = 0;

                        // height and animation curves are not used
                        ModifyHeightsCircleBase(brushData, new AnimationCurve(), true);
                        break;


                    case PlateauType.Rectangular:
                        // sets the other brush data used for this type of brush
                        brushData.scaledWidth = width * terrainScaler;
                        brushData.scaledDepth = depth * terrainScaler;
                        brushData.height = 0;

                        // height and animation curves are not used
                        ModifyHeightsCircleBase(brushData, new AnimationCurve(), false);
                        break;
                }
                break;


            case BrushType.Ramp:
                // sets the other brush data used for this type of brush
                brushData.scaledWidth = width * terrainScaler;
                brushData.scaledDepth = depth * terrainScaler;
                brushData.height = height;

                ChangeTerrainHeightRamp(brushData, rampDirection);
                break;


            case BrushType.Bell:
                // sets the other brush data used for this type of brush
                brushData.scaledWidth = radius * terrainScaler * 2;
                brushData.scaledDepth = radius * terrainScaler * 2;
                brushData.height = height;

                ModifyHeightsCircleBase(brushData, bellCurve, true);
                break;
        }
    }

    // 
    public void ModifyHeightsCircleBase(BrushData brushData, AnimationCurve animCurve, bool isCircle = false)
    {
        terrainRes = terrain.terrainData.heightmapResolution;
        float[,] newHeights = terrain.terrainData.GetHeights(0, 0, terrainRes, terrainRes);

        // the edge of the brush that's closest to 0 on the terrain's x-axis
        float minBrushX = Mathf.Ceil(brushData.affectedPoint.z - brushData.scaledWidth);
        // the edge of the brush that's furthest from 0 on the terrain's x-axis
        float maxBrushX = Mathf.Ceil(brushData.affectedPoint.z + brushData.scaledWidth);

        // the edge of the brush that's closest to 0 on the terrain's y-axis
        float minBrushY = Mathf.Ceil(brushData.affectedPoint.x - brushData.scaledDepth);
        // the edge of the brush that's furthest from 0 on the terrain's y-axis
        float maxBrushY = Mathf.Ceil(brushData.affectedPoint.x + brushData.scaledDepth);

        // change the height data of each vertice
        for (int x = Mathf.Max(Mathf.RoundToInt(minBrushX), 0); x < Mathf.Min(maxBrushX, terrainRes); x++)
        {
            for (int y = Mathf.Max(Mathf.RoundToInt(minBrushY), 0); y < Mathf.Min(maxBrushY, terrainRes); y++)
            {
                // checks if the vertice is within the affected area
                if (VerticeInAffectedArea(x, y, brushData.affectedPoint, brushData.scaledWidth, brushData.scaledDepth))
                {
                    if (isCircle)
                    {
                        float distance = Vector2.Distance(new Vector2(x, y), new Vector2(brushData.affectedPoint.z, brushData.affectedPoint.x));

                        // radius when converted from terrain units back into unity units
                        // scaledWidth is a radius value when isCircle is true 
                        float claculatedRadius = (brushData.scaledWidth * ((float)terrainRes / 100)) / terrainScaler;

                        // if the vertice's distance is within the radius 
                        if (distance <= claculatedRadius)
                        {
                            // the added height of the vertice
                            float evaluatedCurve = animCurve.Evaluate(distance / claculatedRadius);

                            // adds extra height to the vertice's new height using the evaluatedCurve
                            // Mathf.Min and Max are used to keep the value between 0 and 1
                            float extraHeight = Mathf.Max(Mathf.Min(evaluatedCurve, 1), 0)/ 200 * height;

                            // sets the new height of the vertice
                            newHeights[x, y] = brushData.affectedPoint.y + extraHeight;
                        }
                    }
                    else 
                    {
                        // sets the new height of the vertice
                        newHeights[x, y] = brushData.affectedPoint.y;
                    }
                }
            }
        }

        // sets the terrain height to the changed heights
        terrain.terrainData.SetHeights(0, 0, newHeights);
    }


    public void ChangeTerrainHeightRamp(BrushData brushData, RampDirectionType rampDirection)
    {
        //direction is where the ramp is facing
        // 0 == North, 1 == West, 2 == South, 3 == East
        if ((int)rampDirection % 2 == 1)
        {
            float storeWidth = brushData.scaledWidth;
            brushData.scaledWidth = brushData.scaledDepth;
            brushData.scaledDepth = storeWidth;
        }

        int reverseDir = 0;
        // reverses the direction of the slope if facing West or South
        if (rampDirection == RampDirectionType.West || rampDirection == RampDirectionType.South)
        {
            reverseDir = -1;
        }

        terrainRes = terrain.terrainData.heightmapResolution;
        float[,] newHeights = terrain.terrainData.GetHeights(0, 0, terrainRes, terrainRes);

        // the edge of the brush that's closest to 0 on the terrain's x-axis
        float minBrushX = Mathf.Ceil(brushData.affectedPoint.z - brushData.scaledWidth);
        // the edge of the brush that's furthest from 0 on the terrain's x-axis
        float maxBrushX = Mathf.Ceil(brushData.affectedPoint.z + brushData.scaledWidth);

        // the edge of the brush that's closest to 0 on the terrain's y-axis
        float minBrushY = Mathf.Ceil(brushData.affectedPoint.x - brushData.scaledDepth);
        // the edge of the brush that's furthest from 0 on the terrain's y-axis
        float maxBrushY = Mathf.Ceil(brushData.affectedPoint.x + brushData.scaledDepth);

        // change the height data of each vertice
        for (int x = Mathf.Max(Mathf.RoundToInt(minBrushX), 0); x < Mathf.Min(maxBrushX, terrainRes); x++)
        {
            for (int y = Mathf.Max(Mathf.RoundToInt(minBrushY), 0); y < Mathf.Min(maxBrushY, terrainRes); y++)
            {
                // checks if the vertice is within the affected area
                if (VerticeInAffectedArea(x, y, brushData.affectedPoint, brushData.scaledWidth, brushData.scaledDepth))
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
                    newHeights[x, y] = brushData.affectedPoint.y + slopedHeight;

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

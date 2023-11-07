using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



[CustomEditor(typeof(SelectionArea))]
[CanEditMultipleObjects]
public class SelectionAreaEditor : Editor
{
    SelectionArea areaTarget;

    
    private BrushType brushType;        // an enum of the different types of brushes that can be selected. all the brush types can be edited in the SelectionArea script
    private PlateauType plateauType;    // an enum of the different types of plateau brushes. can be edited in the SelectionArea script

    int detail = 32;    // how many points are used for creating a circle array
    private float radius;
    private float width;
    private float height;
    private float depth;

    private void OnEnable()
    {
        areaTarget = (SelectionArea)target;
        radius = areaTarget.radius;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();


        // creates and renders the selection type enum in the inspector and sets the type to what was selected
        brushType = areaTarget.brushType;
        brushType = (BrushType)EditorGUILayout.EnumPopup("Selection Type", brushType);
        if (brushType == BrushType.Plateau)
        {

            // creates and renders the plateau type enum in the inspector and sets the type to what was selected
            plateauType = areaTarget.plateauType;
            plateauType = (PlateauType)EditorGUILayout.EnumPopup("Plateau Type", plateauType);

            if (plateauType == PlateauType.Circular)
            {
                // creates and renders the radius slider in the inspector and sets it to what was selected
                radius = areaTarget.radius;
                radius = EditorGUILayout.Slider("Radius", radius, 0.1f, 10);

                areaTarget.gizmoArray = CreateCircleArray();

            }
            if (plateauType == PlateauType.Rectangular)
            {
                // creates and renders the width slider in the inspector and sets it to what was selected
                width = areaTarget.width;
                width = EditorGUILayout.Slider("Width", width, 0.1f, 20);

                // creates and renders the depth slider in the inspector and sets it to what was selected
                depth = areaTarget.depth;
                depth = EditorGUILayout.Slider("Depth", depth, 0.1f, 20);

                areaTarget.gizmoArray = CreateRectArray();
            }
        }
        else if (brushType == BrushType.Ramp)
        {
            // creates and renders the width slider in the inspector and sets it to what was selected
            width = areaTarget.width;
            width = EditorGUILayout.Slider("Width", width, 0.1f, 20);

            // creates and renders the depth slider in the inspector and sets it to what was selected
            depth = areaTarget.depth;
            depth = EditorGUILayout.Slider("Depth", depth, 0.1f, 20);

            // creates and renders the height slider in the inspector and sets it to what was selected
            height = areaTarget.height;
            height = EditorGUILayout.Slider("Height", height, 0.1f, 20);

            areaTarget.gizmoArray = CreateRampArray();
        }
        // checks if any GUI elements have changed
        if (EditorGUI.EndChangeCheck())
        {
            // updates all changes made in the inspector
            UpdateVariables();
            // repaints the scene
            SceneView.RepaintAll();
        }
    }

    public void UpdateVariables()
    {
        Debug.Log("Updating Variables...");
        // sets all changes made in the inspector to the script
        areaTarget.brushType = brushType;
        areaTarget.plateauType = plateauType;
        areaTarget.radius = radius;
        areaTarget.width = width;
        areaTarget.height = height;
        areaTarget.depth = depth;

        
    }

    public Vector3[] CreateCircleArray()
    {
        // creates an array of vertices used to draw a circle
        // height and degreeRotation is not used, so they're set to 0
        areaTarget.verticeArray = CreateVerticeArray(areaTarget.transform, 32, radius * 2, radius * 2, 0, 0);

        List<Vector3> circleList = new List<Vector3>();
        circleList.AddRange(areaTarget.verticeArray);

        //adds the last vertice and the first vertice as a pair to close the circle
        circleList.Add(circleList[circleList.Count - 1]);
        circleList.Add(circleList[0]);

        return circleList.ToArray();
    }

    public Vector3[] CreateRectArray()
    {
        // creates an array of vertices used to draw a rectangle
        // CreateVerticeArray() will treat width and depth as a radius and create a circle, causing it to shrink in size and be rotated 45 degrees in the wrong direction
        // to fix the issue, multiply width and depth by the square root of 2 to increase the range to the appropriate level and set degreeRotation to 45
        // height is not used, so it's set to 0
        areaTarget.verticeArray = CreateVerticeArray(areaTarget.transform, 4, depth * Mathf.Sqrt(2), width * Mathf.Sqrt(2), 0, 45);

        List<Vector3> rectList = new List<Vector3>();
        rectList.AddRange(areaTarget.verticeArray);

        //adds the last vertice and the first vertice as a pair to close the rectangle
        rectList.Add(rectList[rectList.Count - 1]);
        rectList.Add(rectList[0]);

        return rectList.ToArray();
    }

    public Vector3[] CreateRampArray(int startingCorner = 0)
    {
        List<Vector3> rampList = new List<Vector3>();

        // creates the base rectangle of the ramp
        rampList.AddRange(CreateRectArray());

        // creates the rest of the ramp
        for (int vertNum = 0; vertNum < 2; vertNum++)
        {
            for (int pairPartner = 0; pairPartner < 2; pairPartner++)
            {
                // creates the vertical gizmo lines
                // gets a vertice already present in the rampList
                // uses modulo to loop through the 2nd, 4th, 6th, and 8th indexes of the array, preventing an array overflow
                // vertNum is the next vertice
                Vector3 newVert = rampList[(((startingCorner+vertNum) %4)*2)];
                newVert.y += height * pairPartner;

                // add vertice to list
                rampList.Add(newVert);

                // if the vertice just created is elevated above the base
                if (pairPartner == 1)
                {
                    // add vertice to list
                    rampList.Add(newVert);

                    // creates the diagonal gizmo lines
                    // gets a vertice already present in the rampList
                    // uses modulo to loop through the 2nd, 4th, 6th, and 8th indexes of the array, preventing an array overflow
                    // vertNum is the next vertice
                    // adds 3 - (vertNum*2) to set the end points of the gizmo lines to the verticies in parallel to the starting vertice
                    newVert = rampList[(((startingCorner + vertNum + (3 - (vertNum*2))) % 4) * 2)];
                    // add the new vertice to list
                    rampList.Add(newVert);
                }
            }
        }
        rampList.Add(rampList[11]);
        rampList.Add(rampList[15]);

        return rampList.ToArray();
    }

    public Vector3[] CreateVerticeArray(Transform objectTransform, int detail, float depth, float width, float height, float degreeRotation)
    {
        List<Vector3> vertList = new List<Vector3>();

        // create vertices and add to rectList
        for (int vertNum = 0; vertNum < detail; vertNum++)
        {
            for (int pairPartner = 0; pairPartner < 2; pairPartner++)
            {
                Vector3 newVert = objectTransform.position;

                // sets the offset in the local right, forward, and up directions of the vertice
                newVert += objectTransform.right * Mathf.Cos(Mathf.Deg2Rad * (360 / detail * (vertNum + pairPartner) + degreeRotation)) * depth;
                newVert += objectTransform.forward * Mathf.Sin(Mathf.Deg2Rad * (360 / detail * (vertNum + pairPartner) + degreeRotation)) * width;
                newVert += objectTransform.up * height;

                // adds vertice to list
                vertList.Add(newVert);
            }
        }
        return vertList.ToArray();
    }















    [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]
    static void DrawGizmos(SelectionArea selectionArea, GizmoType gizmoType)
    {
        Gizmos.color = Color.blue;

        // draws a line from the circle array
        Gizmos.DrawLineList(selectionArea.gizmoArray);
        
    }

}

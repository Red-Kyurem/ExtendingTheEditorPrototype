using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



[CustomEditor(typeof(SelectionArea))]
[CanEditMultipleObjects]
public class SelectionAreaEditor : Editor
{
    SelectionArea areaTarget;

    
    private BrushType brushType;                // an enum of the different types of brushes that can be selected. all the brush types can be edited in the SelectionArea script
    private PlateauType plateauType;            // an enum of the different types of plateau brushes. can be edited in the SelectionArea script
    private RampDirectionType rampDirection;    // an enum of the cardinal directions for the ramp brush. can be edited in the SelectionArea script

    int detail = 32;    // how many points are used for creating a circle array
    private float radius;
    private float width;
    private float height;
    private float depth;

    private AnimationCurve bellCurve;
    private int bellCurveDetail = 10;
    private int bellCurves = 4;

    private int index = 0;

    private void OnEnable()
    {
        areaTarget = (SelectionArea)target;
        radius = areaTarget.radius;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        // remembers any GUI element changes for checking if any variables were changed
        EditorGUI.BeginChangeCheck();

        // if the terrain field is empty in the non-editor script, display a warning message
        if (areaTarget.terrain == null)
        { 
            EditorGUILayout.HelpBox("The Terrain field must be filled in, otherwise this brush will not modify any terrain.", MessageType.Warning); 
        }

        // creates and renders the selection type enum in the inspector and sets the type to what was selected
        brushType = areaTarget.brushType;
        brushType = (BrushType)EditorGUILayout.EnumPopup("Brush Type", brushType);
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

                areaTarget.gizmoArray = CreateCircleArray(radius, 0);

            }
            if (plateauType == PlateauType.Rectangular)
            {
                // creates and renders the width slider in the inspector and sets it to what was selected
                width = areaTarget.width;
                width = EditorGUILayout.Slider("Width", width, 0.1f, 20);

                // creates and renders the depth slider in the inspector and sets it to what was selected
                depth = areaTarget.depth;
                depth = EditorGUILayout.Slider("Depth", depth, 0.1f, 20);

                areaTarget.gizmoArray = CreateRectArray(width, depth);
            }
        }
        else if (brushType == BrushType.Ramp)
        {
            // creates and renders the ramp direction type enum in the inspector and sets the type to what was selected
            rampDirection = areaTarget.rampDirection;
            rampDirection = (RampDirectionType)EditorGUILayout.EnumPopup("Ramp Direction", rampDirection);

            // creates and renders the width slider in the inspector and sets it to what was selected
            width = areaTarget.width;
            width = EditorGUILayout.Slider("Width", width, 0.1f, 20);

            // creates and renders the depth slider in the inspector and sets it to what was selected
            depth = areaTarget.depth;
            depth = EditorGUILayout.Slider("Depth", depth, 0.1f, 20);

            // creates and renders the height slider in the inspector and sets it to what was selected
            height = areaTarget.height;
            height = EditorGUILayout.Slider("Height", height, 0.1f, 20);

            areaTarget.gizmoArray = CreateRampArray(width, depth, (int)rampDirection);
        }
        else if (brushType == BrushType.Bell)
        {
            // creates and renders the radius slider in the inspector and sets it to what was selected
            radius = areaTarget.radius;
            radius = EditorGUILayout.Slider("Radius", radius, 0.1f, 10);

            // creates and renders the height slider in the inspector and sets it to what was selected
            height = areaTarget.height;
            height = EditorGUILayout.Slider("Height", height, 0.1f, 20);


            // creates and renders the Animation Curve editor in the inspector and sets it to what was selected
            bellCurve = areaTarget.bellCurve;
            bellCurve = EditorGUILayout.CurveField("Bell Curve", bellCurve, Color.green, ranges: new Rect(0, 0, 1, 1));

            // creates and renders the bellCurveDetail float as a slider in the inspector
            bellCurveDetail = Mathf.RoundToInt(EditorGUILayout.Slider("Bell Curve Detail", bellCurveDetail, 3, 20));

            // creates and renders the bellCurves float as a slider in the inspector
            bellCurves = Mathf.RoundToInt(EditorGUILayout.Slider("Bell Curves", bellCurves, 3, 8));

            // finds all curve keys present in the animation curve and returns a Vector2
            // x is the time of when the key is located (0 to 1)
            // y is the value of the key (0 to 1)
            Vector2[] curveKeyPos = FindAnimCurveKeyPositions(bellCurve);

            areaTarget.gizmoArray = CreateBellArray(radius, height, curveKeyPos);
        }

        index = areaTarget.index;
        index = EditorGUILayout.IntField("Index", index);

        // checks if any GUI elements have changed since EditorGUI.BeginChangeCheck()
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
        areaTarget.rampDirection = rampDirection;
        areaTarget.radius = radius;
        areaTarget.width = width;
        areaTarget.height = height;
        areaTarget.depth = depth;
        areaTarget.index = index;
    }

    // creates an array for an outline of a circle
    public Vector3[] CreateCircleArray(float radius, float height)
    {
        // creates an array of vertices used to draw a circle
        // height and degreeRotation is not used, so they're set to 0
        areaTarget.verticeArray = CreateClosedVerticeArray(areaTarget.transform, 32, radius * 2, radius * 2, height, 0);

        List<Vector3> circleList = new List<Vector3>();
        circleList.AddRange(areaTarget.verticeArray);

        //adds the last vertice and the first vertice as a pair to close the circle
        circleList.Add(circleList[circleList.Count - 1]);
        circleList.Add(circleList[0]);

        return circleList.ToArray();
    }

    // creates an array for an outline of a rectangle
    public Vector3[] CreateRectArray(float width, float depth)
    {
        // creates an array of vertices used to draw a rectangle
        // CreateVerticeArray() will treat width and depth as a radius and create a circle, causing it to shrink in size and be rotated 45 degrees in the wrong direction
        // to fix the issue, multiply width and depth by the square root of 2 to increase the range to the appropriate level and set degreeRotation to 45
        // height is not used, so it's set to 0
        areaTarget.verticeArray = CreateClosedVerticeArray(areaTarget.transform, 4, depth * Mathf.Sqrt(2), width * Mathf.Sqrt(2), 0, 45);

        List<Vector3> rectList = new List<Vector3>();
        rectList.AddRange(areaTarget.verticeArray);

        //adds the last vertice and the first vertice as a pair to close the rectangle
        rectList.Add(rectList[rectList.Count - 1]);
        rectList.Add(rectList[0]);

        return rectList.ToArray();
    }

    // creates an array for an outline of a ramp
    public Vector3[] CreateRampArray(float width, float depth, int startingCorner = 0)
    {

        if (startingCorner % 2 == 1)
        {
            float storeWidth = width;
            width = depth;
            depth = storeWidth;
        }

        List<Vector3> rampList = new List<Vector3>();

        // creates the base rectangle of the ramp
        rampList.AddRange(CreateRectArray(width, depth));

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


    // creates an array for an outline of a bell
    public Vector3[] CreateBellArray(float radius, float height, Vector2[] curveKeyPos)
    {
        List<Vector3> bellList = new List<Vector3>();
        Transform objectTransform = areaTarget.transform;

        // for every curve that will make up the bell
        for (int bellCurveNum = 0; bellCurveNum < bellCurves; bellCurveNum++)
        {
            // the direction the bell curve will point towards
            Vector3 direction = Vector3.zero;
            direction.x = Mathf.Sin(Mathf.Deg2Rad * (360 / bellCurves + ((360 / bellCurves) * bellCurveNum)));
            direction.z = Mathf.Cos(Mathf.Deg2Rad * (360 / bellCurves + ((360 / bellCurves) * bellCurveNum)));

            // how much of the curve has been completed
            float percentComplete = 0;

            // creates gizmo lines in the shape of the slope using the curveKeyPos
            for (int keyNum = 0; keyNum < curveKeyPos.Length - 1; keyNum++)
            {
                // if the first curve key does not start at 0,
                // then create lines to connect the first curve key to the center on the same y-axis
                if (curveKeyPos[0].x != 0 && keyNum == 0)
                {
                    // to create a line, 2 Vector3 vertices must be created
                    for (int pairPartner = 0; pairPartner < 2; pairPartner++)
                    {
                        Vector3 newVert = objectTransform.position;

                        // the magnitude of the vertice (how far the line will be from the center)
                        float vertMagnitude = curveKeyPos[keyNum].x * pairPartner * radius * 2;

                        // sets the offset in the right, forward, and up directions of the vertice
                        newVert += objectTransform.right * vertMagnitude * direction.x;
                        newVert += objectTransform.up * height;
                        newVert += objectTransform.forward * vertMagnitude * direction.z;

                        // adds the vertice to the list
                        bellList.Add(newVert);
                    }
                    percentComplete += curveKeyPos[0].x;
                }

                // finds the linear distance between two curve keys
                float distance = Vector2.Distance(curveKeyPos[keyNum], curveKeyPos[keyNum + 1]);
                // finds the distance between two curve keys on the x-axis
                float addedPercent = curveKeyPos[keyNum + 1].x - curveKeyPos[keyNum].x;
                // the number of lines to create for the space between curve keys
                int numOfLines = Mathf.CeilToInt(distance * bellCurveDetail);

                for (int lineNum = 0; lineNum < numOfLines; lineNum++)
                {
                    // to create a line, 2 Vector3 vertices must be created
                    for (int pairPartner = 0; pairPartner < 2; pairPartner++)
                    {
                        Vector3 newVert = objectTransform.position;

                        // the magnitude of the vertice (how far the line will be from the center)
                        float vertMagnitude = Mathf.Lerp(curveKeyPos[keyNum].x, curveKeyPos[keyNum + 1].x, (lineNum + pairPartner) / (float)numOfLines) * radius * 2;
                        // how high the vertice will be
                        float vertHeight = bellCurve.Evaluate(Mathf.Lerp(percentComplete, percentComplete + addedPercent, (lineNum + pairPartner) / (float)numOfLines)) ;

                        // sets the offset in the right, forward, and up directions of the vertice
                        newVert += objectTransform.right * vertMagnitude * direction.x;
                        newVert += objectTransform.forward * vertMagnitude * direction.z;
                        // Mathf.Min and Max are used to keep the value between 0 and 1
                        newVert += objectTransform.up * Mathf.Max(Mathf.Min(vertHeight, 1), 0) * height;

                        // adds the vertice to the list
                        bellList.Add(newVert);
                    }
                }
                percentComplete += addedPercent;
            }
        }

        // for every curve key after the first one
        for (int keyNum = 1; keyNum < curveKeyPos.Length; keyNum++)
        {
            // creates a circle for the bell at the curveKeyPosition
            bellList.AddRange(CreateCircleArray(radius*curveKeyPos[keyNum].x, curveKeyPos[keyNum].y * height));
        }
            
        return bellList.ToArray();
    }

    // creates a closed loop array of vertices
    public Vector3[] CreateClosedVerticeArray(Transform objectTransform, int detail, float depth, float width, float height, float degreeRotation)
    {
        List<Vector3> vertList = new List<Vector3>();

        // create vertices and add to rectList
        for (int vertNum = 0; vertNum < detail; vertNum++)
        {
            for (int pairPartner = 0; pairPartner < 2; pairPartner++)
            {
                Vector3 newVert = objectTransform.position;

                // sets the offset in the right, forward, and up directions of the vertice
                newVert += objectTransform.right * Mathf.Cos(Mathf.Deg2Rad * (360 / detail * (vertNum + pairPartner) + degreeRotation)) * depth;
                newVert += objectTransform.forward * Mathf.Sin(Mathf.Deg2Rad * (360 / detail * (vertNum + pairPartner) + degreeRotation)) * width;
                newVert += objectTransform.up * height;

                // adds vertice to list
                vertList.Add(newVert);
            }
        }
        return vertList.ToArray();
    }

    public Vector2[] FindAnimCurveKeyPositions(AnimationCurve animCurve)
    {
        List<Vector2> animCurveKeysPos = new List<Vector2>();

        for (int curveNum = 0; curveNum < animCurve.length; curveNum++)
        {
            // finds the animation curve's position on the graph
            // time is on the x-axis
            // value is on the y-axis
            animCurveKeysPos.Add(new Vector2(animCurve[curveNum].time, animCurve[curveNum].value));
        }

        return animCurveKeysPos.ToArray();
    }

    [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]
    static void DrawGizmos(SelectionArea selectionArea, GizmoType gizmoType)
    {
        Gizmos.color = Color.blue;

        // draws a line from the circle array
        Gizmos.DrawLineList(selectionArea.gizmoArray);
        
    }

}

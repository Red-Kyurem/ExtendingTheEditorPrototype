using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



[CustomEditor(typeof(SelectionArea))]
[CanEditMultipleObjects]
public class SelectionAreaEditor : Editor
{
    SelectionArea areaTarget;

    
    private SelectionType selectionType;
    private PlateauType plateauType;
    int detail = 32;
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
        selectionType = areaTarget.selectionType;
        selectionType = (SelectionType)EditorGUILayout.EnumPopup("Selection Type", selectionType);
        if (selectionType == SelectionType.Plateau)
        {

            // creates and renders the plateau type enum in the inspector and sets the type to what was selected
            plateauType = areaTarget.plateauType;
            plateauType = (PlateauType)EditorGUILayout.EnumPopup("Plateau Type", plateauType);
            
            if (plateauType == PlateauType.Circular)
            {
                // creates and renders the radius slider in the inspector and sets it to what was selected
                radius = areaTarget.radius;
                radius = EditorGUILayout.Slider("Radius", radius, 0.1f, 20);

                // creates an array of points used to draw a circle
                areaTarget.listArray = CreateCircleArray(areaTarget.transform);
            }
            if (plateauType == PlateauType.Rectangular)
            {
                // creates and renders the width slider in the inspector and sets it to what was selected
                width = areaTarget.width;
                width = EditorGUILayout.Slider("Width", width, 0.1f, 20);

                // creates and renders the height slider in the inspector and sets it to what was selected
                depth = areaTarget.depth;
                depth = EditorGUILayout.Slider("Depth", depth, 0.1f, 20);

                // creates an array of points used to draw a rectangle
                areaTarget.listArray = CreateRectArray(areaTarget.transform);
            }
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
        // sets all changes made in the inspector to the script
        areaTarget.selectionType = selectionType;
        areaTarget.plateauType = plateauType;
        areaTarget.radius = radius;
        areaTarget.width = width;
        areaTarget.height = height;
        areaTarget.depth = depth;
    }

    public Vector3[] CreateCircleArray(Transform objectTransform)
    {
    
        List<Vector3> circleList = new List<Vector3>();

        // create points and add to circleList
        // detail is how many points will be present in the list
        for (int pointNum = 0; pointNum < detail; pointNum++)
        {
            // Gizmos.DrawLineList requires a pair of points to render a line segment properly
            for (int pairPartner = 0; pairPartner < 2; pairPartner++)
            {
                Vector3 newPoint = objectTransform.position;
                // sets the offset in the local right and local forward directions of the point
                // offsets keep an equal radius when rotated
                // Cos and Sin make the point placement circular
                newPoint += objectTransform.right * Mathf.Cos(Mathf.Deg2Rad * (360 / detail * (pointNum + pairPartner))) * radius;
                newPoint += objectTransform.forward * Mathf.Sin(Mathf.Deg2Rad * (360 / detail * (pointNum + pairPartner))) * radius;

                // adds point to list
                circleList.Add(newPoint);
            }
        }
        //adds the last point and the first point as a pair to close the circle
        circleList.Add(circleList[circleList.Count - 1]);
        circleList.Add(circleList[0]);

        return circleList.ToArray();
    }

    public Vector3[] CreateRectArray(Transform objectTransform)
    {
        List<Vector3> rectList = new List<Vector3>();

        // create points and add to rectList
        for (int pointNum = 0; pointNum < 4; pointNum++)
        {

            // Gizmos.DrawLineList requires a pair of points to render a line segment properly
            for (int pairPartner = 0; pairPartner < 2; pairPartner++)
            {
                Vector3 newPoint = objectTransform.position;
                // sets the offset in the local right and local forward directions of the point
                // offsets are equal when rotated

                newPoint += objectTransform.right * Mathf.Cos(Mathf.Deg2Rad * (360 / 4 * (pointNum + pairPartner) + 45)) * width * Mathf.Sqrt(2);
                newPoint += objectTransform.forward * Mathf.Sin(Mathf.Deg2Rad * (360 / 4 * (pointNum + pairPartner) + 45)) * depth * Mathf.Sqrt(2);

                // adds point to list
                rectList.Add(newPoint);
            }
        }
        return rectList.ToArray();
    }















        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]
    static void DrawGizmos(SelectionArea selectionArea, GizmoType gizmoType)
    {
        // draws a line from the circle array
        Gizmos.DrawLineList(selectionArea.listArray);
        
    }

}

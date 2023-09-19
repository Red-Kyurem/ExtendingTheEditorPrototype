using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum SelectionType
{
    Plateau

}
public enum PlateauType
{
    Circular
}

[CustomEditor(typeof(SelectionArea))]
[CanEditMultipleObjects]
public class SelectionAreaEditor : Editor
{
    SelectionArea areaTarget;

    private SelectionType selectionType;
    private PlateauType plateauType;
    private float radius = 1;


    private void OnEnable()
    {
        areaTarget = (SelectionArea)target;
        
    }

    public override void OnInspectorGUI()
    {
        // creates an array of points used to draw a circle
        areaTarget.circleArray = CreateCircleArray(areaTarget.transform);

        // creates and renders the selection type enum in the inspector and sets the type to what was selected
        selectionType = (SelectionType)EditorGUILayout.EnumPopup("Selection Type", selectionType);
        if (selectionType == SelectionType.Plateau)
        {
            // creates and renders the plateau type enum in the inspector and sets the type to what was selected
            plateauType = (PlateauType)EditorGUILayout.EnumPopup("Plateau Type", plateauType);
            if (plateauType == PlateauType.Circular)
            {
                // creates and renders the radius slider in the inspector and sets it to what was selected
                radius = EditorGUILayout.Slider("Radius", radius, 0.1f, 20);
                areaTarget.radius = radius;
            }
        }
        // checks if any GUI elements have changed and repaints the scene
        if (EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
        }
    }

    public Vector3[] CreateCircleArray(Transform objectTransform)
    {
        // how detailed the circle will be
        int detail = 32;

        List<Vector3> circleList = new List<Vector3>();

        // create points and add to circleList
        // detail is how many points will be present in the list
        for (int pointNum = 0; pointNum < detail; pointNum++)
        {
            // Gizmos.DrawLineList requires a pair of points to render a line segment properly
            for (int pairPartner = 0; pairPartner < 2; pairPartner++)
            {
                Vector3 newPoint = objectTransform.position;
                // sets the offset in the local left (inverted right) and local forward directions of the point
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

    

    [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]
    static void DrawGizmos(SelectionArea selectionArea, GizmoType gizmoType)
    {
        // draws a line from the circle array
        Gizmos.DrawLineList(selectionArea.circleArray);
        
    }

}

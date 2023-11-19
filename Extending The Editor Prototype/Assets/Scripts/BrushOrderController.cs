using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BrushOrderController 
{
    static List<SelectionArea> brushes = new List<SelectionArea>();
    static Queue<SelectionArea> brushQueue = new Queue<SelectionArea>();

    public static void AddBrushToList(SelectionArea newBrush)
    {
        // finds all game objects with a selection area script attached
        SelectionArea[] brushArray = Object.FindObjectsOfType<SelectionArea>();

        // sorts brushArray and sets the result into brushes list
        brushes = InsertionSorter(brushArray);

        foreach (SelectionArea brush in brushes)
        {
            // queue each brush in order of appearance in the list
            brushQueue.Enqueue(brush);
            Debug.Log("Name: " + brush.gameObject.name);
        }

        // runs the next brush to modify terrain
        RunNextBrush();
    }

    // runs the next brush to modify terrain
    // is also called by the brush that has finished modifying the terrain
    public static void RunNextBrush()
    {
        // stops running if there is no more brushes in the queue
        if (brushQueue.Count == 0)
        {
            Debug.Log("No more brushes in queue!");
            return;
        }

        SelectionArea brush = brushQueue.Dequeue();
        // starts the brush
        brush.StartBrush();
    }

    // sorts the specified array using the insertion sorting method
    static List<SelectionArea> InsertionSorter(SelectionArea[] array)
    {
        int arraySize = array.Length;

        // index is how far in the array is
        for (int index = 0; index < arraySize-1; index++)
        {
            // backwatdsCount will count ahead of the index by 1 back to 0
            for (int backwardsCount = index+1; backwardsCount > 0; backwardsCount--)
            {
                // if the next brush's index is greater than the current brush's index
                if (array[backwardsCount-1].index > array[backwardsCount].index)
                {
                    // swap the places of each brush
                    SelectionArea temp = array[backwardsCount - 1];
                    array[backwardsCount - 1] = array[backwardsCount];
                    array[backwardsCount] = temp;
                }
            }
        }

        // return sorted brushes as a list
        return new List<SelectionArea>(array);
    }

    
}

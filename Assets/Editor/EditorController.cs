//used to do things in Editor time, like prepare the scenario
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof (GameController))]
public class EditorController : Editor {
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var rect = GUILayoutUtility.GetRect(500, 40);

        //pre process for set all cells and obstacles
        if (GUI.Button(rect, "Pre-Process"))
        {
            PreProccess();
        }

        rect = GUILayoutUtility.GetRect(500, 40);
        if (GUI.Button(rect, "Fix FD File"))
        {
            FixFDFile();
        }

        rect = GUILayoutUtility.GetRect(500, 40);
        if (GUI.Button(rect, "Calculate Metrics"))
        {
            CalculateMetrics();
        }
    }

    public void PreProccess() {
        if ((target as GameController).loadConfigFile)
        {
            (target as GameController).ClearScene();
            (target as GameController).LoadCellsAuxins();
        }
        else
        {
            //(target as GameController).ClearScene();
            //(target as GameController).DrawObstacles();
            //(target as GameController).DrawCells();
            //(target as GameController).PlaceAuxins();
            //(target as GameController).DrawGoals();
            //(target as GameController).CalculateAllPaths();
        }
    }

    public void FixFDFile()
    {
        (target as GameController).FixFDFile();
    }

    public void CalculateMetrics()
    {
        (target as GameController).CalculateMetrics();
    }
}

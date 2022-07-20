using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(StageEditor))]
public class StageEditorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        StageEditor myTarget = (StageEditor)target;
        if (GUILayout.Button("Reload"))
        {
            if (Application.isPlaying)
            {
                myTarget.StageEditorReload();
            }
            else
            {
                Debug.Log("Can't reload actors when not in play mode!");
            }
        }
        if (GUILayout.Button("Start/Restart"))
        {
            if (Application.isPlaying)
            {
                myTarget.StageEditorStart();
            }
            else
            {
                Debug.Log("Can't spawn actors when not in play mode!");
            }
        }
        if (GUILayout.Button("Stop"))
        {
            if (Application.isPlaying)
            {
                myTarget.StageEditorStop();
            }
            else
            {
                Debug.Log("Can't stop actors when not in play mode!");
            }
        }
    }
}

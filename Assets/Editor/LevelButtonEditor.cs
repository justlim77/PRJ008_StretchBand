using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine.UI;

[CustomEditor(typeof(LevelButton))]
public class LevelButtonEditor : ButtonEditor
{
    public override void OnInspectorGUI()
    {
        LevelButton component = (LevelButton)target;

        base.OnInspectorGUI();

        component.LoadingRing = (Image)EditorGUILayout.ObjectField("Loading Ring", component.LoadingRing, typeof(Image), true);
        component.LevelToLoad = EditorGUILayout.TextField("Level To Load", component.LevelToLoad);
    }
}

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelGenerator))]
public class MapEditor : Editor{

    public override void OnInspectorGUI()
    {
       base.OnInspectorGUI();

       LevelGenerator map = target as LevelGenerator;

       map.GenerateLevel();
    }
}

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CustomEditor(typeof(FlowFieldController))]
public class FlowFieldEditor : Editor
{
#if UNITY_EDITOR
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        FlowFieldController controller = (FlowFieldController)target;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Flow Field Kontrolleri", EditorStyles.boldLabel);

        if (GUILayout.Button("🟢 Create FlowField"))
        {
            controller.CreateFlowField();
        }

        if (GUILayout.Button("🧹 Clear FlowField"))
        {
            controller.ClearFlowField();
        }
    }
#endif
}
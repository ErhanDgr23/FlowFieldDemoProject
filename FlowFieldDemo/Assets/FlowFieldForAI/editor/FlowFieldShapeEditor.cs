using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class FlowFieldShapeEditor : MonoBehaviour
{
    public List<Vector2> vertices = new List<Vector2>();

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (vertices.Count < 2) return;
        Gizmos.color = new Color(1, 0, 0, 0.1f);
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 current = transform.TransformPoint(vertices[i]);
            Vector3 next = transform.TransformPoint(vertices[(i + 1) % vertices.Count]);
            Gizmos.DrawLine(current, next);
        }
    }

    [CustomEditor(typeof(FlowFieldShapeEditor))]
    public class FlowFieldShapeEditorInspector : Editor
    {
        private void OnSceneGUI()
        {
            FlowFieldShapeEditor shape = (FlowFieldShapeEditor)target;
            for (int i = 0; i < shape.vertices.Count; i++)
            {
                Vector3 worldPos = shape.transform.TransformPoint(shape.vertices[i]);
                Vector3 newWorldPos = Handles.PositionHandle(worldPos, Quaternion.identity);
                if (newWorldPos != worldPos)
                {
                    Undo.RecordObject(shape, "Move Vertex");
                    shape.vertices[i] = shape.transform.InverseTransformPoint(newWorldPos);
                }
            }
        }
    }
#endif
}

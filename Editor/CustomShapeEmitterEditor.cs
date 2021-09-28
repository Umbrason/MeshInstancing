using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CustomShapeEmitter))]
public class CustomShapeEmitterEditor : Editor
{
    private Dictionary<CustomShapeEmitter, Dictionary<int, Dictionary<int, bool>>> vertexSelectionDictionary = new Dictionary<CustomShapeEmitter, Dictionary<int, Dictionary<int, bool>>>();
    private Dictionary<CustomShapeEmitter, Dictionary<int, (int, int)>> lineSelectionDictionary = new Dictionary<CustomShapeEmitter, Dictionary<int, (int, int)>>();
    public void OnSceneGUI()
    {
        HandleUtility.AddDefaultControl(0);
        var emitter = target as CustomShapeEmitter;
        DrawEmitterHandles(emitter);
        if (targets != null)
            foreach (var Object in targets)
                DrawEmitterHandles((Object as GameObject)?.GetComponent<CustomShapeEmitter>());
    }

    private void DrawEmitterHandles(CustomShapeEmitter emitter)
    {
        if (!emitter)
            return;
        Undo.RecordObject(emitter, "emitter");
        for (var i = 0; i < emitter.shapes.Count; i++)
            DrawCustomShapeHandles(emitter, i);
    }

    private void DrawCustomShapeHandles(CustomShapeEmitter shapeEmitter, int shapeIndex)
    {
        var vertexEmitterShapeDict = vertexSelectionDictionary.ContainsKey(shapeEmitter) ? vertexSelectionDictionary[shapeEmitter] : vertexSelectionDictionary[shapeEmitter] = new Dictionary<int, Dictionary<int, bool>>() { { shapeIndex, new Dictionary<int, bool>() } };
        var vertexDict = vertexEmitterShapeDict.ContainsKey(shapeIndex) ? vertexEmitterShapeDict[shapeIndex] : vertexEmitterShapeDict[shapeIndex] = new Dictionary<int, bool>();
        var lineEmitterShapeDict = lineSelectionDictionary.ContainsKey(shapeEmitter) ? lineSelectionDictionary[shapeEmitter] : lineSelectionDictionary[shapeEmitter] = new Dictionary<int, (int, int)>() { { shapeIndex, (-1, -1) } };
        var shape = shapeEmitter.shapes[shapeIndex];
        Handles.color = new Color(.3f, .5f, .5f, .2f);
        Handles.DrawAAConvexPolygon(shape.vertices.ToArray());

        for (int i = 0; i < shape.triangleIndices.Count; i += 3)
        {
            var A = shape.vertices[shape.triangleIndices[i + 0]];
            var B = shape.vertices[shape.triangleIndices[i + 1]];
            var C = shape.vertices[shape.triangleIndices[i + 2]];
            var screenPoint = Event.current.mousePosition;
            screenPoint.y = SceneView.lastActiveSceneView.camera.pixelHeight - screenPoint.y;
            var intersection = Vector3.zero;
            lineEmitterShapeDict[shapeIndex] = (-1, -1);
            if (LineScreenPointIntersection(A, B, screenPoint, out intersection))
            {
                lineEmitterShapeDict[shapeIndex] = (i + 0, i + 1);
                Handles.color = new Color(.6f, .7f, .7f);
                var q = Quaternion.LookRotation(intersection - SceneView.currentDrawingSceneView.camera.transform.position);
                Handles.DotHandleCap(0, intersection, q, HandleUtility.GetHandleSize(intersection) * .05f, EventType.Repaint);
            }
            if (LineScreenPointIntersection(B, C, screenPoint, out intersection))
            {
                lineEmitterShapeDict[shapeIndex] = (i + 1, i + 2);
                Handles.color = new Color(.6f, .7f, .7f);
                var q = Quaternion.LookRotation(intersection - SceneView.currentDrawingSceneView.camera.transform.position);
                Handles.DotHandleCap(0, intersection, q, HandleUtility.GetHandleSize(intersection) * .05f, EventType.Repaint);
            }
            if (LineScreenPointIntersection(C, A, screenPoint, out intersection))
            {
                lineEmitterShapeDict[shapeIndex] = (i + 2, i + 0);
                Handles.color = new Color(.6f, .7f, .7f);
                var q = Quaternion.LookRotation(intersection - SceneView.currentDrawingSceneView.camera.transform.position);
                Handles.DotHandleCap(0, intersection, q, HandleUtility.GetHandleSize(intersection) * .05f, EventType.Repaint);
            }

        }
        for (int i = 0; i < shape.triangleIndices.Count; i += 3)
        {
            var A = shape.vertices[shape.triangleIndices[i + 0]];
            var B = shape.vertices[shape.triangleIndices[i + 1]];
            var C = shape.vertices[shape.triangleIndices[i + 2]];
            
            Handles.color = lineEmitterShapeDict[shapeIndex] == (i + 0, i + 1) ? new Color(.3f, .8f, .8f) : new Color(.3f, .5f, .5f);
            Handles.DrawAAPolyLine(A, B);
            Handles.color = lineEmitterShapeDict[shapeIndex] == (i + 1, i + 2) ? new Color(.3f, .8f, .8f) : new Color(.3f, .5f, .5f);
            Handles.DrawAAPolyLine(B, C);
            Handles.color = lineEmitterShapeDict[shapeIndex] == (i + 2, i + 0) ? new Color(.3f, .8f, .8f) : new Color(.3f, .5f, .5f);
            Handles.DrawAAPolyLine(C, A);
        }
        for (int i = 0; i < shape.vertices.Count; i++)
        {
            var vertex = shape.vertices[i];
            var q = Quaternion.LookRotation(shape.vertices[i] - SceneView.currentDrawingSceneView.camera.transform.position);
            var isSelected = vertexDict.ContainsKey(i) ? vertexDict[i] : vertexDict[i] = false;
            Handles.color = isSelected ? new Color(.4f, .9f, .7f) : new Color(.3f, .4f, .5f);
            var size = HandleUtility.GetHandleSize(vertex) * .05f;
            if (Handles.Button(vertex, q, size, size * .8f, Handles.DotHandleCap))
            {
                if (!Event.current.shift)
                    vertexDict.Clear();
                vertexDict[i] = true;
            }
            if (vertexDict[i])
            {
                var newPos = Handles.PositionHandle(shape.vertices[i], Quaternion.identity);
                var delta = newPos - shape.vertices[i];
                for (int j = 0; j < shape.vertices.Count; j++)
                    if (vertexDict.ContainsKey(j) && vertexDict[j])
                        shape.vertices[j] += delta;
            }
        }
    }

    private bool LineScreenPointIntersection(Vector3 A, Vector3 B, Vector2 screenPoint, out Vector3 point)
    {
        var ray = SceneView.lastActiveSceneView.camera.ScreenPointToRay(screenPoint);
        var C = ray.origin;
        var D = ray.origin + ray.direction * SceneView.lastActiveSceneView.camera.farClipPlane;
        if (Utility.MouseDistanceToLine(A, B) <= 5f)
            return Utility.ClosestPointsOnTwoLines(out point, out var useless, A, (B - A).normalized, C, (D - C).normalized);
        point = default;
        return false;
    }


}

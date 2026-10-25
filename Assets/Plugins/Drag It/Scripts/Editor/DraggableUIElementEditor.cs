#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace RyujinInteractive.CustomUI
{
/// <summary>
/// Adds runtime guidance and Edit Mode controls to the DraggableUIElement inspector.
/// The action buttons are available only while the game is running.
/// </summary>
[CustomEditor(typeof(DraggableUIElement))]
public class DraggableUIElementEditor : Editor
{
    /// <summary>Draws the default component fields, usage notes, and runtime action buttons.</summary>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        bool changed = DrawDefaultInspector();
        serializedObject.ApplyModifiedProperties();

        DraggableUIElement draggableElement = (DraggableUIElement)target;
        if (changed && Application.isPlaying)
        {
            // Route inspector changes through the property so raycast settings update immediately.
            draggableElement.EditMode = draggableElement.EditMode;
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Enable Edit Mode to drag this UI element within its parent. Disable Edit Mode to lock it in place.",
            MessageType.Info);
        EditorGUILayout.HelpBox(
            "Snap Step (Pixels) sets the grid spacing while dragging. Set it to 0 to turn snapping off.",
            MessageType.Info);
        EditorGUILayout.HelpBox(
            "In a running game, use these buttons to enter or exit Edit Mode. You can also call EnableEditMode() and DisableEditMode() from your own UI buttons or scripts.",
            MessageType.None);

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Enable Edit Mode"))
                draggableElement.EnableEditMode();
            if (GUILayout.Button("Disable Edit Mode"))
                draggableElement.DisableEditMode();
            EditorGUILayout.EndHorizontal();
        }

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("The Edit Mode action buttons become available in Play Mode.", MessageType.Warning);
        }
    }
}
}
#endif

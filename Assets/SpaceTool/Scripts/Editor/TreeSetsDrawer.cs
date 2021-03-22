using UnityEngine;
using UnityEditor;

/*
    [CustomPropertyDrawer(typeof(TreeSets))]
public class TreeSetsDrawer : PropertyDrawer
{
    Rect r = Rect.zero;
    SerializedProperty[] t = new SerializedProperty[7];
    bool isActive = false;

    void SetSerializedProperty(SerializedProperty property)
    {
        t[0] = property.FindPropertyRelative("scale1");
        t[1] = property.FindPropertyRelative("offset1X");
        t[2] = property.FindPropertyRelative("offset1Y");
        t[3] = property.FindPropertyRelative("scale2");
        t[4] = property.FindPropertyRelative("offset2X");
        t[5] = property.FindPropertyRelative("offset2Y");
        t[6] = property.FindPropertyRelative("biomeTrees");
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        SetSerializedProperty(property);
        isActive = EditorGUILayout.Foldout(isActive, "Tree Sets");
        if (isActive)
        {
            EditorGUI.indentLevel++;
            DrawData("Offset and scale 1", 0);
            DrawData("Offset and scale 2", 3);
            EditorGUILayout.PropertyField(t[6]);
            EditorGUI.indentLevel--;
        }
        EditorGUI.EndProperty();
    }

    void DrawData(string name, int index)
    {
        SetLabel(name);
        r.x -= 15;
        r.width = (r.width + 15) / 3;
        EditorGUI.LabelField(r, "Scale");
        r.x += 40;
        t[index].floatValue = EditorGUI.FloatField(r, t[index].floatValue);
        r.x = r.x + r.width - 25;
        EditorGUI.LabelField(r, "X");
        r.x += 15;
        t[index + 1].floatValue = EditorGUI.FloatField(r, t[index + 1].floatValue);
        r.x = r.x + r.width - 25;
        EditorGUI.LabelField(r, "Y");
        r.x += 15;
        t[index + 2].floatValue = EditorGUI.FloatField(r, t[index + 2].floatValue);
    }

    void SetLabel(string name)
    {
        r = EditorGUILayout.GetControlRect(true, 16);
        r = EditorGUI.PrefixLabel(r, new GUIContent(name));
        r.x -= 15;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 0;
    }

}



 */


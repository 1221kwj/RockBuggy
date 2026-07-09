using RGSK;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RaceUI))]
public class RaceUI_Editor : Editor
{

    RaceUI m_target;

    public void OnEnable()
    {
        //    m_target = (RaceUI)target;
    }


    public override void OnInspectorGUI()
    {

        //LOGO
        Texture logo = (Texture)Resources.Load("EditorUI/RGSKLogo");
        GUILayout.Label(logo, GUILayout.Height(50));

        DrawDefaultInspector();
    }
}

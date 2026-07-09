using RGSK;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

[CustomEditor(typeof(PathCreator))]
public class Path_Creator_Editor : Editor
{
    PathCreator m_target;
    RaycastHit hit;

	AnimBool createPath;

	bool bChangeGUILayout = false;
	bool bToggleOnOff;

    public void OnEnable()
    {
        m_target = (PathCreator)target;
		createPath = new AnimBool(false);
	}

    public override void OnInspectorGUI()
    {
        //LOGO
        Texture logo = (Texture)Resources.Load("EditorUI/RGSKLogo");
        GUILayout.Label(logo, GUILayout.Height(50));

        GUILayout.BeginVertical("Box");
        GUILayout.Box("Path Creation", EditorStyles.boldLabel);
        EditorGUILayout.Space();

		EditorGUI.BeginDisabledGroup(bChangeGUILayout == true);
		{
			m_target.raceMode = (PathCreator.RaceMode)EditorGUILayout.EnumPopup("Select Race Mode", m_target.raceMode);

			if (m_target.raceMode != PathCreator.RaceMode.None && GUILayout.Button("Start Create Path"))
			{
				bChangeGUILayout = true;
				createPath.target = true;
			}
		}
		EditorGUI.EndDisabledGroup();

		if (EditorGUILayout.BeginFadeGroup(createPath.faded))
		{
			EditorGUILayout.HelpBox("This component will help you visually create a path around your track.\n\nEnabling 'Node Layout Mode' will allow you to place nodes by clicking. \n\nClick the 'Finish' button when you are done.", MessageType.Info);

			m_target.layoutMode = EditorGUILayout.Toggle("Node Layout Mode", m_target.layoutMode);
			
			//Ground em' all!
			if (GUILayout.Button("Align Nodes To Ground"))
			{
				m_target.AlignToGround();
			}

			//Delete the last placed node
			if (GUILayout.Button("Delete Last Node"))
			{
				m_target.DeleteLastNode();
			}

			//Finish
			if (GUILayout.Button("Finish"))
			{
				CreateWaypointCircuit();
			}
		}
		EditorGUILayout.EndFadeGroup();

        GUILayout.EndVertical();
    }

    void OnSceneGUI()
    {
        //Handle UI
        Handles.BeginGUI();

        Rect outRect = new Rect(Screen.width - 250, Screen.height - 100, 200, 50);

        GUILayout.BeginArea(new Rect(outRect));
        GUILayout.BeginVertical("Box");
        GUILayout.Box("Node Layout Mode : " + m_target.layoutMode, EditorStyles.boldLabel);
        string s = (m_target.layoutMode) ? "Disable" : "Enable";
        if (GUILayout.Button(s))
        {
            m_target.layoutMode = !m_target.layoutMode;
        }
        GUILayout.EndVertical();
        GUILayout.EndArea();

        Handles.EndGUI();

        //Handles.Label(m_target.transform.position, "Path");

        //Layout Mode
        if (m_target.layoutMode)
        {
            if (Event.current.type == EventType.MouseDown)
            {

                Event e = Event.current;


                if (e.button == 0)
                {
                    //Make sure we cant click anythingelse
                    int controlID = GUIUtility.GetControlID(FocusType.Passive);
                    GUIUtility.hotControl = controlID;
                    e.Use();

                    //Create a new node at clicked pos
                    Ray sceneRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

                    if (Physics.Raycast(sceneRay, out hit, 1000))
                    {
                        GameObject newNode = new GameObject("Node");
                        newNode.transform.position = hit.point;
                        newNode.transform.parent = m_target.transform;
                    }
                }
                else
                {
                    //Reset hot control
                    GUIUtility.hotControl = 0;
                }
            }
        }
    }

    public void CreateWaypointCircuit()
    {
        WaypointCircuit circuit = m_target.gameObject.AddComponent<WaypointCircuit>();
        circuit.AddWaypointsFromChildren();
        circuit.RaceMode = m_target.raceMode;
        DestroyImmediate(m_target.gameObject.GetComponent<PathCreator>());
    }
}

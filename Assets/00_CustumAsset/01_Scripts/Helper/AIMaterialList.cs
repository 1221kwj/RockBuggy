using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMaterialList : MonoBehaviour
{
	public List<Material> matList;
	[SerializeField] private SkinnedMeshRenderer AI_Renderer;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void ChangeMaterial()
	{
		int randomNum = Random.Range(0, 5);

		if (AI_Renderer != null)
		{
			Material[] mat = AI_Renderer.materials;
			mat[0] = matList[randomNum];
		}
	}
}

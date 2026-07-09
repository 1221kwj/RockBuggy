using System.Collections.Generic;

using UnityEngine;

public class AudienceAnim : MonoBehaviour
{
	private List<string> _animName = new List<string>()
	{
		"idle",
		"applause",
		"applause2",
		"celebration",
		"celebration2",
		"celebration3"
	};

	private Animation _anim;

	private int _currentAnimCount = 0;
	private int _totalAnimCount = 0;


    private void Start()
    {
		_anim = GetComponent<Animation>();
		_totalAnimCount = _anim.GetClipCount();
		_currentAnimCount = Random.Range(0, _totalAnimCount);
		_anim.Play(_animName[_currentAnimCount]);
    }

    // Update is called once per frame
    private void Update()
    {
        if (_anim.isPlaying == false)
		{
			_currentAnimCount = Random.Range(0, _totalAnimCount);
			_anim.Play(_animName[_currentAnimCount]);
		}
    }
}

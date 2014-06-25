using UnityEngine;

public class StarWarsIntro : MonoBehaviour
{
	void Update()
	{
		transform.position += transform.up * Time.deltaTime * 1.5f;
	}
}

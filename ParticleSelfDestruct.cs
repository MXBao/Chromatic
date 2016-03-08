using UnityEngine;
using System.Collections;

public class ParticleSelfDestruct : MonoBehaviour {

	void Start () {
		Destroy (gameObject, 1.5f);
	}
}

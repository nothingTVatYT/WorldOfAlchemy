using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ProceduralRock : MonoBehaviour {

	[SerializeField] float distance = 0f;
	[SerializeField] int seed = 0;

	// Use this for initialization
	void Start () {
		DestortMesh();
	}
	
	void DestortMesh() {
		Random.InitState(seed);
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		// displace vertices
		Vector3[] newVertices = new Vector3[mesh.vertices.Length];
		float r = 0.5f;
		for (int i = 0; i < mesh.vertices.Length; i++) {
			Vector3 vertex = mesh.vertices[i];
			Vector3 normal = mesh.normals[i];
			float polar = Mathf.Acos(vertex.y / r); // 0 - PI
			float azimut = Mathf.Atan2(vertex.z, vertex.x); // -PI - PI
			Vector3 newVertex = normal * r * (Mathf.PerlinNoise(polar / Mathf.PI, (azimut + Mathf.PI)/Mathf.PI/2) * distance - distance/2);
			newVertices[i] = newVertex;
		}
		mesh.vertices = newVertices;
		mesh.RecalculateNormals();
	}

	/// <summary>
	/// Called when the script is loaded or a value is changed in the
	/// inspector (Called in the editor only).
	/// </summary>
	void OnValidate()
	{
		DestortMesh();
	}
}

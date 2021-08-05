using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Linq;

public class BlenderAssetProcessor : AssetPostprocessor
{
	public bool rotateOnImport = false;

	public void OnPostprocessModel(GameObject obj  )
	{
		if (rotateOnImport) {
			Debug.Log ("fix orientation of blender object");
			//only perform corrections with blender files

			Vector3 rot = obj.transform.rotation.eulerAngles;
			rot.z += 180f;
			obj.transform.rotation = Quaternion.Euler (rot);

			/*
			ModelImporter importer = assetImporter as ModelImporter;
			if (Path.GetExtension (importer.assetPath) == ".blend") {
				RotateObject (obj.transform);
			}
			*/
			//Don't know why we need this...
			//Fixes wrong parent rotation
			//obj.transform.rotation = Quaternion.identity;
		}
	}

	//recursively rotate a object tree individualy
	private void RotateObject(Transform obj  )
	{
		//Vector3 objRotation = obj.eulerAngles;
		//objRotation.x += 90f;
		//obj.eulerAngles = objRotation;

		Vector3 pos = obj.position;
		pos.y = obj.position.z;
		pos.z = obj.position.y;
		obj.position = pos;

		//if a meshFilter is attached, we rotate the vertex mesh data
		MeshFilter meshFilter = obj.GetComponent(typeof(MeshFilter)) as MeshFilter;
		if (meshFilter)
		{
			RotateMesh(meshFilter.sharedMesh);
			//FlipX(meshFilter.sharedMesh);
		}

		//do this too for all our children
		//Casting is done to get rid of implicit downcast errors
		foreach (Transform child in obj)
		{
			RotateObject(child);
		}
	}

	private void FlipX(Mesh mesh) {
		Vector3[] vertices = mesh.vertices;
		for (int i = 0; i < vertices.Length; i++)
			vertices [i].x = -vertices [i].x;
		mesh.vertices = vertices;

		//for each submesh, we invert the order of vertices for all triangles
		//for some reason changing the vertex positions flips all the normals???
		for (int submesh = 0; submesh < mesh.subMeshCount; submesh++)
		{
			int[] triangles = mesh.GetTriangles(submesh);
			for (int i = 0; i < triangles.Length; i += 3)
			{
				int intermediate = triangles[i];
				triangles[i] = triangles[i + 2];
				triangles[i + 2] = intermediate;
			}
			mesh.SetTriangles(triangles, submesh);
		}

		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
	}

	//"rotate" the mesh data
	private void RotateMesh(Mesh mesh)
	{
		int index = 0;

		//switch all vertex z values with y values
		Vector3[] vertices = mesh.vertices;
		for (index = 0; index < vertices.Length; index++)
		{
			vertices[index] = new Vector3(vertices[index].x, vertices[index].z, vertices[index].y);
		}
		mesh.vertices = vertices;

		//for each submesh, we invert the order of vertices for all triangles
		//for some reason changing the vertex positions flips all the normals???
		for (int submesh = 0; submesh < mesh.subMeshCount; submesh++)
		{
			int[] triangles = mesh.GetTriangles(submesh);
			for (index = 0; index < triangles.Length; index += 3)
			{
				int intermediate = triangles[index];
				triangles[index] = triangles[index + 2];
				triangles[index + 2] = intermediate;
			}
			mesh.SetTriangles(triangles, submesh);
		}

		//recalculate other relevant mesh data
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
	}
}
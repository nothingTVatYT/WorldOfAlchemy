using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTransformer : MonoBehaviour
{

	public void ApplyRotation ()
	{
		GameObject go = gameObject;
		MeshFilter mf = go.GetComponent<MeshFilter> ();
		if (mf != null) {
			Mesh mesh = mf.sharedMesh;
			Vector3[] vertices = mesh.vertices;
			Vector3[] verticesInWorldSpace = new Vector3[vertices.Length];
			Quaternion rotation = go.transform.localRotation;
			Matrix4x4 local2world = go.transform.localToWorldMatrix;
			for (int i = 0; i < vertices.Length; i++) {
				verticesInWorldSpace [i] = local2world.MultiplyPoint3x4 (vertices [i]);
			}

			// adapt the transform
			transform.localRotation = Quaternion.identity;

			// calculate new local vertices in local space
			Matrix4x4 world2local = go.transform.worldToLocalMatrix;
			for (int i = 0; i < vertices.Length; i++) {
				vertices [i] = world2local.MultiplyPoint3x4 (verticesInWorldSpace [i]);
			}

			// write back
			mesh.vertices = vertices;
			mesh.RecalculateNormals ();
			mesh.RecalculateBounds ();
		} else {
			Matrix4x4 local2world = go.transform.localToWorldMatrix;
			Quaternion previousRotation = transform.localRotation;
			transform.localRotation = Quaternion.identity;
			Matrix4x4 world2local = go.transform.worldToLocalMatrix;
			//transform.localRotation = previousRotation;
			for (int i = 0; i < transform.childCount; i++) {
				ApplyRotation (go.transform.GetChild (i).gameObject, world2local, local2world);
			}
			transform.localRotation = Quaternion.identity;
		}
	}

	public void ApplyRotation (GameObject go, Matrix4x4 worldToLocal, Matrix4x4 localToWorld)
	{
		MeshFilter mf = go.GetComponent<MeshFilter> ();
		Vector3 newPosition = worldToLocal.MultiplyPoint3x4(localToWorld.MultiplyPoint3x4 (go.transform.localPosition));
		go.transform.localPosition = newPosition;

		if (mf != null) {

			Mesh mesh = mf.sharedMesh;
			Vector3[] vertices = mesh.vertices;
			Vector3[] verticesInWorldSpace = new Vector3[vertices.Length];
			for (int i = 0; i < vertices.Length; i++) {
				verticesInWorldSpace [i] = localToWorld.MultiplyPoint3x4 (vertices [i]);
			}

			// calculate new local vertices in local space
			for (int i = 0; i < vertices.Length; i++) {
				vertices [i] = worldToLocal.MultiplyPoint3x4 (verticesInWorldSpace [i]);
			}

			// write back
			mesh.vertices = vertices;
			mesh.RecalculateNormals ();
			mesh.RecalculateBounds ();
		} else {
			for (int i = 0; i < transform.childCount; i++) {
				ApplyRotation (go.transform.GetChild (i).gameObject, worldToLocal, localToWorld);
			}
		}
	}

}


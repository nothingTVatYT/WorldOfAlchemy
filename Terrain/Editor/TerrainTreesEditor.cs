using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainTreeColliderHelper))]
public class TerrainTreesEditor : Editor {

	public override void OnInspectorGUI() {
		DrawDefaultInspector ();
		if (GUILayout.Button("Copy Tree Colliders")) {
			Terrain terrain = ((TerrainTreeColliderHelper)target).gameObject.GetComponent<Terrain>();
			Transform colliderParent = terrain.transform.Find ("TreeColliders");
			if (colliderParent == null) {
				GameObject g = new GameObject ();
				g.name = "TreeColliders";
				colliderParent = g.transform;
				colliderParent.SetParent (terrain.transform);
				colliderParent.position = terrain.transform.position;
				colliderParent.localPosition = Vector3.zero;
			}
			TreePrototype[] prototypes = terrain.terrainData.treePrototypes;
			TreeInstance[] instances = terrain.terrainData.treeInstances;
			float width = terrain.terrainData.size.x;
			float height = terrain.terrainData.size.z;
			float y = terrain.terrainData.size.y;
			foreach (TreeInstance tree in instances) {
				GameObject treeCollider = Instantiate(prototypes[tree.prototypeIndex].prefab);
				MeshRenderer r = treeCollider.GetComponent<MeshRenderer> ();
				if (r != null)
					r.enabled = false;
				treeCollider.transform.SetParent (colliderParent);
				treeCollider.transform.localPosition = new Vector3 (tree.position.x * width, tree.position.y * y, tree.position.z * height);
				treeCollider.isStatic = true;
			}
		}
	}
}

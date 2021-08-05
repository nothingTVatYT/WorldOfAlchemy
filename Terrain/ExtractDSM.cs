using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtractDSM : MonoBehaviour {

	public Texture2D dsmData;
	public Texture2D newTexture;
	public RectInt areaToCrop;
	public float unitsPerPixel;
	public float additionalHeightScale = 0.1f;
	public float minHeight;
	public float maxHeight;
	public int smoothFactor;
	public Terrain terrain;

	public void Start() {
	}

	public void Crop() {
		int finalSize = terrain.terrainData.heightmapResolution;
		int scaledSize = Mathf.RoundToInt(finalSize / unitsPerPixel);
		areaToCrop.width = scaledSize;
		areaToCrop.height = scaledSize;
		newTexture = new Texture2D (scaledSize, scaledSize);
		newTexture.SetPixels (dsmData.GetPixels (areaToCrop.x, areaToCrop.y, scaledSize, scaledSize));
		newTexture.Apply ();
		TextureScale.Bilinear(newTexture, finalSize, finalSize);
		float[,] heightMap = textureToHeightmap (newTexture, minHeight, maxHeight, terrain.terrainData.heightmapScale.y);
		terrain.terrainData.SetHeights (0, 0, heightMap);
	}

	private float[,] textureToHeightmap(Texture2D texture, float minHeight, float maxHeight, float terrainHeight) {
		float[,] heightMap = new float[texture.width, texture.height];
		Color[] pixelColors = texture.GetPixels ();
		for (int y = 0; y < texture.height; y++)
			for (int x = 0; x < texture.width; x++)
				heightMap [x, y] = (minHeight + (maxHeight - minHeight) * pixelColors [y * texture.width + x].grayscale) / terrainHeight * additionalHeightScale;
		return heightMap;
	}

	public void Analyze() {
		float lowestValue = 1;
		float highestValue = 0;

		Color[] colors = dsmData.GetPixels ();
		foreach (Color c in colors) {
			float grayscale = c.grayscale;
			if (grayscale > highestValue)
				highestValue = grayscale;
			if (grayscale < lowestValue)
				lowestValue = grayscale;
		}
		Debug.Log ("gray scale is between " + lowestValue + " and " + highestValue);
		Debug.Log ("terrain size is " + terrain.terrainData.heightmapResolution + " x " + terrain.terrainData.heightmapResolution);
		Debug.Log ("terrain scale is " + terrain.terrainData.heightmapScale);
	}

	public Texture2D ScaleTexture(Texture src, int width, int height){
		RenderTexture rt = RenderTexture.GetTemporary(width, height);
		Graphics.Blit(src, rt);

		RenderTexture currentActiveRT = RenderTexture.active;
		RenderTexture.active = rt;
		Texture2D tex = new Texture2D(rt.width,rt.height); 

		tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
		tex.Apply();

		RenderTexture.ReleaseTemporary(rt);
		RenderTexture.active = currentActiveRT;

		return tex;
	}


}

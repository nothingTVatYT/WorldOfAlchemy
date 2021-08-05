using System;
using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using Random = UnityEngine.Random;

public class paintTerrain : MonoBehaviour {

	[System.Serializable]
	public struct StitchParameters {
		public float falloffRate;
		public float falloffThreshold;
	}

	[System.Serializable]
	public struct TerraformParameters {
		[Header("Perlin Noise Settings")]
		[Range(0.000f,0.01f)]
		public float bumpiness;
		[Range(0.000f,1.000f)]
		public float damp;
		[Header("Mountain Settings")]
		public int numMountains;
		[Range(0.001f,0.5f)]
		public float heightChange;
		[Range(0.0001f,0.05f)]
		public float sideSlope;
		[Header("Hole Settings")]
		public int numHoles;
		[Range(0.0f,1.0f)]
		public float holeDepth;
		[Range(0.001f,0.5f)]
		public float holeChange;
		[Range(0.0001f,0.05f)]
		public float holeSlope;

		[Header("River Settings")]
		public int numRivers;
		[Range(0.001f,0.05f)]
		public float digDepth;
		[Range(0.001f,1.0f)]
		public float maxDepth;
		[Range(0.0001f,0.05f)]
		public float bankSlope;

		[Header("Rough Settings")]
		[Range(0.000f,0.05f)]
		public float roughAmount;
		[Range(0,6)]
		[Header("Smooth Settings")]
		public int smoothAmount;
	}

	TerrainData terrainData;
	float[,] newHeightData;

	public TerrainSplatMapSettings splatSettings;
	public TerraformParameters terraformParameters;
	public StitchParameters stitching;
	public bool autoRepaint = false;
	public bool onlyAlterSplatForSteepTerrain = true;
	public bool checkOnly = false;
	public Texture2D grayscaleMap;
	public int tileHeightmapSize = 1025;
	public int tileSize = 1024;

	void normalize(float[] v)
	{
		float total = 0;
		for(int i = 0; i < v.Length; i++)
		{
			total += v[i];
		}
		
		for(int i = 0; i < v.Length; i++)
		{
			v[i] /= total;
		}

	}

	public float map(float value, float sMin, float sMax, float mMin, float mMax)
    {
        return (value - sMin) * (mMax - mMin) / (sMax - sMin) + mMin;
    }

	void Mountain(int x, int y, float height, float slope)
    {
		if(x <= 0 || x >= terrainData.heightmapResolution) return; //off x range of map
		if(y <= 0 || y >= terrainData.heightmapResolution) return; //off y range of map
    	if(height <= 0) return; //if hit lowest level
    	if(newHeightData[x,y] >= height) return; //if run into higher elevation
    	newHeightData[x,y] = height;
    	Mountain(x-1, y, height-Random.Range(0.001f,slope), slope);
    	Mountain(x+1, y, height-Random.Range(0.001f,slope), slope);
    	Mountain(x, y-1, height-Random.Range(0.001f,slope), slope);
    	Mountain(x, y+1, height-Random.Range(0.001f,slope), slope);
    }

    void RiverCrawler(int x, int y, float height, float slope)
     {
		if(x < 0 || x >= terrainData.heightmapResolution) return; //off x range of map
		if(y < 0 || y >= terrainData.heightmapResolution) return; //off y range of map
		if(height <= terraformParameters.maxDepth) return; //if hit lowest level
    	if(newHeightData[x,y] <= height) return; //if run into lower elevation

		newHeightData[x,y] = height;
		
		RiverCrawler(x+1, y, height+Random.Range(slope,slope+0.01f), slope);
		RiverCrawler(x-1, y, height+Random.Range(slope,slope+0.01f), slope);
		RiverCrawler(x+1, y+1, height+Random.Range(slope,slope+0.01f), slope);
		RiverCrawler(x-1, y+1, height+Random.Range(slope,slope+0.01f), slope);
		RiverCrawler(x, y-1, height+Random.Range(slope,slope+0.01f), slope);
		RiverCrawler(x, y+1, height+Random.Range(slope,slope+0.01f), slope);
     }

    void Hole(int x, int y, float height, float slope)
    {
		if(x <= 0 || x >= terrainData.heightmapResolution) return; //off x range of map
		if(y <= 0 || y >= terrainData.heightmapResolution) return; //off y range of map
		if(height <= terraformParameters.holeDepth) return; //if hit lowest level
    	if(newHeightData[x,y] <= height) return; //if run into lower elevation
    	newHeightData[x,y] = height;
    	Hole(x-1, y, height+Random.Range(slope,slope+0.01f), slope);
    	Hole(x+1, y, height+Random.Range(slope,slope+0.01f), slope);
    	Hole(x, y-1, height+Random.Range(slope,slope+0.01f), slope);
    	Hole(x, y+1, height+Random.Range(slope,slope+0.01f), slope);
     }

     void ApplyRiver()
     {
		for(int i = 0; i < terraformParameters.numRivers; i++)
     	{
			int cx = Random.Range(10,terrainData.heightmapResolution-10);
			int cy = 0; //Random.Range(10,terrainData.alphamapHeight-10);
			int xdir = Random.Range(-1,2);
			int ydir = Random.Range(-1,2);
			while(cy >= 0 && cy < terrainData.heightmapResolution && cx > 0 && cx < terrainData.heightmapResolution)
			{
				RiverCrawler(cx,cy, newHeightData[cx,cy]-terraformParameters.digDepth, terraformParameters.bankSlope);
				
				if(Random.Range(0,50)<5)
					xdir = Random.Range(-1,2);
				
				if(Random.Range(0,50)<5)
					ydir = Random.Range(0,2);

				cx = cx + xdir;
				cy = cy + ydir;
			}
		}
     }

     void ApplyHoles()
     {
		for(int i = 0; i < terraformParameters.numHoles; i++)
		{
			//height data needs to be between 0 and 1
			int xpos = Random.Range(10,terrainData.heightmapResolution-10);
			int ypos = Random.Range(10,terrainData.heightmapResolution-10);
			float newHeight = newHeightData[xpos,ypos] - terraformParameters.holeChange;
			Hole(xpos,ypos, newHeight, terraformParameters.holeSlope);
		}

     }


     void ApplyMountains()
     {
		for(int i = 0; i < terraformParameters.numMountains; i++)
		{
			//height data needs to be between 0 and 1
			int xpos = Random.Range(10,terrainData.heightmapResolution-10);
			int ypos = Random.Range(10,terrainData.heightmapResolution-10);
			float newHeight = newHeightData[xpos,ypos] + terraformParameters.heightChange;
			Mountain(xpos,ypos, newHeight, terraformParameters.sideSlope);
		}

     }

     
	void RoughTerrain()
     {
		for (int y = 1; y < terrainData.heightmapResolution-1; y++)
		{
			for (int x = 1; x < terrainData.heightmapResolution-1; x++)
		    {
				newHeightData[x,y] += Random.Range(0,terraformParameters.roughAmount);
		    }
		}
     }

     void SmoothTerrain()
     {
		int mh = terrainData.heightmapResolution-1;
		int mw = terrainData.heightmapResolution-1;

		int xm1, xp1, ym1, yp1;
		for (int y = 0; y <= mh; y++)
		{
			for (int x = 0; x <= mw; x++)
		    {
				xm1 = x > 0 ? x - 1 : x;
				ym1 = y > 0 ? y - 1 : y;
				xp1 = x < mw ? x + 1 : x;
				yp1 = y < mh ? y + 1 : y;

		    	float avgheight = (newHeightData[x,y] +
		    						newHeightData[xp1,y] +
		    						newHeightData[xm1,y] +
		    						newHeightData[xp1,yp1] +
		    						newHeightData[xm1,ym1] +
		    						newHeightData[xp1,ym1] +
		    						newHeightData[xm1,yp1] +
		    						newHeightData[x,yp1] +
		    						newHeightData[x,ym1])/9.0f;

		    	newHeightData[x,y] = avgheight;
		    }
		}
     }

     void ApplyPerlin()
     {
		for (int y = 0; y < terrainData.heightmapResolution; y++)
		{
			for (int x = 0; x < terrainData.heightmapResolution; x++)
		    {
				newHeightData[x,y] = Mathf.PerlinNoise(x*terraformParameters.bumpiness,y*terraformParameters.bumpiness)*terraformParameters.damp;
		   	}
		} 
     }

	/*
	public void Stitch() {
		terrainData = gameObject.GetComponent<Terrain>().terrainData;
		TerrainAdditionalData.TerrainRelations neighbors = gameObject.GetComponent<TerrainAdditionalData> ().neighbors;

		float[,] heightmap = terrainData.GetHeights (0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);

		if (neighbors.left != null) {
			Debug.Log ("stitch with terrain to the left");
			neighbors.left.Flush ();
			int verticesMoved = 0;
			TerrainData other = neighbors.left.terrainData;
			float[,] seem = other.GetHeights (0,0, other.heightmapResolution, other.heightmapResolution);
			for (int y = 0; y < terrainData.heightmapResolution; y++) {
				float difference = seem [y, other.heightmapResolution-1] - heightmap [y, 0];
				int myX = 0;
				heightmap [y, myX] += difference;
				verticesMoved++;
				while (Mathf.Abs(difference) > stitching.falloffThreshold) {
					difference *= stitching.falloffRate;
					myX++;
					heightmap [y, myX] += difference;
					verticesMoved++;
				}
			}
			Debug.Log (verticesMoved + " vertices moved.");
		}
		if (neighbors.top != null) {
			TerrainData other = neighbors.top.terrainData;
			float[,] seem = other.GetHeights (0,0, other.heightmapResolution, other.heightmapResolution);
			for (int x = 0; x < terrainData.heightmapResolution; x++) {
				float difference = seem [0, x] - heightmap [terrainData.heightmapResolution-1, x];
				int myY = terrainData.heightmapResolution-1;
				heightmap [myY, x] += difference;
				while (Mathf.Abs(difference) > stitching.falloffThreshold) {
					difference *= stitching.falloffRate;
					myY--;
					heightmap [myY, x] += difference;
				}
			}
		}
		if (neighbors.right != null) {
			TerrainData other = neighbors.right.terrainData;
			float[,] seem = other.GetHeights (0,0, other.heightmapResolution, other.heightmapResolution);
			for (int y = 0; y < terrainData.heightmapResolution; y++) {
				float difference = seem [y, 0] - heightmap [y, terrainData.heightmapResolution-1];
				int myX = terrainData.heightmapResolution-1;
				heightmap [y, myX] += difference;
				while (Mathf.Abs(difference) > stitching.falloffThreshold) {
					difference *= stitching.falloffRate;
					myX--;
					heightmap [y, myX] += difference;
				}
			}
		}
		if (neighbors.bottom != null) {
			TerrainData other = neighbors.bottom.terrainData;
			float[,] seem = other.GetHeights (0,0, other.heightmapResolution, other.heightmapResolution);
			for (int x = 0; x < terrainData.heightmapResolution; x++) {
				float difference = seem [other.heightmapResolution-1, x] - heightmap [0, x];
				int myY = 0;
				heightmap [myY, x] += difference;
				while (Mathf.Abs(difference) > stitching.falloffThreshold) {
					difference *= stitching.falloffRate;
					myY++;
					heightmap [myY, x] += difference;
				}
			}
		}

		terrainData.SetHeights (0, 0, heightmap);
		gameObject.GetComponent<Terrain> ().Flush ();
	} */

	void Start()
	{
		terrainData = gameObject.GetComponent<Terrain>().terrainData;
	}

	public void OnValidate() {
		if (splatSettings != null && !DelegateInfo.IsSubscribed(splatSettings.onSettingsChanged, this)) {
			splatSettings.onSettingsChanged += splatSettingsChanged;
		}
		if (tileHeightmapSize > 0) {
			int n2 = tileHeightmapSize - 1;
			if (Mathf.NextPowerOfTwo(n2) != n2) {
				tileHeightmapSize = Mathf.NextPowerOfTwo (n2) + 1;
			}
		}
	}

	void splatSettingsChanged() {
		if (autoRepaint)
			PaintTextures ();
	}

	public void GenerateFromGrayscale() {
		Texture2D texture = grayscaleMap;
		terrainData = gameObject.GetComponent<Terrain>().terrainData;
		Color[] colors = texture.GetPixels ();
		float[,] heightmap = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
		int h = texture.height;
		int w = texture.width;
		for (int y = 0; y < h; y++) {
			for (int x = 0; x < w; x++) {
				heightmap [y, x] = colors [x + y * texture.width].grayscale;
			}
		}
		terrainData.SetHeights (0, 0, heightmap);
	}

	/*
	public void SetupNeighborhood() {
		Terrain[] allTerrains = FindObjectsOfType<Terrain> ();
		Debug.Log ("Found " + allTerrains.Length + " terrains.");
		int tilesOneAxis = (int)Mathf.Sqrt (allTerrains.Length);
		Terrain[,] terrainTiles = new Terrain[tilesOneAxis, tilesOneAxis];
		String pattern = "Terrain-(\\d+)-(\\d+)";
		foreach(Terrain t in allTerrains) {
			Match match = Regex.Match (t.name, pattern);
			int tileX = int.Parse(match.Groups [1].Value);
			int tileY = int.Parse(match.Groups [2].Value);
			terrainTiles [tileY, tileX] = t;
			//paintTerrain pt = t.GetComponent<paintTerrain> (); pt.stitching.falloffRate = pt.stitchingFalloff; pt.stitching.falloffThreshold = 0.01f;
		}
		setupNeighbors (terrainTiles);
	}

	void setupNeighbors(Terrain[,] terrainTiles) {
		int terrainsInY = terrainTiles.GetLength(0);
		int terrainsInX = terrainTiles.GetLength(1);
		for (int y = 0; y < terrainsInY; y++)
			for (int x = 0; x < terrainsInX; x++) {
				Terrain left = x > 0 ? terrainTiles [y, x - 1] : null;
				Terrain top = y < terrainsInY - 1 ? terrainTiles [y + 1, x] : null;
				Terrain right = x < terrainsInX - 1 ? terrainTiles [y, x + 1] : null;
				Terrain bottom = y > 0 ? terrainTiles [y - 1, x] : null;
				terrainTiles[y, x].SetNeighbors(left, top, right, bottom);
				TerrainAdditionalData ad = terrainTiles [y, x].GetComponent<TerrainAdditionalData> ();
				if (ad == null)
					ad = terrainTiles [y, x].gameObject.AddComponent<TerrainAdditionalData> ();
				ad.neighbors.Set (left, top, right, bottom);
				ad.tileX = x;
				ad.tileY = y;
			}
	}
	*/

	public void SplitTerrain() {
		StartCoroutine (_SplitTerrain());
	}

	IEnumerator _SplitTerrain() {
		terrainData = gameObject.GetComponent<Terrain>().terrainData;
		int bigWidth = terrainData.heightmapResolution;
		int bigHeight = terrainData.heightmapResolution;
		float[,] bigHeightmap = terrainData.GetHeights (0, 0, bigWidth, bigHeight);
		int tilesOneAxis = (bigWidth - 1) / (tileHeightmapSize - 1);
		Debug.Log ("splitting into " + tilesOneAxis + "x" + tilesOneAxis + " tiles of " + tileHeightmapSize + "x" + tileHeightmapSize);
		Terrain terrainTemplate = gameObject.GetComponent<Terrain> ();
		int terrainHeight = Mathf.RoundToInt(terrainTemplate.terrainData.size.y);
		Debug.Log ("terrainData.size = " + terrainTemplate.terrainData.size);
		Terrain[,] terrainTiles = new Terrain[tilesOneAxis, tilesOneAxis];
		for (int tileY=0; tileY < tilesOneAxis; tileY++) {
			for (int tileX=0; tileX < tilesOneAxis; tileX++) {
				Terrain terrainTile = Instantiate (terrainTemplate);
				// that does not work (won't clone alphamaps)
				//terrainTile.terrainData = Instantiate (terrainTemplate.terrainData);
				// instead create a new one and copy properties
				terrainTile.terrainData = new TerrainData();
				CloneTools.CopyProperties (terrainTemplate.terrainData, terrainTile.terrainData);
				terrainTile.transform.position = new Vector3 ((tileX - tilesOneAxis / 2) * tileSize, 0, (tileY - tilesOneAxis/2) * tileSize);
				terrainTile.terrainData.heightmapResolution = tileHeightmapSize;

				Debug.Log ("new alpha map resolution is " + terrainTile.terrainData.alphamapResolution);
				int alphamapSize = terrainTile.terrainData.alphamapResolution;
				// clone alpha maps
				terrainTile.terrainData.SetAlphamaps (0, 0, terrainTile.terrainData.GetAlphamaps (0, 0, alphamapSize, alphamapSize));
				terrainTile.name = "Terrain-" + tileX + "-" + tileY;
				terrainTile.terrainData.name = "TerrainData-" + tileX + "-" + tileY;
				terrainTile.terrainData.size = new Vector3 (tileSize, terrainHeight, tileSize);
				terrainTile.GetComponent<TerrainCollider> ().terrainData = terrainTile.terrainData;
				float[,] tileHeightMap = new float[tileHeightmapSize, tileHeightmapSize];
				for (int y = 0; y < tileHeightmapSize; y++) {
					for (int x = 0; x < tileHeightmapSize; x++) {
						tileHeightMap[y,x] = bigHeightmap[y + tileY * (tileHeightmapSize-1), x + tileX * (tileHeightmapSize-1)];
					}
				}
				terrainTile.terrainData.SetHeights (0, 0, tileHeightMap);
				terrainTile.GetComponent<paintTerrain> ().PaintTextures ();
				terrainTiles [tileY, tileX] = terrainTile;
			}
		}
		//setupNeighbors (terrainTiles);
		return null;
	}

	public void Generate()
	{
		terrainData = gameObject.GetComponent<Terrain>().terrainData;
		newHeightData = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];

		ApplyPerlin();
		RoughTerrain();
		ApplyMountains();
		ApplyHoles();
		ApplyRiver();
		for(int i = 0; i < terraformParameters.smoothAmount; i++)
			SmoothTerrain();
		terrainData.SetHeights(0,0, newHeightData);
		PaintTextures ();
	}

	public void SmoothHeightmap() {
		terrainData = gameObject.GetComponent<Terrain>().terrainData;
		float[,] heightmap = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);
		for (int i = 0; i < terraformParameters.smoothAmount; i++)
			smoothHeightmap (heightmap);
		terrainData.SetHeights (0, 0, heightmap);
	}

	float[,] smoothHeightmap(float[,] map)
	{
		for (int y = 1; y < map.GetLength(0) - 1; y++)
		{
			for (int x = 1; x < map.GetLength(1) - 1; x++)
			{
				float avgheight = (map[x,y] +
					map[x+1,y] +
					map[x-1,y] +
					map[x+1,y+1] +
					map[x-1,y-1] +
					map[x+1,y-1] +
					map[x-1,y+1] +
					map[x,y+1] +
					map[x,y-1])/9.0f;

				map[x,y] = avgheight;
			}
		}
		return map;
	}

	public void PaintTexturesForSteepness() {
		Terrain t = gameObject.GetComponent<Terrain> ();
		float[,,] map = new float[t.terrainData.alphamapWidth, t.terrainData.alphamapHeight, t.terrainData.alphamapLayers];
		if (onlyAlterSplatForSteepTerrain) {
			// retrieve existing maps
			map = t.terrainData.GetAlphamaps (0, 0, t.terrainData.alphamapWidth, t.terrainData.alphamapHeight);
		}
		float maxAngle = 0;
		float minAngle = 90;
		float minHeight = 1000;
		float maxHeight = 0;
		int minHeightX = -1;
		int minHeightZ = -1;
		int checkedNormals = 0;
		int alteredSplats = 0;
		// For each point on the alphamap...
		for (int y = 0; y < t.terrainData.alphamapHeight; y++) {
			for (int x = 0; x < t.terrainData.alphamapWidth; x++) {
				// Get the normalized terrain coordinate that
				// corresponds to the the point.
				float normX = x * 1.0f / (t.terrainData.alphamapWidth - 1);
				float normY = y * 1.0f / (t.terrainData.alphamapHeight - 1);

				// Get the steepness value at the normalized coordinate.
				float angle = t.terrainData.GetSteepness(normY, normX);
				float height = t.terrainData.GetInterpolatedHeight (normY, normX);
				if (normX < 0.99f && normX > 0.01f && normY < 0.99f && normY > 0.01f) {
					if (height < minHeight) {
						minHeight = height;
						minHeightX = Mathf.RoundToInt (normX * t.terrainData.size.x);
						minHeightZ = Mathf.RoundToInt (normY * t.terrainData.size.z);
					}
					if (height > maxHeight)
						maxHeight = height;
					if (angle > maxAngle) {
						maxAngle = angle;
					}
					if (angle < minAngle)
						minAngle = angle;
				}
				checkedNormals++;
				// Steepness is given as an angle, 0..90 degrees. Divide
				// by 90 to get an alpha blending value in the range 0..1.
				float frac = angle / 90.0f;
				//float frac = angle < 30.0f ? 0 : 1;
				if (onlyAlterSplatForSteepTerrain) {
					if (frac >= splatSettings.steepnessThreshold) {
						alteredSplats++;
						float previousRemainingAlpha = 1f - map [x, y, 3];
						map [x, y, 3] = frac;
						float remainingAlpha = 1f - frac;
						for (int i = 0; i < t.terrainData.alphamapLayers; i++) {
							if (i != 3) {
								float previousAlpha = map [x, y, i];
								if (previousAlpha > 0)
									map [x, y, i] = previousAlpha / previousRemainingAlpha * remainingAlpha;
							}
						}
					}
				} else {
					map[x, y, 3] = frac; // rock
					if (height > 110)
						map [x, y, 4] = 1 - frac; // snow
					else
						map[x, y, 2] = 1 - frac; // grass
				}
			}
		}

		if (!checkOnly)
			t.terrainData.SetAlphamaps(0, 0, map);
		Debug.Log(string.Format("steepness angle is between {0} and {1}, height is between {2} (at {4}/{5}) and {3}",
			minAngle, maxAngle, minHeight, maxHeight, minHeightX, minHeightZ));
		if (onlyAlterSplatForSteepTerrain) {
			Debug.Log (string.Format ("steepness threshold should be between {0} and {1} to have an effect", minAngle / 90f, maxAngle / 90f));
			Debug.Log ("altered " + alteredSplats + " of " + checkedNormals + " (" + ((float)alteredSplats / checkedNormals * 100f) + "%)");
		}
			
	}

	public void PaintTextures()
	{
		terrainData = gameObject.GetComponent<Terrain>().terrainData;
		float[, ,] splatmapData = new float[terrainData.alphamapWidth, 
	                                           terrainData.alphamapHeight, 
	                                           terrainData.alphamapLayers];
		
		Debug.Log (String.Format("Painting terrain {0}, alpha map size is {1}x{2}, terrain is {3}x{4}", terrainData,
			terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.size.x, terrainData.size.z));

		for (int y = 0; y < terrainData.alphamapHeight; y++)
		{
		    for (int x = 0; x < terrainData.alphamapWidth; x++)
		    {
				float terrainX = x * 1.0f / (terrainData.alphamapWidth - 1);
				float terrainY = y * 1.0f / (terrainData.alphamapHeight - 1);
				float terrainHeight = terrainData.GetInterpolatedHeight(terrainY,terrainX);

			     float[] splat = new float[splatSettings.splatHeights.Length];

				for(int i = 0; i < splatSettings.splatHeights.Length; i++)
			     {
					float thisNoise = 1; //map(Mathf.PerlinNoise(x*0.03f,y*0.03f),0,1,0.5f,1);
					float thisHeightStart = splatSettings.splatHeights[i].startingHeight * thisNoise - 
						splatSettings.splatHeights[i].overlap  * thisNoise;

				    float nextHeightStart = 0;
					if(i != splatSettings.splatHeights.Length-1)
				    {
				
						nextHeightStart = splatSettings.splatHeights[i+1].startingHeight * thisNoise + 
							splatSettings.splatHeights[i+1].overlap  * thisNoise;
				    }


					if(i == splatSettings.splatHeights.Length-1 && terrainHeight >= thisHeightStart)
			     		splat[i] = 1;

			     	else if(terrainHeight >= thisHeightStart && 
			     		terrainHeight <= nextHeightStart)
			     		splat[i] = 1;
			     }
			     
			     normalize(splat);

				for(int j = 0; j < splatSettings.splatHeights.Length; j++)
			     {
			     	splatmapData[x, y, j] = splat[j];
			     }
		 	}

		}

		terrainData.SetAlphamaps(0, 0, splatmapData);
	}
}

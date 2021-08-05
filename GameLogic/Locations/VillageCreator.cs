using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class VillageCreator : MonoBehaviour
{

	[Serializable]
	public struct HousesCount
	{
		public int number;
		public GameObject houseModel;
	}

	public struct HousePlacement
	{
		public GameObject gameObject;
		public Vector3 position;
		public Vector3 rotation;
		public float maxDistanceFromGround;
		public bool invalidPosition;

		public HousePlacement (GameObject go)
		{
			gameObject = go;
			position = go.transform.position;
			rotation = go.transform.rotation.eulerAngles;
			maxDistanceFromGround = 100f;
			invalidPosition = true;
		}

		public HousePlacement Mutation ()
		{
			// mutate position x/z
			if (Random.value < 0.3f) {
				position.x += Random.Range (-1f, 1f);
				position.z += Random.Range (-1f, 1f);
			}
			// mutate rotation
			if (Random.value < 0.3f) {
				rotation.y += Random.Range (-90f, 90f);
			}
			return this;
		}
	}

	public struct HousePlacements
	{
		public List<HousePlacement> placements;

		public HousePlacements (List<HousePlacement> placements)
		{
			this.placements = placements;
			totalGroundOffset = 0;
			invalidPositions = 0;
			fitness = 0;
		}

		public HousePlacements (HousePlacements p1, HousePlacements p2, float mutationRate)
		{
			placements = new List<HousePlacement> ();
			for (int i = 0; i < p1.placements.Count; i++) {
				float distSum = p1.placements [i].maxDistanceFromGround + p2.placements [i].maxDistanceFromGround;
				float chance1 = p1.placements [i].invalidPosition == p2.placements [i].invalidPosition
					? (1f - p1.placements [i].maxDistanceFromGround / distSum)
					: (p1.placements [i].invalidPosition ? 0.1f : 0.9f);
				HousePlacement h = Random.value < chance1 ? p1.placements [i] : p2.placements [i];
				if (Random.value <= mutationRate) {
					h.Mutation ();
				}
				placements.Add (h);
			}
			totalGroundOffset = 0;
			invalidPositions = 0;
			fitness = 0;
		}

		public float totalGroundOffset;
		public int invalidPositions;
		public float fitness;

		public float CalculateFitness ()
		{
			totalGroundOffset = 0;
			invalidPositions = 0;
			for (int i = 0; i < placements.Count; i++) {
				HousePlacement placement = placements [i];
				placement.gameObject.GetComponent<GroundHelper> ().IncludeInBoundingCheck (false);
			}
			for (int i = 0; i < placements.Count; i++) {
				//foreach(HousePlacement placement in placements) {
				HousePlacement placement = placements [i];
				placement.gameObject.transform.position = placement.position;
				placement.gameObject.transform.rotation = Quaternion.Euler (placement.rotation);
				GroundHelper gh = placement.gameObject.GetComponent<GroundHelper> ();
				gh.TouchGround ();
				placement.position = placement.gameObject.transform.position;
				placement.maxDistanceFromGround = gh.maxDistanceFromGround;
				placement.invalidPosition = !gh.isPlacedOnGround;
				totalGroundOffset += placement.maxDistanceFromGround;
				if (placement.invalidPosition)
					invalidPositions++;
				placements [i] = placement;
			}
			fitness = -invalidPositions * 50 - totalGroundOffset;
			return fitness;
		}

		public void Show ()
		{
			foreach (HousePlacement placement in placements) {
				placement.gameObject.transform.position = placement.position;
				placement.gameObject.transform.rotation = Quaternion.Euler (placement.rotation);
			}
		}
	}

	public class FitnessComparer : IComparer<HousePlacements>
	{
		#region IComparer implementation

		public int Compare (HousePlacements x, HousePlacements y)
		{
			return x.fitness > y.fitness ? -1 : (x.fitness < y.fitness ? 1 : 0);
		}

		#endregion
	}

	readonly FitnessComparer fitnessComparer = new FitnessComparer ();

	[Serializable]
	public struct SAParameters
	{
		public float startTemperature;
		public float endTemperature;
		public int steps;
		public int cycles;

		public SAParameters (float startTemperature, float endTemperature, int steps, int cycles)
		{
			this.startTemperature = startTemperature;
			this.endTemperature = endTemperature;
			this.steps = steps;
			this.cycles = cycles;
		}
	}

	[Serializable]
	public struct GAParameters
	{
		public int numberIndividuals;
		public int numberParents;
		public int numberElites;
		public int generations;
		public float mutationRate;

		public GAParameters (int numberIndividuals, int numberParents, int numberElites, int generations, float mutationRate)
		{
			this.numberIndividuals = numberIndividuals;
			this.numberParents = numberParents;
			this.numberElites = numberElites;
			this.generations = generations;
			this.mutationRate = mutationRate;
		}
	}

	public HousesCount[] houseDefinitions;
	public VillageBounds villageBounds;
	public GAParameters geneticAlgorithmParameters = new GAParameters (100, 20, 10, 100, 0.1f);
	public SAParameters simulatedAnnealingParameters = new SAParameters(4, 0.001f, 80, 10);
	public bool addNewHouses = false;

	List<HousePlacements> individuals = new List<HousePlacements> ();
	Transform modelTemplates;
	HousePlacements currentFittest;
	HousePlacements currentWorst;
	float averageFitness;
	int generation;
	int stepCounter;
	float currentTemperature;
	bool isStarted = false;

	void Start ()
	{
		PrepareModels ();
	}

	void Update ()
	{
	}

	public void StartGeneticAlgorithm ()
	{
		if (isStarted)
			return;
		isStarted = true;
		List<HousePlacement> placements = CreateRandomPlacements ();
		HousePlacements first = new HousePlacements (placements);
		individuals.Add (first);
		for (int i = 0; i < geneticAlgorithmParameters.numberIndividuals - 1; i++) {
			individuals.Add (Randomize (first));
		}
		generation = 0;
		Debug.Log (individuals.Count + " initialized.");
		StartCoroutine (BreedNextGeneration ());
	}

	public void StartSimulatedAnnealing ()
	{
		if (isStarted)
			return;
		isStarted = true;
		if (currentFittest.placements == null || addNewHouses) {
			currentFittest = new HousePlacements (CreateRandomPlacements ());
		}

		float fitness = currentFittest.CalculateFitness ();
		Debug.Log ("Start simulated annealing, start random fitness=" + fitness);
		StartCoroutine (SimulateAnnealing ());
	}

	public void Reset() {
		if (currentFittest.placements != null) {
			foreach (HousePlacement hp in currentFittest.placements)
				Destroy (hp.gameObject);
			currentFittest.placements = null;
		}
	}

	void PrepareModels ()
	{
		GameObject templates = new GameObject ();
		templates.transform.parent = transform;
		templates.name = "_Models";

		for (int i = 0; i < houseDefinitions.Length; i++) {
			for (int n = 0; n < houseDefinitions [i].number; n++) {
				GameObject go = Instantiate (houseDefinitions [i].houseModel);
				go.transform.parent = templates.transform;
				/*
				BoxCollider boundingCollider = go.GetComponent<BoxCollider> ();
				if (boundingCollider == null) {
					AddBoundingBoxCollider (go);
				}
				*/
				go.AddComponent<GroundHelper> ();
				go.name = houseDefinitions [i].houseModel.name;
			}
		}

		templates.SetActive (false);
		modelTemplates = templates.transform;
	}

	public List<HousePlacement> CreateRandomPlacements ()
	{
		List<HousePlacement> newPlacement = new List<HousePlacement> ();
		for (int i = 0; i < modelTemplates.childCount; i++) {
			Vector3 v = ProjectedOnGround (villageBounds.RandomSpot ());
			GameObject preparedModel = modelTemplates.GetChild (i).gameObject;
			GameObject go = Instantiate (preparedModel);
			go.transform.parent = transform;
			go.transform.position = v;
			go.transform.rotation *= Quaternion.AngleAxis (Random.Range (0, 360), Vector3.up);
			go.name = preparedModel.name + "-" + i;
			newPlacement.Add (new HousePlacement (go));
		}
		return newPlacement;
	}

	public HousePlacements Randomize (HousePlacements original)
	{
		List<HousePlacement> newPlacement = new List<HousePlacement> ();
		for (int i = 0; i < original.placements.Count; i++) {
			Vector3 v = ProjectedOnGround (villageBounds.RandomSpot ());
			GameObject go = original.placements [i].gameObject;
			go.transform.position = v;
			go.transform.rotation *= Quaternion.AngleAxis (Random.Range (0, 360), Vector3.up);
			newPlacement.Add (new HousePlacement (go));
		}
		return new HousePlacements (newPlacement);
	}

	public HousePlacements Randomize (HousePlacements original, float factor)
	{
		List<HousePlacement> newPlacement = new List<HousePlacement> ();
		for (int i = 0; i < original.placements.Count; i++) {
			GameObject go = original.placements [i].gameObject;
			float r = villageBounds.maxRadius * factor;
			Vector3 v;
			do {
				v = go.transform.position + new Vector3 (Random.Range (-r, r), 0, Random.Range (-r, r));
			} while (!villageBounds.IsInBounds(v));
			v = ProjectedOnGround (v);
			go.transform.position = v;
			go.transform.rotation *= Quaternion.AngleAxis (Random.Range (-180, 180) * factor, Vector3.up);
			newPlacement.Add (new HousePlacement (go));
		}
		return new HousePlacements (newPlacement);
	}

	Vector3 ProjectedOnGround (Vector3 v)
	{
		RaycastHit hit;
		if (Physics.Raycast (v + Vector3.up * 10, Vector3.down, out hit)) {
			return hit.point;
		}
		return v;
	}

	IEnumerator BreedNextGeneration ()
	{
		GAParameters parms = geneticAlgorithmParameters;
		while (generation < parms.generations) {
			float fitnessSum = 0;
			for (int i = 0; i < individuals.Count; i++) {
				fitnessSum += individuals [i].CalculateFitness ();
			}
			yield return null;
			averageFitness = fitnessSum / individuals.Count;
			individuals.Sort (fitnessComparer);
			currentFittest = individuals [0];
			currentWorst = individuals [individuals.Count - 1];
			Debug.Log ("Generation #" + generation + ", fitness=" + currentFittest.fitness + " - " + currentWorst.fitness + ", avg=" + averageFitness);
			currentFittest.Show ();
			yield return null;

			List<HousePlacements> nextGeneration = new List<HousePlacements> ();
			for (int i = 0; i < parms.numberElites; i++) {
				nextGeneration.Add (individuals [i]);
			}
			for (int i = 0; i < parms.numberIndividuals - parms.numberElites; i++) {
				HousePlacements p1 = individuals [Random.Range (0, parms.numberParents)];
				HousePlacements p2 = individuals [Random.Range (0, parms.numberParents)];
				HousePlacements child = new HousePlacements (p1, p2, parms.mutationRate);
				nextGeneration.Add (child);
			}
			individuals = nextGeneration;
			generation++;
			yield return null;
		}
		isStarted = false;
	}

	IEnumerator SimulateAnnealing ()
	{
		SAParameters parms = simulatedAnnealingParameters;
		currentTemperature = parms.startTemperature;
		float currentBestFitness = currentFittest.fitness;
		for (stepCounter = 0; stepCounter < parms.steps; stepCounter++) {
			float progress = (float)stepCounter / parms.steps;
			currentTemperature = parms.startTemperature * Mathf.Pow (parms.endTemperature / parms.startTemperature, progress);
			for (int i = 0; i < parms.cycles; i++) {
				HousePlacements neighbour = Randomize (currentFittest, 1f - progress);
				float fitness = neighbour.CalculateFitness ();
				float probability;
				if (fitness > currentBestFitness) {
					probability = 1;
				} else {
					probability = Mathf.Exp (-Mathf.Abs (fitness - currentBestFitness) / currentTemperature);
				}
				if (probability >= Random.value) {
					Debug.Log (string.Format ("previous best fitness={0}, test fitness={1}, probability={2}, temperature={3}, progress={4}",
						currentBestFitness, fitness, probability, currentTemperature, progress));
					currentFittest = neighbour;
					currentBestFitness = fitness;
				}
			}
			yield return null;
		}
		Debug.Log (string.Format ("Simulated annealing finished with fitness={0} (avg fitness of placement={1}).",
			currentBestFitness, currentBestFitness/currentFittest.placements.Count));
		currentFittest.Show ();
		isStarted = false;
	}
}

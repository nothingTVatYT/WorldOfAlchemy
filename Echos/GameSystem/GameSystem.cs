using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSystem : MonoBehaviour
{

	public static GameSystem gameSystem;

	public delegate void OnItemEquipChanged (WOAItem item, bool equipped);

	public OnItemEquipChanged onItemEquipChangedCallback;

	public delegate void OnEffectChanged (WOAEffect removedEffect, WOAEffect newEffect);

	public OnEffectChanged onEffectChangedCallback;

	[SerializeField] public GameObject player;
	[SerializeField] public GameObject canvas;
	private BasePlayer playerData;
	[SerializeField] private GameObject targetPanel;
	[SerializeField] private SceneLightSettings[] sceneLightSettings;
	[SerializeField] private float interactingDistance = 3;

	private PlayerSoundController playerSoundController;
	private NPCBehaviour npc;
	private Interactable interactable;
	private bool attackRequested;
	private WOAWeapon usedWeapon;
	private float lastHit;
	[HideInInspector]
	public bool playerIsLoaded;

	void Awake ()
	{
		gameSystem = this;
		playerIsLoaded = false;
		Debug.Log ("GameSystem initialized");
	}

	void Start ()
	{
		playerData = player.GetComponent<BasePlayer> ();
		playerSoundController = player.GetComponent<PlayerSoundController> ();
		// update player data from global game states
		GameStates g = GlobalGameState.gameState.loadedGame;
		if (g != null) {
			g.playerData.Fill(playerData.volatileData);
			playerData.name = g.playerData.name;
			player.transform.position = g.playerLocation;
			player.transform.rotation = Quaternion.Euler(g.playerRotation);
			playerData.InitializePlayer ();
			playerIsLoaded = true;
		}
	}

	void Update() {
		if (Input.GetButtonDown("Quit") && !canvas.activeSelf) {
			Logout ();
		}
		if (Input.GetKeyDown(KeyCode.U) && Input.GetKey(KeyCode.RightControl)) {
			canvas.SetActive (!canvas.activeSelf);
		}
		if (Input.GetKeyDown(KeyCode.T) && Input.GetKey(KeyCode.RightControl)) {
			Terrain.activeTerrain.drawTreesAndFoliage = !Terrain.activeTerrain.drawTreesAndFoliage;
		}
	}

	public void Logout() {
		GlobalGameState.gameState.SaveGame (player);
		GlobalGameState.gameState.GotoMenu ();
		PlayerPrefs.Save ();
	}

	// Update is called once per frame
	void FixedUpdate ()
	{
		//expireEffects ();
		if (npc != null && attackRequested && usedWeapon != null) {
			if (lastHit < 1e-3f || (Time.time - lastHit > usedWeapon.speed)) {
				float dmg = usedWeapon.damage;
				lastHit = Time.time;
				usedWeapon.startCooldown ();
				BaseCharacter enemy = BaseCharacter.getCharacterFromGameObject (playerData.target);
				if (enemy != null) {
					enemy.takeDamage (dmg);
					GameConsole.gameConsole.println (GameConsole.InfoLevel.FineDetail, string.Format ("You hit {0} and caused {1} damage.", enemy.characterName, dmg));
				}
				attackRequested = false;
			}
		}
	}

	public bool playerIsAttacking ()
	{
		return attackRequested;
	}

	public bool playerCanReach(Transform t) {
		return Vector3.Distance(t.position, player.transform.position) <= interactingDistance;
	}

	public void tryPlayOneShot (PlayerSoundController.SoundEvent soundEvent)
	{
		if (playerSoundController != null)
			playerSoundController.PlayOneShot (soundEvent);
	}

	public WOAWeapon getEquippedWeapon ()
	{
		return usedWeapon;
	}

	public bool hasLivingTarget ()
	{
		return npc != null && npc.currentState () != NPCBehaviour.State_Dead;
	}

	public bool currentTargetIsDead ()
	{
		return npc != null && npc.currentState () == NPCBehaviour.State_Dead;
	}

	public void attackWith (WOAWeapon item, GameObject thing)
	{
		if (playerData.target == null) {
			GameConsole.gameConsole.println(GameConsole.InfoLevel.General, "You need a target to attack.");
			return;
		}
		if (item.coolingProgress >= 1) {
			float distance = Vector3.Distance (player.transform.position, playerData.target.transform.position);
			if (distance <= item.range) {
				if (npc != null) {
					GameObject bullet = Instantiate (thing);
					thing.transform.position = player.transform.position + Vector3.up * 0.3f + Vector3.right * 0.1f;
					lastHit = Time.time;
					IWeaponObject wo = bullet.GetComponent<IWeaponObject> ();
					if (item.throwSound != null)
						player.GetComponent<AudioSource> ().PlayOneShot (item.randomThrowSound ());
					if (wo != null)
						wo.attackTarget (player, playerData.target);
					else {
						string msg = thing.name + " has no IWeaponObject attached.";
						Debug.LogError (msg);
						GameConsole.gameConsole.println (GameConsole.InfoLevel.SevereWarning, msg);
						Destroy (bullet);
					}
					item.startCooldown ();
				}
			} else {
				GameConsole.gameConsole.println(GameConsole.InfoLevel.General, "The target is too far away.");
			}
		} else
			GameConsole.gameConsole.println(GameConsole.InfoLevel.General, item.itemName + " is not ready.");
	}

	public void attackWith (WOAWeapon item)
	{
		if (playerData.target != null) {
			if (item != null) {
				float distance = Vector3.Distance (player.transform.position, playerData.target.transform.position);
				if (distance <= item.range) {
					if (npc != null) {
						attackRequested = true;
						usedWeapon = item;
					} else {
						if (interactable != null)
							interactable.use (playerData);
					}
				} else {
					GameConsole.gameConsole.println(GameConsole.InfoLevel.General, "The target is too far away.");
				}
			} else {
				GameConsole.gameConsole.println(GameConsole.InfoLevel.General, "There is no weapon in this slot.");
			}
		} else {
			GameConsole.gameConsole.println(GameConsole.InfoLevel.General, "You need a target to attack.");
		}

	}

	public void teleportPlayer(NamedLocation location) {
		if (location.scene != "") {
			ensureSceneLoaded (location.scene);
		}
		player.transform.position = location.position;
		player.transform.rotation = Quaternion.Euler (location.rotation);
	}

	public void switchScenes (String sceneToLoad, String sceneToUnload)
	{
		if (sceneToLoad != "") {
			SceneManager.LoadScene (sceneToLoad, LoadSceneMode.Additive);
			setLightSettings (sceneToLoad);
		}
		if (sceneToUnload != "") {
			StartCoroutine (SceneUnloader (sceneToUnload, 2));
		}
	}

	void ensureSceneLoaded(String sceneName) {
		if (!SceneManager.GetSceneByName(sceneName).isLoaded) {
			switchScenes (sceneName, null);
		}
	}

	void setLightSettings (string sceneName)
	{
		foreach (SceneLightSettings setting in sceneLightSettings) {
			if (setting != null && setting.sceneName == sceneName) {
				RenderSettings.ambientIntensity = setting.ambientIntensity;
			}
		}
	}

	IEnumerator SceneUnloader (string sceneName, float delayTime)
	{
		yield return new WaitForSeconds (delayTime);
		SceneManager.UnloadSceneAsync (sceneName);
	}
}

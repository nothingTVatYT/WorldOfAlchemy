using UnityEngine;

public class CharacterIK : MonoBehaviour {

	[System.Serializable]
	public struct StepSoundSettings {
		public AudioClip clipLeftFoot;
		public AudioClip clipRightFoot;
		public float footOnGroundThreshold;
		public float footInAirThreshold;
		public float volume;
		public float minSoundInterval;
	}

	Animator anim;
	Transform leftFoot;
	Transform rightFoot;
	[SerializeField] float footHeightDifference = 0.08f;
	[SerializeField] float footRayOffsetY = 0f;
	Vector3 footRayOffset;
	[SerializeField] bool stepSoundsEnabled;
	[SerializeField] StepSoundSettings stepSounds;
	AudioSource audioSource;
	bool playedLeftStep = false;
	bool playedRightStep = false;
	float soundPlayed = 0;

	FloatRingBuffer lhBuffer = new FloatRingBuffer(30);
	FloatRingBuffer rhBuffer = new FloatRingBuffer(30);

	void Start () {
		anim = GetComponent<Animator> ();
		leftFoot = anim.GetBoneTransform (HumanBodyBones.LeftFoot);
		rightFoot = anim.GetBoneTransform (HumanBodyBones.RightFoot);
		footRayOffset = new Vector3 (0, footRayOffsetY, 0);
		audioSource = GetComponent<AudioSource> ();
		if (audioSource == null) {
			audioSource = gameObject.AddComponent<AudioSource> ();
			audioSource.spatialBlend = 1f;
		}
		audioSource.volume = stepSounds.volume;
		lhBuffer.Add(0);
		rhBuffer.Add(0);
	}

	public void OnAnimatorIK(int layerIndex) {
			float lh = rightFoot.position.y - leftFoot.position.y;
			if (lh > 0.01f)
				lhBuffer.Add(lh);
			float rh = leftFoot.position.y - rightFoot.position.y;
			if (rh > 0.01f)
				rhBuffer.Add(rh);

			float relLh = Mathf.InverseLerp(0, lhBuffer.Max(), lh);
			float relRh = Mathf.InverseLerp(0, rhBuffer.Max(), rh);

			if (relLh <= footHeightDifference && relRh <= footHeightDifference)
				SetIK(1,1);
			else
				SetIK(relLh, relRh);
	}

	public void SetIK(float leftFootOnGround, float rightFootOnGround) {
		
			RaycastHit hit;
			if (stepSoundsEnabled) {
				if (leftFootOnGround >= stepSounds.footOnGroundThreshold && !playedLeftStep) {
					if (Time.time - soundPlayed > stepSounds.minSoundInterval)
						audioSource.PlayOneShot (stepSounds.clipLeftFoot);
					soundPlayed = Time.time;
					playedLeftStep = true;
					playedRightStep = false;
				//} else if (leftFootOnGround < stepSounds.footInAirThreshold && playedLeftStep) {
					//playedLeftStep = false;
				}
			}
			if (Physics.Raycast(leftFoot.position + footRayOffset, Vector3.down, out hit, 1f)) {
				anim.SetIKPosition (AvatarIKGoal.LeftFoot, hit.point + Vector3.up * anim.leftFeetBottomHeight);
				anim.SetIKPositionWeight (AvatarIKGoal.LeftFoot, leftFootOnGround);
				anim.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation);
				anim.SetIKRotationWeight (AvatarIKGoal.LeftFoot, leftFootOnGround);
			} else {
				anim.SetIKPositionWeight (AvatarIKGoal.LeftFoot, 0);
				anim.SetIKRotationWeight (AvatarIKGoal.LeftFoot, 0);
			}
			if (stepSoundsEnabled) {
				if (rightFootOnGround >= stepSounds.footOnGroundThreshold && !playedRightStep) {
					if (Time.time - soundPlayed > stepSounds.minSoundInterval)
						audioSource.PlayOneShot (stepSounds.clipRightFoot);
					soundPlayed = Time.time;
					playedRightStep = true;
					playedLeftStep = false;
				//} else if (rightFootOnGround < stepSounds.footInAirThreshold && playedRightStep) {
				//	playedRightStep = false;
				}
			}
			if (Physics.Raycast(rightFoot.position + footRayOffset, Vector3.down, out hit, 1f)) {
				anim.SetIKPosition (AvatarIKGoal.RightFoot, hit.point + Vector3.up * anim.rightFeetBottomHeight);
				anim.SetIKPositionWeight (AvatarIKGoal.RightFoot, rightFootOnGround);
				anim.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation);
				anim.SetIKRotationWeight (AvatarIKGoal.RightFoot, rightFootOnGround);
			} else {
				anim.SetIKPositionWeight (AvatarIKGoal.RightFoot, 0);
				anim.SetIKRotationWeight (AvatarIKGoal.RightFoot, 0);
			}
	}
}

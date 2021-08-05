using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace global
{
	[Serializable]
	public class EchosMouseLook
	{
		public float XSensitivity = 2f;
		public float YSensitivity = 2f;
		public bool clampVerticalRotation = true;
		public float MinimumX = -90F;
		public float MaximumX = 90F;
		public float MinimumY = -90F;
		public float MaximumY = 90F;
		public bool smooth;
		public float smoothTime = 5f;
		public bool lockCursor = true;
		public bool turnPlayer = true;

		private bool m_cursorIsLocked = false;
		private float absXRotation = 0;
		private float absYRotation = 0;
		private float lastTimeToggled = 0;

		public void LookRotation (Transform player, Transform camera)
		{
			if (!Cursor.visible) {
				float yRot = Input.GetAxis ("Mouse X") * XSensitivity;
				float xRot = Input.GetAxis ("Mouse Y") * YSensitivity;

				Quaternion m_CameraTargetRot = camera.localRotation;
				float newXRot = absXRotation - xRot;
				float newYRot = absYRotation + yRot;
				newXRot = Mathf.Clamp (newXRot, MinimumX, MaximumX);
				newYRot = Mathf.Clamp (newYRot, MinimumY, MaximumY);
				absXRotation = newXRot;
				absYRotation = newYRot;
				m_CameraTargetRot = Quaternion.Euler (absXRotation, turnPlayer ? 0 : absYRotation, 0f);
				if (turnPlayer && Mathf.Abs(yRot) > 0) {
					Quaternion p = player.localRotation * Quaternion.Euler(0, yRot, 0);
					player.localRotation = p;
				}

				if (smooth) {
					camera.localRotation = Quaternion.Slerp (camera.localRotation, m_CameraTargetRot,
						smoothTime * Time.deltaTime);
				} else {
					camera.localRotation = m_CameraTargetRot;
				}

			}
			UpdateCursorLock ();
		}

		public void ResetLookRotation (Transform camera)
		{
			absXRotation = 0;
			absYRotation = 0;
			camera.localRotation = Quaternion.Euler (absXRotation, absYRotation, 0f);
		}

		public void SetCursorLock (bool value)
		{
			lockCursor = value;
			if (!lockCursor) {//we force unlock the cursor if the user disable the cursor locking helper
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}

		public void UpdateCursorLock ()
		{
			//if the user set "lockCursor" we check & properly lock the cursos
			if (lockCursor)
				InternalLockUpdate ();
		}

		private void InternalLockUpdate ()
		{
			if (Input.GetKeyUp (KeyCode.Escape)) {
				m_cursorIsLocked = false;
			} else if (Input.GetButtonUp("Fire2")) {
				if (Time.time - lastTimeToggled > 0.1f) {
					m_cursorIsLocked = !m_cursorIsLocked;
					lastTimeToggled = Time.time;
				}
			}

			if (m_cursorIsLocked) {
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			} else if (!m_cursorIsLocked) {
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}
	}
}

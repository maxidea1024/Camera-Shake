using System.Collections.Generic;
using UnityEngine;

namespace CameraShake
{
    /// <summary>
    /// Camera shaker component registeres new shakes, holds a list of active shakes, and applies them to the camera additively.
    /// </summary>
    public class CameraShaker : MonoBehaviour
    {
        public static CameraShaker Instance;
        public static CameraShakePresets Presets;

        private readonly List<ICameraShake> activeShakes = new();

        [Tooltip("Transform which will be affected by the shakes.\n\nCameraShaker will set this transform's local position and rotation.")]
        [SerializeField]
        private Transform _cameraRigTransform;

        [Tooltip("Scales the strength of all shakes.")]
        [Range(0, 1)]
        public float StrengthMultiplier = 1f;

        public CameraShakePresets ShakePresets;

        /// <summary>
        /// Adds a shake to the list of active shakes.
        /// </summary>
        public static void Shake(ICameraShake shake)
        {
            if (IsInstanceNull()) return;

            Instance.RegisterShake(shake);
        }

        /// <summary>
        /// Adds a shake to the list of active shakes.
        /// </summary>
        public void RegisterShake(ICameraShake shake)
        {
            shake.Initialize(_cameraRigTransform.position, _cameraRigTransform.rotation);
            activeShakes.Add(shake);
        }

        /// <summary>
        /// Sets the transform which will be affected by the shakes.
        /// </summary>
        public void SetCameraTransform(Transform cameraTransform)
        {
            _cameraRigTransform = cameraTransform;
            _cameraRigTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        private void Awake()
        {
            Instance = this;
            ShakePresets = new CameraShakePresets(this);
            Presets = ShakePresets;
            if (_cameraRigTransform == null)
            {
                _cameraRigTransform = transform;
            }
        }

        private void Update()
        {
            if (_cameraRigTransform == null)
            {
                return;
            }

            var cameraDisplacement = Displacement.Zero;
            for (int activeShakeIndex = activeShakes.Count - 1; activeShakeIndex >= 0; --activeShakeIndex)
            {
                if (activeShakes[activeShakeIndex].IsFinished)
                {
                    activeShakes.RemoveAt(activeShakeIndex);
                }
                else
                {
                    activeShakes[activeShakeIndex].Tick(Time.deltaTime, _cameraRigTransform.position, _cameraRigTransform.rotation);
                    cameraDisplacement += activeShakes[activeShakeIndex].CurrentDisplacement;
                }
            }

            _cameraRigTransform.SetLocalPositionAndRotation(
                StrengthMultiplier * cameraDisplacement.Position,
                Quaternion.Euler(StrengthMultiplier * cameraDisplacement.EulerAngles));
        }

        private static bool IsInstanceNull()
        {
            if (Instance == null)
            {
                Debug.LogError("CameraShaker Instance is missing!");
                return true;
            }

            return false;
        }
    }
}

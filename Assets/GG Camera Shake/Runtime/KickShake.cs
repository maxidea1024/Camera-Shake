using UnityEngine;

namespace CameraShake
{
    public class KickShake : ICameraShake
    {
        readonly Params _pars;
        readonly Vector3? _sourcePosition;
        readonly bool _attenuateStrength;

        Displacement _direction;
        Displacement _prevWaypoint;
        Displacement _currentWaypoint;
        bool _release;
        float _t;

        /// <summary>
        /// Creates an instance of KickShake in the direction from the source to the camera.
        /// </summary>
        /// <param name="parameters">Parameters of the shake.</param>
        /// <param name="sourcePosition">World position of the source of the shake.</param>
        /// <param name="attenuateStrength">Change strength depending on distance from the camera?</param>
        public KickShake(Params parameters, Vector3 sourcePosition, bool attenuateStrength)
        {
            _pars = parameters;
            _sourcePosition = sourcePosition;
            _attenuateStrength = attenuateStrength;
        }

        /// <summary>
        /// Creates an instance of KickShake. 
        /// </summary>
        /// <param name="parameters">Parameters of the shake.</param>
        /// <param name="direction">Direction of the kick.</param>
        public KickShake(Params parameters, Displacement direction)
        {
            _pars = parameters;
            _direction = direction.Normalized;
        }

        public Displacement CurrentDisplacement { get; private set; }

        public bool IsFinished { get; private set; }

        public void Initialize(Vector3 cameraPosition, Quaternion cameraRotation)
        {
            if (_sourcePosition != null)
            {
                _direction = Attenuator.Direction(_sourcePosition.Value, cameraPosition, cameraRotation);
                if (_attenuateStrength)
                {
                    _direction *= Attenuator.Strength(_pars.Attenuation, _sourcePosition.Value, cameraPosition);
                }
            }

            _currentWaypoint = Displacement.Scale(_direction, _pars.Strength);
        }

        public void Tick(float deltaTime, Vector3 cameraPosition, Quaternion cameraRotation)
        {
            if (_t < 1)
            {
                Move(deltaTime,
                    _release ? _pars.ReleaseTime : _pars.AttackTime,
                    _release ? _pars.ReleaseCurve : _pars.AttackCurve);
            }
            else
            {
                CurrentDisplacement = _currentWaypoint;
                _prevWaypoint = _currentWaypoint;
                if (_release)
                {
                    IsFinished = true;
                    return;
                }
                else
                {
                    _release = true;
                    _t = 0;
                    _currentWaypoint = Displacement.Zero;
                }
            }
        }

        private void Move(float deltaTime, float duration, AnimationCurve curve)
        {
            if (duration > 0)
                _t += deltaTime / duration;
            else
                _t = 1;

            CurrentDisplacement = Displacement.Lerp(
                _prevWaypoint,
                _currentWaypoint,
                curve.Evaluate(_t));
        }

        [System.Serializable]
        public class Params
        {
            /// <summary>
            /// Strength of the shake for each axis.
            /// </summary>
            [Tooltip("Strength of the shake for each axis.")]
            public Displacement Strength = new(Vector3.zero, Vector3.one);

            /// <summary>
            /// How long it takes to move forward.
            /// </summary>
            [Tooltip("How long it takes to move forward.")]
            public float AttackTime = 0.05f;
            public AnimationCurve AttackCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

            /// <summary>
            /// How long it takes to move back.
            /// </summary>
            [Tooltip("How long it takes to move back.")]
            public float ReleaseTime = 0.2f;
            public AnimationCurve ReleaseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

            /// <summary>
            /// How strength falls with distance from the shake source.
            /// </summary>
            [Tooltip("How strength falls with distance from the shake source.")]
            public Attenuator.StrengthAttenuationParams Attenuation;
        }
    }
}

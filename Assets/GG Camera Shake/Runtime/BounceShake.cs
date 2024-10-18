using UnityEngine;

namespace CameraShake
{
    public class BounceShake : ICameraShake
    {
        readonly Params _pars;
        readonly AnimationCurve _moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        readonly Vector3? _sourcePosition = null;

        float _attenuation = 1;
        Displacement _direction;
        Displacement _previousWaypoint;
        Displacement _currentWaypoint;
        int _bounceIndex;
        float _t;

        /// <summary>
        /// Creates an instance of BounceShake.
        /// </summary>
        /// <param name="parameters">Parameters of the shake.</param>
        /// <param name="sourcePosition">World position of the source of the shake.</param>
        public BounceShake(Params parameters, Vector3? sourcePosition = null)
        {
            _sourcePosition = sourcePosition;
            _pars = parameters;
            Displacement rnd = Displacement.InsideUnitSpheres();
            _direction = Displacement.Scale(rnd, _pars.AxesMultiplier).Normalized;
        }

        /// <summary>
        /// Creates an instance of BounceShake.
        /// </summary>
        /// <param name="parameters">Parameters of the shake.</param>
        /// <param name="initialDirection">Initial direction of the shake motion.</param>
        /// <param name="sourcePosition">World position of the source of the shake.</param>
        public BounceShake(Params parameters, Displacement initialDirection, Vector3? sourcePosition = null)
        {
            _sourcePosition = sourcePosition;
            _pars = parameters;
            _direction = Displacement.Scale(initialDirection, _pars.AxesMultiplier).Normalized;
        }

        public Displacement CurrentDisplacement { get; private set; }

        public bool IsFinished { get; private set; }

        public void Initialize(Vector3 cameraPosition, Quaternion cameraRotation)
        {
            _attenuation = _sourcePosition == null ?
                1 : Attenuator.Strength(_pars.Attenuation, _sourcePosition.Value, cameraPosition);
            _currentWaypoint = _attenuation * _direction.ScaledBy(_pars.PositionStrength, _pars.RotationStrength);
        }

        public void Tick(float deltaTime, Vector3 cameraPosition, Quaternion cameraRotation)
        {
            if (_t < 1)
            {
                if (_pars.Freq == 0)
                {
                    _t = 1;
                }
                else
                {
                    _t += deltaTime * _pars.Freq;
                }

                CurrentDisplacement = Displacement.Lerp(_previousWaypoint, _currentWaypoint,
                    _moveCurve.Evaluate(_t));
            }
            else
            {
                _t = 0;
                CurrentDisplacement = _currentWaypoint;
                _previousWaypoint = _currentWaypoint;
                _bounceIndex++;
                if (_bounceIndex > _pars.NumBounces)
                {
                    IsFinished = true;
                    return;
                }

                Displacement rnd = Displacement.InsideUnitSpheres();
                _direction = -_direction
                    + _pars.Randomness * Displacement.Scale(rnd, _pars.AxesMultiplier).Normalized;
                _direction = _direction.Normalized;
                float decayValue = 1 - (float)_bounceIndex / _pars.NumBounces;
                _currentWaypoint = decayValue * decayValue * _attenuation
                    * _direction.ScaledBy(_pars.PositionStrength, _pars.RotationStrength);
            }
        }

        [System.Serializable]
        public class Params
        {
            /// <summary>
            /// Strength of the shake for positional axes.
            /// </summary>
            [Tooltip("Strength of the shake for positional axes.")]
            public float PositionStrength = 0.05f;

            /// <summary>
            /// Strength of the shake for rotational axes.
            /// </summary>
            [Tooltip("Strength of the shake for rotational axes.")]
            public float RotationStrength = 0.1f;

            /// <summary>
            /// Preferred direction of shaking.
            /// </summary>
            [Tooltip("Preferred direction of shaking.")]
            public Displacement AxesMultiplier = new(Vector2.one, Vector3.forward);

            /// <summary>
            /// Frequency of shaking.
            /// </summary>
            [Tooltip("Frequency of shaking.")]
            public float Freq = 25;

            /// <summary>
            /// Number of vibrations before stop.
            /// </summary>
            [Tooltip("Number of vibrations before stop.")]
            public int NumBounces = 5;

            /// <summary>
            /// Randomness of motion.
            /// </summary>
            [Range(0, 1)]
            [Tooltip("Randomness of motion.")]
            public float Randomness = 0.5f;

            /// <summary>
            /// How strength falls with distance from the shake source.
            /// </summary>
            [Tooltip("How strength falls with distance from the shake source.")]
            public Attenuator.StrengthAttenuationParams Attenuation;
        }
    }
}

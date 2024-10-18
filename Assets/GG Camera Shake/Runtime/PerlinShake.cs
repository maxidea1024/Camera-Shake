using UnityEngine;

namespace CameraShake
{
    public class PerlinShake : ICameraShake
    {
        readonly Params _pars;
        readonly Envelope _envelope;

        public IAmplitudeController AmplitudeController;

        private Vector2[] _seeds;
        private float _time;
        private Vector3? _sourcePosition;
        private float _norm;

        /// <summary>
        /// Creates an instance of PerlinShake.
        /// </summary>
        /// <param name="parameters">Parameters of the shake.</param>
        /// <param name="maxAmplitude">Maximum amplitude of the shake.</param>
        /// <param name="sourcePosition">World position of the source of the shake.</param>
        /// <param name="manualStrengthControl">Pass true if you want to control amplitude manually.</param>
        public PerlinShake(
            Params parameters,
            float maxAmplitude = 1,
            Vector3? sourcePosition = null,
            bool manualStrengthControl = false)
        {
            _pars = parameters;
            _envelope = new Envelope(_pars.Envelope, maxAmplitude,
                manualStrengthControl ?
                    Envelope.EnvelopeControlMode.Manual : Envelope.EnvelopeControlMode.Auto);
            AmplitudeController = _envelope;
            _sourcePosition = sourcePosition;
        }

        public Displacement CurrentDisplacement { get; private set; }

        public bool IsFinished { get; private set; }

        public void Initialize(Vector3 cameraPosition, Quaternion cameraRotation)
        {
            _seeds = new Vector2[_pars.NoiseModes.Length];
            _norm = 0f;
            for (int i = 0; i < _seeds.Length; i++)
            {
                _seeds[i] = Random.insideUnitCircle * 20f;
                _norm += _pars.NoiseModes[i].Amplitude;
            }
        }

        public void Tick(float deltaTime, Vector3 cameraPosition, Quaternion cameraRotation)
        {
            if (_envelope.IsFinished)
            {
                IsFinished = true;
                return;
            }

            _time += deltaTime;
            _envelope.Tick(deltaTime);

            Displacement disp = Displacement.Zero;
            for (int i = 0; i < _pars.NoiseModes.Length; i++)
            {
                disp += _pars.NoiseModes[i].Amplitude / _norm *
                    SampleNoise(_seeds[i], _pars.NoiseModes[i].Freq);
            }

            CurrentDisplacement = _envelope.Intensity * Displacement.Scale(disp, _pars.Strength);

            if (_sourcePosition != null)
            {
                CurrentDisplacement *= Attenuator.Strength(_pars.Attenuation, _sourcePosition.Value, cameraPosition);
            }
        }

        private Displacement SampleNoise(Vector2 seed, float freq)
        {
            var position = new Vector3(
                Mathf.PerlinNoise(seed.x + _time * freq, seed.y),
                Mathf.PerlinNoise(seed.x, seed.y + _time * freq),
                Mathf.PerlinNoise(seed.x + _time * freq, seed.y + _time * freq));
            position -= Vector3.one * 0.5f;

            var rotation = new Vector3(
                Mathf.PerlinNoise(-seed.x - _time * freq, -seed.y),
                Mathf.PerlinNoise(-seed.x, -seed.y - _time * freq),
                Mathf.PerlinNoise(-seed.x - _time * freq, -seed.y - _time * freq));
            rotation -= Vector3.one * 0.5f;

            return new Displacement(position, rotation);
        }

        [System.Serializable]
        public class Params
        {
            /// <summary>
            /// Strength of the shake for each axis.
            /// </summary>
            [Tooltip("Strength of the shake for each axis.")]
            public Displacement Strength = new(Vector3.zero, new Vector3(2, 2, 0.8f));

            /// <summary>
            /// Layers of perlin noise with different frequencies.
            /// </summary>
            [Tooltip("Layers of perlin noise with different frequencies.")]
            public NoiseMode[] NoiseModes = { new(12, 1) };

            /// <summary>
            /// Strength over time.
            /// </summary>
            [Tooltip("Strength of the shake over time.")]
            public Envelope.EnvelopeParams Envelope;

            /// <summary>
            /// How strength falls with distance from the shake source.
            /// </summary>
            [Tooltip("How strength falls with distance from the shake source.")]
            public Attenuator.StrengthAttenuationParams Attenuation;
        }

        [System.Serializable]
        public struct NoiseMode
        {
            public NoiseMode(float freq, float amplitude)
            {
                Freq = freq;
                Amplitude = amplitude;
            }

            /// <summary>
            /// Frequency multiplier for the noise.
            /// </summary>
            [Tooltip("Frequency multiplier for the noise.")]
            public float Freq;

            /// <summary>
            /// Amplitude of the mode.
            /// </summary>
            [Tooltip("Amplitude of the mode.")]
            [Range(0, 1)]
            public float Amplitude;
        }
    }
}

﻿using UnityEngine;

namespace CameraShake
{
    /// <summary>
    /// Contains shorthands for creating common shake types.
    /// </summary>
    public class CameraShakePresets
    {
        private readonly CameraShaker _shaker;

        public CameraShakePresets(CameraShaker shaker)
        {
            _shaker = shaker;
        }

        /// <summary>
        /// Suitable for short and snappy shakes in 2D. Moves camera in X and Y axes and rotates it in Z axis. 
        /// </summary>
        /// <param name="positionStrength">Strength of motion in X and Y axes.</param>
        /// <param name="rotationStrength">Strength of rotation in Z axis.</param>
        /// <param name="freq">Frequency of shaking.</param>
        /// <param name="numBounces">Number of vibrations before stop.</param>
        public void ShortShake2D(float positionStrength = 0.08f, float rotationStrength = 0.1f, float freq = 25f, int numBounces = 5)
        {
            BounceShake.Params pars = new()
            {
                PositionStrength = positionStrength,
                RotationStrength = rotationStrength,
                Freq = freq,
                NumBounces = numBounces
            };
            _shaker.RegisterShake(new BounceShake(pars));
        }

        /// <summary>
        /// Suitable for longer and stronger shakes in 3D. Rotates camera in all three axes.
        /// </summary>
        /// <param name="strength">Strength of the shake.</param>
        /// <param name="freq">Frequency of shaking.</param>
        /// <param name="numBounces">Number of vibrations before stop.</param>
        public void ShortShake3D(float strength = 0.3f, float freq = 25f, int numBounces = 5)
        {
            BounceShake.Params pars = new()
            {
                AxesMultiplier = new Displacement(Vector3.zero, new Vector3(1, 1, 0.4f)),
                RotationStrength = strength,
                Freq = freq,
                NumBounces = numBounces
            };
            _shaker.RegisterShake(new BounceShake(pars));
        }

        /// <summary>
        /// Suitable for longer and stronger shakes in 2D. Moves camera in X and Y axes and rotates it in Z axis.
        /// </summary>
        /// <param name="positionStrength">Strength of motion in X and Y axes.</param>
        /// <param name="rotationStrength">Strength of rotation in Z axis.</param>
        /// <param name="duration">Duration of the shake.</param>
        public void Explosion2D(float positionStrength = 1f, float rotationStrength = 3f, float duration = 0.5f)
        {
            PerlinShake.NoiseMode[] modes =
            {
                new(8, 1),
                new(20, 0.3f)
            };
            Envelope.EnvelopeParams envelopePars = new()
            {
                Decay = duration <= 0f ? 1f : 1f / duration
            };
            PerlinShake.Params pars = new()
            {
                Strength = new Displacement(new Vector3(1, 1) * positionStrength, Vector3.forward * rotationStrength),
                NoiseModes = modes,
                Envelope = envelopePars,
            };
            _shaker.RegisterShake(new PerlinShake(pars));
        }

        /// <summary>
        /// Suitable for longer and stronger shakes in 3D. Rotates camera in all three axes. 
        /// </summary>
        /// <param name="strength">Strength of the shake.</param>
        /// <param name="duration">Duration of the shake.</param>
        public void Explosion3D(float strength = 8f, float duration = 0.7f)
        {
            PerlinShake.NoiseMode[] modes =
            {
                new(6, 1),
                new(20, 0.2f)
            };
            Envelope.EnvelopeParams envelopePars = new()
            {
                Decay = duration <= 0f ? 1f : 1f / duration
            };
            PerlinShake.Params pars = new()
            {
                Strength = new Displacement(Vector3.zero, new Vector3(1, 1, 0.5f) * strength),
                NoiseModes = modes,
                Envelope = envelopePars,
            };
            _shaker.RegisterShake(new PerlinShake(pars));
        }
    }
}

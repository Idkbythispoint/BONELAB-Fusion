﻿using LabFusion.Extensions;
using LabFusion.Scene;
using LabFusion.Utilities;
using LabFusion.Math;

using UnityEngine;

namespace LabFusion.Data;

public class PDController
{
    // Clamping values to make sure forces and torques don't get out of hand
    private const float MaxForce = 5000f;
    private const float MaxTorque = 1000f;

    // Tweak these values to control movement properties of all synced objects
    private const float PositionFrequency = 10f;
    private const float PositionDamping = 3f;

    private const float RotationFrequency = 100f;
    private const float RotationDamping = 10f;

    // Calculated KP and KD values for adding forces. These are only calculated once
    private static float _positionKp;
    private static float _positionKd;

    private static float _rotationKp;
    private static float _rotationKd;

    // Calculated KSG and KDG values to multiply the forces, these are calculated once per frame
    private static float _positionKsg;
    private static float _positionKdg;

    private static float _rotationKsg;
    private static float _rotationKdg;

    // Last frame value
    private static float _lastFixedDelta = 1f;

    // Derivatives
    private bool _validPosition = false;
    private bool _validRotation = false;

    private Vector3 _lastVelocity = Vector3Extensions.zero;
    private Vector3 _lastAngularVelocity = Vector3Extensions.zero;

    private Vector3 _lastPosition = Vector3Extensions.zero;
    private Quaternion _lastRotation = QuaternionExtensions.identity;

    public Vector3 SavedForce { get; set; }
    public Vector3 SavedTorque { get; set; }

    public bool ValidPosition => _validPosition;

    public bool ValidRotation => _validRotation;

    public static void OnInitializeMelon()
    {
        _positionKp = CalculateKP(PositionFrequency);
        _positionKd = CalculateKD(PositionFrequency, PositionDamping);

        _rotationKp = CalculateKP(RotationFrequency);
        _rotationKd = CalculateKD(RotationFrequency, RotationDamping);
    }

    public static void OnFixedUpdate()
    {
        float dt = TimeUtilities.FixedDeltaTime;

        // Make sure the deltaTime has changed
        if (ManagedMathf.Approximately(dt, _lastFixedDelta))
        {
            return;
        }

        _lastFixedDelta = dt;

        // Position
        float pG = 1f / (1f + _positionKd * dt + _positionKp * dt * dt);
        _positionKsg = _positionKp * pG;
        _positionKdg = (_positionKd + _positionKp * dt) * pG;

        // Rotation
        float rG = 1f / (1f + _rotationKd * dt + _rotationKp * dt * dt);
        _rotationKsg = _rotationKp * rG;
        _rotationKdg = (_rotationKd + _rotationKp * dt) * rG;
    }

    private static float CalculateKP(float frequency)
    {
        return (6f * frequency) * (6f * frequency) * 0.25f;
    }

    private static float CalculateKD(float frequency, float damping)
    {
        return 4.5f * frequency * damping;
    }

    public void ResetPosition()
    {
        _validPosition = false;

        SavedForce = Vector3.zero;
    }

    public void ResetRotation()
    {
        _validRotation = false;

        SavedTorque = Vector3.zero;
    }

    public void Reset()
    {
        ResetPosition();
        ResetRotation();
    }

    public Vector3 GetForce(in Vector3 position, in Vector3 velocity, in Vector3 targetPos, in Vector3 targetVel)
    {
        if (!NetworkTransformManager.IsInBounds(targetPos))
        {
            return Vector3.zero;
        }

        var limitedTargetVel = NetworkTransformManager.LimitVelocity(targetVel);

        // Update derivatives if needed
        if (!_validPosition)
        {
            _lastPosition = targetPos;
            _lastVelocity = limitedTargetVel;
            _validPosition = true;
        }

        Vector3 Pt0 = position;
        Vector3 Vt0 = velocity;

        Vector3 Pt1 = _lastPosition;
        Vector3 Vt1 = _lastVelocity;

        var force = (Pt1 - Pt0) * _positionKsg + (Vt1 - Vt0) * _positionKdg;

        // Acceleration
        force += (limitedTargetVel - _lastVelocity) / _lastFixedDelta;
        _lastVelocity = limitedTargetVel;

        _lastPosition = targetPos;

        // Clamp force
        force = Vector3.ClampMagnitude(force, MaxForce);

        return force;
    }

    public Vector3 GetTorque(in Quaternion rotation, in Vector3 angularVelocity, in Quaternion targetRot, in Vector3 targetVel)
    {
        // Update derivatives if needed
        if (!_validRotation)
        {
            _lastRotation = targetRot;
            _lastAngularVelocity = targetVel;
            _validRotation = true;
        }

        Quaternion Qt1 = _lastRotation;
        Vector3 Vt1 = _lastAngularVelocity;

        Quaternion q = Qt1 * Quaternion.Inverse(rotation);
        if (q.w < 0)
        {
            q.x = -q.x;
            q.y = -q.y;
            q.z = -q.z;
            q.w = -q.w;
        }
        q.ToAngleAxis(out float xMag, out Vector3 x);
        x = Vector3.Normalize(x);

        x *= ManagedMathf.Deg2Rad;
        var torque = _rotationKsg * x * xMag + _rotationKdg * (Vt1 - angularVelocity);

        // Acceleration
        torque += (targetVel - _lastAngularVelocity) / _lastFixedDelta;
        _lastAngularVelocity = targetVel;

        _lastRotation = targetRot;

        // Clamp torque
        torque = Vector3.ClampMagnitude(torque, MaxTorque);

        return torque;
    }
}
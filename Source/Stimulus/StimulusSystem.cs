
using System.Collections.Generic;
using System;
using UnityEngine;

public class Emission
{
    public StimulusType Type { get; private set; }
    public float Intensity { get; private set; }
    public Vector3 Position { get; private set; }
    public float Radius { get; private set; }

    public Emission(StimulusType type, float intensity, Vector3 position, float radius)
    {
        Type = type;
        Intensity = intensity;
        Position = position;
        Radius = radius;
    }
}

public enum StimulusType
{
    Pierce,
    Fire,
    Sound,
    Impact,
    Contact,
    Shockwave,
    Explosion,
}

public interface IStimEmitter
{
    //
}



public interface IStimListener
{
    //
}
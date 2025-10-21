using UnityEngine;
using System.Collections.Generic;

public class WeaponStimEmitter : MonoBehaviour, IStimEmitter
{
    [SerializeField] private float emissionRadius = 10f;
    [SerializeField] private float fireStimIntensity = 10f;
    [SerializeField] private float soundStimIntensity = 8f;

    #region Public API
    public void GunFireStimulus(Vector3 position, float pierceIntensity, RaycastHit rayHit)
    {
        // fire from muzzle flash
        var fireEmission = new Emission(StimulusType.Fire, fireStimIntensity, position, emissionRadius);
        // sound from gunshot
        var soundEmission = new Emission(StimulusType.Sound, soundStimIntensity, position, emissionRadius);
        // pierce from bullet impact
        var pierceEmission = new Emission(StimulusType.Pierce, pierceIntensity, position, emissionRadius);
        // Impact from bullet impact
        var impactEmission = new Emission(StimulusType.Impact, pierceIntensity, position, emissionRadius);

        ProcessFireStimulus(fireEmission);
        ProcessSoundStimulus(soundEmission);
        ProcessPierceStimulus(pierceEmission, rayHit);
        ProcessImpactStimulus(impactEmission, rayHit);
    }
    #endregion

    #region Private API
    private void ProcessPierceStimulus(Emission _emission, RaycastHit _rayHit = default)
    {
        // Handle the individual emission
        Debug.Log($"Processing Pierce Emission: Type={_emission.Type}, Intensity={_emission.Intensity}, Position={_emission.Position}");

        if (_rayHit.collider != null)
        {
            Debug.Log($"Pierce Raycast Hit: {_rayHit.collider.name} at {_rayHit.point}");
        }
    }

    private void ProcessFireStimulus(Emission _emission)
    {
        // Handle the individual emission
        Debug.Log($"Processing Fire Emission: Type={_emission.Type}, Intensity={_emission.Intensity}, Position={_emission.Position}");
    }

    private void ProcessSoundStimulus(Emission _emission)
    {
        // Handle the individual emission
        Debug.Log($"Processing Sound Emission: Type={_emission.Type}, Intensity={_emission.Intensity}, Position={_emission.Position}");
    }
    
    private void ProcessImpactStimulus(Emission _emission, RaycastHit _rayHit = default)
    {
        // Handle the individual emission
        Debug.Log($"Processing Impact Emission: Type={_emission.Type}, Intensity={_emission.Intensity}, Position={_emission.Position}");

        if (_rayHit.collider != null)
        {
            Debug.Log($"Impact Raycast Hit: {_rayHit.collider.name} at {_rayHit.point}");
        }
    }
    #endregion
}
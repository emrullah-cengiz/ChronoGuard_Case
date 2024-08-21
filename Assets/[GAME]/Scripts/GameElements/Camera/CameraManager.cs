using System;
using System.Collections.Generic;
using Cinemachine;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Unity.VisualScripting;
using UnityEngine;

public class CameraManager : SerializedMonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _vcam;
    [OdinSerialize] [NonSerialized] private Dictionary<NoiseType, CamNoiseSetting> _camNoiseSettings;

    private CinemachineBasicMultiChannelPerlin _vcNoise;
    private void OnEnable()
    {
        Events.Player.OnDamageTake += OnDamageTake;
        // Events.Player.OnWeaponFire += OnWeaponFire;
    }

    private void Awake()
    {
        _vcNoise = _vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    private void OnWeaponFire()
    {
        ShakeCam(NoiseType.WeaponFire).Forget();
    }

    private void OnDamageTake(int damage, int health)
    {
        ShakeCam(NoiseType.DamageTake).Forget();
    }

    private async UniTaskVoid ShakeCam(NoiseType noiseType)
    {
        var setting = _camNoiseSettings[noiseType];

        _vcNoise.m_AmplitudeGain = setting.Amplitude;
        _vcNoise.m_FrequencyGain = setting.Frequency;
        
        await UniTask.Delay((int)(setting.Duration * 1000));
        
        _vcNoise.m_AmplitudeGain = 0;
        _vcNoise.m_FrequencyGain = 0;
    } 

    public class CamNoiseSetting
    {
        public float Duration;
        public float Amplitude;
        public float Frequency;
    }

    public enum NoiseType
    {
        WeaponFire,
        DamageTake,
    }
}

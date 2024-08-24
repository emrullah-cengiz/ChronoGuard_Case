using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimatorController : CharacterAnimatorController
{
    private readonly int ATTACK_PARAM = Animator.StringToHash("Attack");
    private readonly int ATTACK_TYPE_PARAM = Animator.StringToHash("AttackType");

    private const string ATTACK_SPEED_PARAM_FORMAT = "Attack{0}_Speed";

    private Dictionary<EnemyAttackType, float> _attackClipSpeeds;

    protected override void _Awake()
    {
        _attackClipSpeeds = new();
    }

    public void SetAttackSpeedByAttackRate(EnemyAttackType attackType, float rate)
    {
        if (rate > 1)
        {
            _animator.SetFloat(string.Format(ATTACK_SPEED_PARAM_FORMAT, (int)attackType), 1);
            _attackClipSpeeds[attackType] = 1;
            return;
        }

        var clipDuration = GetAttackClipDuration(attackType);

        var speed = clipDuration / rate;

        _animator.SetFloat(string.Format(ATTACK_SPEED_PARAM_FORMAT, (int)attackType), speed);
        _attackClipSpeeds[attackType] = speed;
    }

    private float GetAttackClipDuration(EnemyAttackType attackType)
    {
        var clips = _animator.runtimeAnimatorController.animationClips;

        foreach (var clip in clips)
            if (clip.name == Enum.GetName(typeof(EnemyAttackType), attackType))
                return clip.length;

        throw new Exception($"No animation clip found for attack type {Enum.GetName(typeof(EnemyAttackType), attackType)}");
    }

    public void TriggerAttack(EnemyAttackType type)
    {
        _animator.SetInteger(ATTACK_TYPE_PARAM, (int)type);
        _animator.SetTrigger(ATTACK_PARAM);
    }

    public float GetAttackClipSpeed(EnemyAttackType type) => _attackClipSpeeds[type];
}
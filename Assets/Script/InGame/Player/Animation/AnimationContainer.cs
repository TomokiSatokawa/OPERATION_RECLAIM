using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using UnityEngine;

namespace InGame.Player.Motion
{
    [CreateAssetMenu(fileName = "AnimationContainer", menuName = "Scriptable Objects/AnimationContainer")]
    public class AnimationContainer : ScriptableObject
    {
        [SerializeField] private AvatarMaskData[] _avatarMaskData;
        [SerializeField] private AnimationData<BasicAnimationType>[] _basicClip;
        [SerializeField] private AnimationData<PlayerRifleAnimationType>[] _playerRifleClip;
        [SerializeField] private AnimationData<EnemyRifleAnimationType>[] _enemyRifleClip;
        [SerializeField] private AnimationData<FootmanAnimationType>[] _footmanClip;

        private Dictionary<EnumID, IReadOnlyAnimationData> _animationDictionary = new();
        public IReadOnlyList<IReadOnlyAnimationData> AnimationList => _animationDictionary.Values.ToList();

        public void Initialize()
        {
            if (_animationDictionary.Count > 0) return;

            AddDictionary(new BasicMotion(), _basicClip);
            AddDictionary(new PlayerRifleMotion(), _playerRifleClip);
            AddDictionary(new EnemyRifleMotion(), _enemyRifleClip);
            AddDictionary(new FootmanMotion(), _footmanClip);
        }

        public IReadOnlyAnimationData GetAnimation(EnumGroupBase enumGroup)
        {
            return GetAnimation(enumGroup.Create());
        }

        public IReadOnlyAnimationData GetAnimation(EnumID id)
        {
            if (_animationDictionary.TryGetValue(id, out var result))
            {
                return result;
            }

            Debug.LogError(id.ToString() + " is not found");
            return null;
        }

        public AvatarMask GetMask(AvatarMaskType type)
        {
            foreach (var data in _avatarMaskData)
            {
                if (data.MaskType == type)
                    return data.Mask;
            }

            Debug.LogError(type.ToString() + " is not found");
            return null;
        }

        private void AddDictionary<T>(EnumGroupBase<T> group, AnimationData<T>[] clips) where T : Enum
        {
            foreach (var clipData in clips)
            {
                EnumID id = EnumID.Create(group, clipData.Name);
                _animationDictionary.Add(id, clipData);
            }
        }

        public interface IReadOnlyAnimationData
        {
            public AnimationClip Clip { get; }
            public AvatarMaskType AvatarMaskType { get; }
            public BlendData Blend { get; }
        }

        [System.Serializable]
        public class AnimationData<T> : IReadOnlyAnimationData where T : Enum
        {
            [SerializeField] private T _name;
            [SerializeField] private AnimationClip _clip;
            [SerializeField] private AvatarMaskType _avatarMaskData;
            [SerializeField] private BlendData _blendData;

            public T Name => _name;
            public AnimationClip Clip => _clip;
            public AvatarMaskType AvatarMaskType => _avatarMaskData;
            public BlendData Blend => _blendData;


        }

        [System.Serializable]
        public class AvatarMaskData
        {
            [SerializeField] private AvatarMaskType _maskType;
            [SerializeField] private AvatarMask _mask;

            public AvatarMaskType MaskType => _maskType;
            public AvatarMask Mask => _mask;
        }
    }

    [System.Serializable]
    public struct BlendData
    {
        public float Duration;
        public AnimationCurve Curve;
    }

    public enum AvatarMaskType
    {
        Full, Upper, Lower
    }

    public enum BasicAnimationType : byte
    {
        Idol, Forward, Right, Left, Back, Run
    }

    public enum PlayerRifleAnimationType : byte
    {
        Idol, Forward, Right, Left, Back, Run,Reload
    }

    public enum EnemyRifleAnimationType : byte
    {
        Idol, Forward, Right, Left,
    }

    public enum FootmanAnimationType : byte
    {
        Idol, Run, Attack
    }
}
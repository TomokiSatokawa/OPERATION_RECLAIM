using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using InGame.Player.Motion;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using static InGame.Player.Motion.AnimationContainer;

namespace Common.Motion
{
    public class AnimationPlayer : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private AnimationContainer _container;
        [SerializeField] private StateMotionData _stateMotionData;
        [SerializeField] private BlendData _baseBlend;
        [SerializeField] private MotionType _startMotionType;

        private CancellationTokenSource _topLayerToken;
        private AnimationPlaybackState _currentPlayOneShot;

         private BlendMixer _horizontalBlend;
        private BlendMixer _verticalBlend;
        private BlendMixer _axisBlend;
        private StateMotionData.MotionGroup _currentGroup;
        private enum AnimationLayer
        {
            Base, TopLayer
        }

        private PlayableGraph _graph;
        private AnimationLayerMixerPlayable _layerMixer;
        private AnimationMixerPlayable _horizontalMixer;
        private AnimationMixerPlayable _verticalMixer;
        private AnimationMixerPlayable _axisMixer;

        protected virtual void Start()
        {
            _container.Initialize();

            _graph = PlayableGraph.Create("PlayerAnimation");

            //レイヤーミキサー
            _layerMixer = AnimationLayerMixerPlayable.Create(_graph, Enum.GetNames(typeof(AnimationLayer)).Length);

            //OutPut処理
            var output = AnimationPlayableOutput.Create(_graph, "AnimationOutput", _animator);
            output.SetSourcePlayable(_layerMixer);

            //移動ミキサー
            _axisMixer = AnimationMixerPlayable.Create(_graph, 2);
            _horizontalMixer = AnimationMixerPlayable.Create(_graph, 3);
            _verticalMixer = AnimationMixerPlayable.Create(_graph, 4);

            UpdateBaseClip(_startMotionType);

            //左右ミキサーを登録
            _axisMixer.ConnectInput(0, _horizontalMixer, 0);
            _axisMixer.ConnectInput(1, _verticalMixer, 0);

            _axisMixer.SetInputWeight(0, 0.5f);
            _axisMixer.SetInputWeight(1, 0.5f);

            //ベースミキサーをレイヤーに登録
            _layerMixer.ConnectInput((int)AnimationLayer.Base, _axisMixer, 0);
            _layerMixer.SetInputWeight((int)AnimationLayer.Base, 1f);

            //ブレンドミキサー
            _axisBlend = new(_baseBlend, 2);
            _horizontalBlend = new(_baseBlend, 3);
            _verticalBlend = new(_baseBlend, 4);

            _graph.Play();
        }

        private void OnDestroy()
        {
            _graph.Destroy();
            _topLayerToken?.Cancel();
            _topLayerToken?.Dispose();
        }

        public void Tick(Vector2 moveAmount)
        {
            _horizontalBlend.Tick(_horizontalMixer, Mathf.Clamp(moveAmount.x, -1f, 1f) + 1f);
            _verticalBlend.Tick(_verticalMixer, Mathf.Clamp(moveAmount.y, -1f, 2f) + 1f);

            float horizontal = Mathf.Abs(moveAmount.x);
            float vertical = Mathf.Abs(moveAmount.y);
            float total = horizontal + vertical;

            float target = total > 0f   ? vertical / total : 0.5f;

            _axisBlend.Tick(_axisMixer, target);
        }

        public void UpdateBaseClip(MotionType type)
        {
            _currentGroup = _stateMotionData.GetMotionGroup(type);

            if (_currentGroup == null)
                return;

            var idle = AnimationClipPlayable.Create(_graph, _container.GetAnimation(_currentGroup.IdleClip).Clip);
            var idle2 = AnimationClipPlayable.Create(_graph, _container.GetAnimation(_currentGroup.IdleClip).Clip);
            var walkForward = AnimationClipPlayable.Create(_graph, _container.GetAnimation(_currentGroup.WalkForwardClip).Clip);
            var walkRight = AnimationClipPlayable.Create(_graph, _container.GetAnimation(_currentGroup.WalkRightClip).Clip);
            var walkLeft = AnimationClipPlayable.Create(_graph, _container.GetAnimation(_currentGroup.WalkLeftClip).Clip);
            var walkBack = AnimationClipPlayable.Create(_graph, _container.GetAnimation(_currentGroup.WalkBackClip).Clip);
            var run = AnimationClipPlayable.Create(_graph, _container.GetAnimation(_currentGroup.RunClip).Clip);

            TryDisconnect(_horizontalMixer, 0);
            TryDisconnect(_horizontalMixer, 1);
            TryDisconnect(_horizontalMixer, 2);

            TryDisconnect(_verticalMixer, 0);
            TryDisconnect(_verticalMixer, 1);
            TryDisconnect(_verticalMixer, 2);
            TryDisconnect(_verticalMixer, 3);

            _horizontalMixer.ConnectInput(0, walkLeft, 0);
            _horizontalMixer.ConnectInput(1, idle, 0);
            _horizontalMixer.ConnectInput(2, walkRight, 0);

            _verticalMixer.ConnectInput(0, walkBack, 0);
            _verticalMixer.ConnectInput(1, idle2, 0);
            _verticalMixer.ConnectInput(2, walkForward, 0);
            _verticalMixer.ConnectInput(3, run, 0);
        }

        public IReadOnlyAnimationPlaybackState PlayOneShot(EnumGroupBase groupBase)
        {
            //既に実行中だった場合はキャンセル
            _topLayerToken?.Cancel();
            _topLayerToken?.Dispose();
            _topLayerToken = new CancellationTokenSource();
            var playbackState = new AnimationPlaybackState();
            PlayOneShotAsync(groupBase, playbackState).Forget();
            return playbackState;
        }

        public void StopPlayOneShot()
        {
            _currentPlayOneShot?.OnStop();
        }

        public void PlayDie()
        {
            PlayDieAsync().Forget();
        }

        private async UniTask PlayDieAsync()
        {
            var animationData = _container.GetAnimation(_currentGroup.DieClip);

            var playable = AnimationClipPlayable.Create(_graph, animationData.Clip);

            TryDisconnect(_layerMixer, (int)AnimationLayer.TopLayer);
            _layerMixer.ConnectInput((int)AnimationLayer.TopLayer, playable, 0);
            _layerMixer.SetInputWeight((int)AnimationLayer.TopLayer, 0f);

            // Maskを設定
            var mask = _container.GetMask(animationData.AvatarMaskType);
            _layerMixer.SetLayerMaskFromAvatarMask((int)AnimationLayer.TopLayer, mask);

            float elapsedTime = 0f;
            float blendDuration = animationData.Blend.Duration;

            if (blendDuration <= 0f)
            {
                _layerMixer.SetInputWeight((int)AnimationLayer.TopLayer, 1f);
                return;
            }

            while (elapsedTime < blendDuration)
            {
                float progress = Mathf.Clamp01(elapsedTime / blendDuration);

                _layerMixer.SetInputWeight((int)AnimationLayer.TopLayer, progress);

                elapsedTime += Time.deltaTime;
                await UniTask.Yield(destroyCancellationToken);
            }

            // 最後に必ず1にする
            _layerMixer.SetInputWeight((int)AnimationLayer.TopLayer, 1f);
        }

        private async UniTask PlayOneShotAsync(EnumGroupBase groupBase, AnimationPlaybackState playbackState)
        {
            var animationData = _container.GetAnimation(groupBase);
            var playable = AnimationClipPlayable.Create(_graph, animationData.Clip);
            _currentPlayOneShot = playbackState;

            //単発クリップを専用レイヤーにセット
            TryDisconnect(_layerMixer, (int)AnimationLayer.TopLayer);
            _layerMixer.ConnectInput((int)AnimationLayer.TopLayer, playable, 0);
            _layerMixer.SetInputWeight((int)AnimationLayer.TopLayer, 0f);

            //Maskを設定
            var mask = _container.GetMask(animationData.AvatarMaskType);
            _layerMixer.SetLayerMaskFromAvatarMask((int)AnimationLayer.TopLayer, mask);

            float elapsedTime = 0f;
            float clipLength = animationData.Clip.length;
            float blendDuration = animationData.Blend.Duration;

            _currentPlayOneShot.StopAnimation += () =>
            {
                elapsedTime = clipLength - blendDuration;
            };

            try
            {
                //Animation中待機処理
                while (elapsedTime < clipLength)
                {
                    // 開始Blend
                    if (elapsedTime < blendDuration)
                    {
                        float normalizedTime = elapsedTime / blendDuration;
                        float curveValue = animationData.Blend.Curve.Evaluate(normalizedTime);

                        _layerMixer.SetInputWeight((int)AnimationLayer.TopLayer, curveValue);
                    }
                    // 終了Blend
                    else if (elapsedTime >= clipLength - blendDuration)
                    {
                        float normalizedTime = (elapsedTime - (clipLength - blendDuration)) / blendDuration;
                        float curveValue = animationData.Blend.Curve.Evaluate(normalizedTime);

                        _layerMixer.SetInputWeight((int)AnimationLayer.TopLayer, 1f - curveValue);
                    }
                    // Blendなし
                    else
                    {
                        _layerMixer.SetInputWeight((int)AnimationLayer.TopLayer, 1f);
                    }

                    await UniTask.Yield(cancellationToken: _topLayerToken.Token);
                    elapsedTime += Time.deltaTime;
                    playbackState.Time = elapsedTime;
                }
            }
            catch (OperationCanceledException)
            {

            }

            //元に戻す
            TryDisconnect(_layerMixer, (int)AnimationLayer.TopLayer);
            _layerMixer.SetInputWeight((int)AnimationLayer.TopLayer, 0f);
            playbackState.IsPlaying = false;
        }

        private void TryDisconnect(Playable playable, int inputPot)
        {
            if (playable.GetInput(inputPot).IsValid())
                playable.DisconnectInput(inputPot);
        }
    }

    [Serializable]
    public class BlendMixer
    {
      private BlendData _blendData;

        private float _blendPosition;
        private float _blendStart;
        private float _blendTarget;
        private float _blendTime;

        private int _inputCount;

        public BlendMixer(BlendData blendData,int inputCount)
        {
            _blendData = blendData;
            _inputCount = inputCount;
        }

        public void Tick(AnimationMixerPlayable axisMixer, float target)
        {
            if (!Mathf.Approximately(target, _blendTarget))
            {
                _blendStart = _blendPosition;
                _blendTarget = target;
                _blendTime = 0f;
            }

            if (!Mathf.Approximately(_blendStart, _blendTarget))
            {
                _blendTime += Time.deltaTime;

                var t = Mathf.Clamp01(_blendTime / _blendData.Duration);
                t = _blendData.Curve.Evaluate(t);

                _blendPosition = Mathf.Lerp(
                    _blendStart,
                    _blendTarget,
                    t
                );
            }

            SetBlendWeight(axisMixer, _blendPosition);
        }

        private void SetBlendWeight(AnimationMixerPlayable baseMixer, float blendPosition)
        {
            for (int i = 0; i < _inputCount; i++)
            {
                float weight = Mathf.Clamp01(1f - Mathf.Abs(blendPosition - i));
                baseMixer.SetInputWeight(i, weight);
            }
        }
    }
}

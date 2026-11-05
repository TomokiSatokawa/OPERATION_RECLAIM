using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using InGame.Player.Motion;

namespace Common.Motion
{
    [DefaultExecutionOrder(1000)] // 他のLateUpdate（反動など）より後
    public class PoseOverridePlayer : MonoBehaviour
    {
        [SerializeField] private Animator _animator;            // Rigが乗っているAnimator
        [SerializeField] private AnimationContainer _container; // AnimationPlayerと同じもの
        [SerializeField] private bool _freezeBase;   // false: Rigに追従して加算（推奨） / true: 開始時の姿勢に固定

        // 本物の骨格
        private Transform[] _bones;
        private Vector3[] _snapPos;
        private Quaternion[] _snapRot;

        // 評価用の複製骨格
        private GameObject _proxyRoot;
        private Animator _proxyAnimator;
        private Transform[] _proxyBones;
        private Vector3[] _p0;
        private Quaternion[] _r0;
        private bool[] _driven;
        private PlayableGraph _graph;
        private AnimationClipPlayable _clipPlayable;

        // 再生状態
        private AnimationPlaybackState _state;
        private AnimationClip _clip;
        private BlendData _blend;
        private float _speed;
        private AvatarMask _mask;
        private float _elapsed;
        private bool _pending;

        private float BlendTime => Mathf.Min(_blend.Duration, _clip.length * 0.5f);

        private void Start() => BuildProxy();

        private void OnDestroy()
        {
            if (_graph.IsValid()) _graph.Destroy();
            if (_proxyRoot != null) Destroy(_proxyRoot);
        }

        // ---- 公開API（AnimationPlayerと同じ形） ----

        public IReadOnlyAnimationPlaybackState PlayOneShot(EnumGroupBase groupBase)
        {
            if (_state != null) _state.IsPlaying = false;   // 再生中なら打ち切る

            var data = _container.GetAnimation(groupBase);
            _clip = data.Clip;
            _blend = data.Blend;
            _speed = data.Speed;
            _mask = _container.GetMask(data.AvatarMaskType);

            var state = new AnimationPlaybackState();
            state.StopAnimation += () =>
            {
                if (_state != state) return;   // 古い再生のStopは無視
                // 終了ブレンドの開始位置へ。すでに先にいるなら戻さない
                _elapsed = Mathf.Max(_elapsed, _clip.length - BlendTime);
            };

            _state = state;
            _pending = true;   // スナップショットはLateUpdateで取る
            return state;
        }

        public void StopPlayOneShot() => _state?.OnStop();

        // ---- 複製骨格 ----

        private void BuildProxy()
        {
            var real = new List<Transform>();
            var proxy = new List<Transform>();

            _proxyRoot = new GameObject("[PoseProxy]") { hideFlags = HideFlags.HideAndDontSave };
            CopyChildren(_animator.transform, _proxyRoot.transform, real, proxy);

            _proxyAnimator = _proxyRoot.AddComponent<Animator>();
            _proxyAnimator.avatar = _animator.avatar;
            _proxyAnimator.applyRootMotion = false;
            _proxyAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            _bones = real.ToArray();
            _proxyBones = proxy.ToArray();
            int n = _bones.Length;
            _snapPos = new Vector3[n]; _snapRot = new Quaternion[n];
            _p0 = new Vector3[n]; _r0 = new Quaternion[n];
            _driven = new bool[n];
        }

        // Animator自身のTransformは含めない（ルートに回転が掛からないように）
        private static void CopyChildren(Transform src, Transform dst, List<Transform> real, List<Transform> proxy)
        {
            foreach (Transform child in src)
            {
                var go = new GameObject(child.name) { hideFlags = HideFlags.HideAndDontSave };
                var t = go.transform;
                t.SetParent(dst, false);
                t.localPosition = child.localPosition;
                t.localRotation = child.localRotation;
                t.localScale = child.localScale;

                real.Add(child);
                proxy.Add(t);
                CopyChildren(child, t, real, proxy);
            }
        }

        private void BuildGraph()
        {
            if (_graph.IsValid()) _graph.Destroy();   // 毎回作り直す（マスクも初期化される）

            _graph = PlayableGraph.Create("PoseOverride");
            _graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);

            var mixer = AnimationLayerMixerPlayable.Create(_graph, 2);
            var output = AnimationPlayableOutput.Create(_graph, "Out", _proxyAnimator);
            output.SetSourcePlayable(mixer);

            _clipPlayable = AnimationClipPlayable.Create(_graph, _clip);
            _clipPlayable.SetSpeed(_speed);
            mixer.ConnectInput(1, _clipPlayable, 0);
            mixer.SetInputWeight(1, 1f);
            if (_mask != null) mixer.SetLayerMaskFromAvatarMask(1, _mask);

            _graph.Play();
        }

        private void Evaluate(float time)
        {
            _clipPlayable.SetTime(time);
            _graph.Evaluate(0f);
        }

        // ---- 再生 ----

        private void Begin()
        {
            _pending = false;
            int n = _bones.Length;

            // 1) Rig適用後の「今の姿勢」= 0基準
            for (int i = 0; i < n; i++)
            {
                _snapPos[i] = _bones[i].localPosition;
                _snapRot[i] = _bones[i].localRotation;
            }

            // 2) 複製骨格でクリップを評価（Animation設定・マスクはここで効く）
            BuildGraph();
            Evaluate(0f);
            for (int i = 0; i < n; i++)
            {
                _p0[i] = _proxyBones[i].localPosition;
                _r0[i] = _proxyBones[i].localRotation;
                _driven[i] = false;
            }

            // 3) 実際に動くボーンを洗い出す（マスク外・カーブ無しは触らない）
            float length = _clip.length;
            const float Step = 0.05f;
            for (float s = Step; ; s += Step)
            {
                float t = Mathf.Min(s, length);
                Evaluate(t);
                for (int i = 0; i < n; i++)
                {
                    if (_driven[i]) continue;
                    var b = _proxyBones[i];
                    if ((b.localPosition - _p0[i]).sqrMagnitude > 1e-8f ||
                        Quaternion.Angle(b.localRotation, _r0[i]) > 0.01f)
                        _driven[i] = true;
                }
                if (t >= length) break;
            }

            _elapsed = 0f;
        }

        private void LateUpdate()
        {
            if (_pending) Begin();
            if (_state == null) return;

            float length = _clip.length;
            float blend = BlendTime;

            // ブレンド重み（AnimationPlayerと同じ式）
            float w = 1f;
            if (blend > 0f)
            {
                if (_elapsed < blend)
                    w = _blend.Curve.Evaluate(_elapsed / blend);
                else if (_elapsed >= length - blend)
                    w = 1f - _blend.Curve.Evaluate((_elapsed - (length - blend)) / blend);
            }
            w = Mathf.Clamp01(w);

            Evaluate(Mathf.Min(_elapsed, length));

            for (int i = 0; i < _bones.Length; i++)
            {
                if (!_driven[i]) continue;

                var p = _proxyBones[i];

                // 0にする基準: 今のRig適用後の姿勢（freezeならスナップショット）
                Vector3 basePos = _freezeBase ? _snapPos[i] : _bones[i].localPosition;
                Quaternion baseRot = _freezeBase ? _snapRot[i] : _bones[i].localRotation;

                // 基準 + (クリップ(t) − クリップ1フレーム目)
                Vector3 pos = basePos + (p.localPosition - _p0[i]);
                Quaternion rot = baseRot * Quaternion.Inverse(_r0[i]) * p.localRotation;
                // 回転を親の空間の差分で足したい場合はこちら
                // Quaternion rot = (p.localRotation * Quaternion.Inverse(_r0[i])) * baseRot;

                _bones[i].localPosition = Vector3.Lerp(_bones[i].localPosition, pos, w);
                _bones[i].localRotation = Quaternion.Slerp(_bones[i].localRotation, rot, w);
            }

            _elapsed += Time.deltaTime * _speed;
            _state.Time = _elapsed;

            if (_elapsed >= length)
            {
                _state.IsPlaying = false;
                _state = null;
                if (_graph.IsValid()) _graph.Destroy();
            }
        }
    }
}
//AI
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace InGame.Enemy
{
    /// <summary>
    /// 指定レイヤーのRendererにアウトラインを描画するPassを登録するRendererFeature
    /// </summary>
    public class EnemyOutlineFeature : ScriptableRendererFeature
    {
        [SerializeField] private Material _outlineMaterial;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private RenderPassEvent _renderPassEvent = RenderPassEvent.AfterRenderingOpaques;

        private EnemyOutlinePass _pass;

        public override void Create()
        {
            if (_outlineMaterial == null)
            {
                Debug.LogError("[EnemyOutline] アウトライン用マテリアルが未設定です");
                _pass = null;
                return;
            }

            _pass = new EnemyOutlinePass(_outlineMaterial, _layerMask)
            {
                renderPassEvent = _renderPassEvent
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (_pass == null)
            {
                return;
            }

            renderer.EnqueuePass(_pass);
        }
    }
}
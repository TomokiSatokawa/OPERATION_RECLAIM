//AI
using UnityEngine;

namespace InGame.Teammate
{
    /// <summary>
    /// 仲間キャラのRendererをシルエット描画用レイヤーへ切り替え、壁越しシルエットの対象にする責務を持つ
    /// </summary>
    public class TeammateSilhouetteTarget : MonoBehaviour
    {
        [SerializeField] private string _silhouetteLayerName = "TeammateSilhouette";
        [SerializeField] private Renderer[] _targetRenderers;

        private int[] _originalLayers;

        private void Awake()
        {
            if (_targetRenderers == null || _targetRenderers.Length == 0)
            {
                _targetRenderers = GetComponentsInChildren<Renderer>(true);
            }

            _originalLayers = new int[_targetRenderers.Length];
            for (int i = 0; i < _targetRenderers.Length; i++)
            {
                _originalLayers[i] = _targetRenderers[i].gameObject.layer;
            }
        }

        private void OnEnable()
        {
            int layer = LayerMask.NameToLayer(_silhouetteLayerName);
            if (layer < 0)
            {
                Debug.LogError($"[TeammateSilhouetteTarget] レイヤーが存在しません: {_silhouetteLayerName}");
                return;
            }

            foreach (Renderer targetRenderer in _targetRenderers)
            {
                targetRenderer.gameObject.layer = layer;
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < _targetRenderers.Length; i++)
            {
                if (_targetRenderers[i] == null)
                {
                    continue;
                }

                _targetRenderers[i].gameObject.layer = _originalLayers[i];
            }
        }
    }
}

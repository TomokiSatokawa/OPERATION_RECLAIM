using UnityEngine;

namespace InGame.Stage
{
    /// <summary>
    /// 1F・2Fのステージ階層に関するLayerとY座標を提供する
    /// </summary>
    public class StageLayerProvider : MonoBehaviour
    {
        [Header("Layer")]
        [SerializeField] private LayerMask _firstFloorLayer;
        [SerializeField] private LayerMask _secondFloorLayer;
        [SerializeField] private LayerMask _sharedLayer;

        [Header("Floor Height")]
        [SerializeField] private Transform _firstFloorTransform;
        [SerializeField] private Transform _secondFloorTransform;

        /// <summary>1F用の合成Layer(1F固有 + 共有)</summary>
        public LayerMask FirstFloorCombinedLayer => _firstFloorLayer | _sharedLayer;

        /// <summary>2F用の合成Layer(2F固有 + 共有)</summary>
        public LayerMask SecondFloorCombinedLayer => _secondFloorLayer | _sharedLayer;

        /// <summary>1FのY座標</summary>
        public float FirstFloorY => GetY(_firstFloorTransform, nameof(_firstFloorTransform));

        /// <summary>2FのY座標</summary>
        public float SecondFloorY => GetY(_secondFloorTransform, nameof(_secondFloorTransform));

        private float GetY(Transform floorTransform, string fieldName)
        {
            if (floorTransform == null)
            {
                Debug.LogError($"[StageLayerProvider] {fieldName} が未設定です");
                return 0f;
            }

            return floorTransform.position.y;
        }
    }
}
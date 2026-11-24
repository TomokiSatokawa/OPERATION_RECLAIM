using CoreKit.ObjectPool;
using InGame.Bot.Perception;
using UnityEngine;

namespace InGame.UI.Map
{
    /// <summary>
    /// SoundSensorで追加された音の聞こえる範囲を、マップ上に円のUIで可視化する
    /// </summary>
    public class SoundIndicator : MonoBehaviour
    {
        [SerializeField] private PoolObject _prefab;
        [SerializeField] private PoolManager _poolManager;
        [SerializeField] private RectTransform _content;
        [SerializeField] private RectTransform _mapRect;
        [SerializeField] private Camera _mapCamera;

        private void Start()
        {
            SoundSensor.OnAddSound += OnAddSound;
        }

        private void OnDestroy()
        {
            SoundSensor.OnAddSound -= OnAddSound;
        }

        /// <summary>
        /// World座標をマップUI上のanchoredPositionに変換する
        /// </summary>
        private Vector2 WorldToMapPosition(Vector3 worldPosition)
        {
            Vector3 viewport = _mapCamera.WorldToViewportPoint(worldPosition);
            var size = _mapRect.rect.size;
            return new Vector2((viewport.x - 0.5f) * size.x, (viewport.y - 0.5f) * size.y);
        }

        /// <summary>
        /// World座標の半径をマップUI上の半径に変換する
        /// </summary>
        private float WorldToMapRadius(Vector3 worldCenter, float worldRadius)
        {
            var edge = worldCenter + _mapCamera.transform.right * worldRadius;
            return Vector2.Distance(WorldToMapPosition(worldCenter), WorldToMapPosition(edge));
        }

        private void OnAddSound(SoundSensor.SoundData sound)
        {
            var indicator = _poolManager.Get(_prefab);
            if (indicator == null)
            {
                Debug.LogError("[SoundIndicator] PoolからIndicatorを取得できませんでした");
                return;
            }

            var rect = indicator.transform as RectTransform;
            rect.SetParent(_content, false);

            var radius = WorldToMapRadius(sound.Position, sound.Radius);
            rect.anchoredPosition = WorldToMapPosition(sound.Position);
            rect.sizeDelta = Vector2.one * radius * 2f;
        }
    }
}
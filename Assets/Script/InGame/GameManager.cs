using InGame.UI.Map;
using UnityEngine;

namespace InGame
{

    public class GameManager : SingletonMonoBehaviour<GameManager>
    {
        private void Start()
        {
            MousePointerControl.Locked();
        }

        private void OnDestroy()
        {
            MapPinRegistry.Dispose();
        }
    }
}
using UnityEngine;

namespace InGame.Player.Rifle
{
    public class RecoilCameraControl : MonoBehaviour
    {
        [SerializeField] private CameraController _controller;
        [SerializeField] private float _recoilAmount;
        [SerializeField] private float _recoilDurationOffset;
    }
}

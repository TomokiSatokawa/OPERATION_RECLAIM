using TMPro;
using UnityEngine;

namespace InGame.UI
{

    public class BulletCountView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _remainingBulletCount;

        public void UpdateRemainingBulletCount(int count)
        {
            _remainingBulletCount.text = count.ToString("D2");
        }
    }
}

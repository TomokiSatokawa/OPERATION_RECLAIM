using UnityEngine;

namespace InGame.Player.Rifle
{

    public class BulletController : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _deleteDistance;
        private Vector3 _targetPosition;

        private void FixedUpdate()
        {
            //•ûŒüŽw’è
            var direction = _targetPosition - this.transform.position;
            this.transform.up = direction;

            //ˆÚ“®(
            this.transform.position += this.transform.up * _moveSpeed;

            if((_targetPosition - this.transform.position).sqrMagnitude <  _deleteDistance * _deleteDistance)
            {
                Destroy(this.gameObject);
            }
        }

        public void SetData(Vector3 target)
        {
            _targetPosition = target;
        }
    }
}

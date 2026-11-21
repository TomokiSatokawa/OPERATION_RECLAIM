//AI
using InGame.Bot.StateMachine;
using UnityEngine;
using UnityEngine.AI;

namespace InGame.Enemy.StateMachine
{
    /// <summary>
    /// ターゲットを追跡するステート。視界内ならPlayerの位置、見失った後は予測位置へ向かう
    /// </summary>
    [System.Serializable]
    public class ChaseState : BotStateBase
    {
        private const float NavMeshSampleDistance = 2f;

        [SerializeField] private float _attackRange = 8f;
        [SerializeField] private float _lostDuration;

        private float _lostTimer;
        private Transform _target;

        protected override void OnEnter(StateContext context)
        {
            if (context.Perception == null)
            {
                Debug.LogError("[ChaseState] Perception が未設定です。");
                return;
            }

            _target = context.BotController.Perception.GetBestTarget();
        }

        protected override void OnUpdate(StateContext context)
        {
            if (context.BotController.Perception == null)
            {
                return;
            }

            var chase = context.BotController.Perception.TryGetTargetCurrentPosition(_target, out var position);

            if (chase)
            {
                ChaseVisibleTarget(context, position);
                return;
            }

            _target = context.BotController.Perception.GetBestTarget();

            //ターゲットがいない
            if (_target == null)
            {
                _lostTimer -= Time.deltaTime;

                if (_lostTimer <= 0)
                    RequestChangeState = nameof(PatrolState);
            }
            else
                _lostTimer = _lostDuration;
        }

        public override bool CanTransition(StateContext context)
        {
            // Playerが見えたら開始する
            return context.Perception != null && context.Perception.CanSeeTarget;
        }

        protected override void OnExit(StateContext context)
        {
            context.BotController.StopMove();
        }

        private void ChaseVisibleTarget(StateContext context, Vector3 position)
        {
            float sqrDistance = (position - context.Bot.position).sqrMagnitude;

            // 攻撃範囲に入ったら攻撃ステートへ
            if (context.BotController.Perception.CheckTargetVisibility(_target)
                && sqrDistance <= _attackRange * _attackRange)
            {
                RequestChangeState = nameof(AttackState);
                return;
            }

            MoveTo(context, position);
        }

        private void MoveTo(StateContext context, Vector3 position)
        {
            if (!NavMesh.SamplePosition(position, out NavMeshHit hit, NavMeshSampleDistance, NavMesh.AllAreas))
            {
                Debug.LogError($"[ChaseState] NavMesh上に目標位置が見つかりません。position={position}");
                return;
            }

            context.BotController.SetMoveTarget(hit.position, true);
        }
    }
}
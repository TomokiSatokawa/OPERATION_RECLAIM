using UnityEngine;
using UnityEngine.AI;

namespace InGame.Enemy.State
{
    /// <summary>
    /// 遮蔽物へ移動し、隠れる・身を出して撃つを繰り返す攻撃ステート
    /// </summary>
    [System.Serializable]
    public class AttackState : EnemyStateBase
    {
        private enum AttackSubState
        {
            MoveToCover,
            Hide,
            PeekAndShoot
        }

        private const float ArriveTolerance = 0.2f;
        private const float HideDurationMin = 1.5f;
        private const float HideDurationMax = 3f;
        private const float PeekDuration = 2f;
        private const float FireInterval = 0.4f;
        private const float RotateSpeed = 10f;

        private AttackSubState _subState;
        private CoverPoint _currentCover;
        private float _timer;
        private float _fireTimer;

        protected override void OnEnter(EnemyStateContext context)
        {
            _currentCover = CoverSelector.FindBestCover(
                context.Agent.transform.position, context.Player, context.Agent);

            if (_currentCover != null)
            {
                _currentCover.TryReserve(context.Agent);
            }

            ChangeSubState(AttackSubState.MoveToCover, context);
        }

        protected override void OnUpdate(EnemyStateContext context)
        {
            switch (_subState)
            {
                case AttackSubState.MoveToCover:
                    UpdateMoveToCover(context);
                    break;
                case AttackSubState.Hide:
                    UpdateHide(context);
                    break;
                case AttackSubState.PeekAndShoot:
                    UpdatePeekAndShoot(context);
                    break;
                default:
                    Debug.LogError($"[AttackState] 未対応のサブステート: {_subState}");
                    break;
            }
        }

        protected override void OnExit(EnemyStateContext context)
        {
            if (_currentCover != null)
            {
                _currentCover.Release(context.Agent);
                _currentCover = null;
            }

            context.Agent.ResetPath();
        }

        public override bool CanExit(EnemyStateContext context)
        {
            return true;
        }

        public override bool CanTransition(EnemyStateContext context)
        {
            return true;
        }

        private void ChangeSubState(AttackSubState next, EnemyStateContext context)
        {
            _subState = next;

            switch (next)
            {
                case AttackSubState.MoveToCover:
                    if (_currentCover == null)
                    {
                        // カバーが無い場合は、その場で撃つ
                        ChangeSubState(AttackSubState.PeekAndShoot, context);
                        return;
                    }
                    context.Agent.SetDestination(_currentCover.HidePosition);
                    break;

                case AttackSubState.Hide:
                    if (_currentCover == null)
                    {
                        ChangeSubState(AttackSubState.PeekAndShoot, context);
                        return;
                    }
                    _timer = Random.Range(HideDurationMin, HideDurationMax);
                    context.Agent.SetDestination(_currentCover.HidePosition);
                    break;

                case AttackSubState.PeekAndShoot:
                    _timer = PeekDuration;
                    _fireTimer = 0f;
                    if (_currentCover != null)
                    {
                        context.Agent.SetDestination(_currentCover.PeekPosition);
                    }
                    break;

                default:
                    Debug.LogError($"[AttackState] 未対応のサブステート: {next}");
                    break;
            }
        }

        private void UpdateMoveToCover(EnemyStateContext context)
        {
            if (HasArrived(context.Agent))
            {
                ChangeSubState(AttackSubState.Hide, context);
            }
        }

        private void UpdateHide(EnemyStateContext context)
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                ChangeSubState(AttackSubState.PeekAndShoot, context);
            }
        }

        private void UpdatePeekAndShoot(EnemyStateContext context)
        {
            _timer -= Time.deltaTime;

            // カバーありの場合は、身を出す位置に着くまで撃たない
            bool isReady = _currentCover == null || HasArrived(context.Agent);
            if (isReady)
            {
                FaceTarget(context);

                if (!CoverSelector.HasLineOfSight(context.Agent.transform.position, context.Player))
                {
                    // 射線が通らないなら撃たずに隠れ直す
                    ChangeSubState(AttackSubState.Hide, context);
                    return;
                }

                _fireTimer -= Time.deltaTime;
                if (_fireTimer <= 0f)
                {
                    Fire(context);
                    _fireTimer = FireInterval;
                }
            }

            if (_timer <= 0f)
            {
                ChangeSubState(AttackSubState.Hide, context);
            }
        }

        private void FaceTarget(EnemyStateContext context)
        {
            Transform enemy = context.Agent.transform;
            Vector3 direction = context.Player.position - enemy.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < Mathf.Epsilon)
            {
                return;
            }

            enemy.rotation = Quaternion.Slerp(
                enemy.rotation, Quaternion.LookRotation(direction), RotateSpeed * Time.deltaTime);
        }

        private void Fire(EnemyStateContext context)
        {
            Debug.Log("攻撃!!");
            // TODO: 弾の発射、AnimationPlayerでの射撃モーション再生
        }

        private static bool HasArrived(NavMeshAgent agent)
        {
            return !agent.pathPending
                && agent.remainingDistance <= agent.stoppingDistance + ArriveTolerance;
        }
    }
}
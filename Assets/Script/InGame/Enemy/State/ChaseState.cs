using UnityEngine;
using UnityEngine.AI;

namespace InGame.Enemy.State
{
    /// <summary>
    /// ターゲットを追跡するステート。視界内ならPlayerの位置、見失った後は予測位置へ向かう
    /// </summary>
    [System.Serializable]
    public class ChaseState : EnemyStateBase
    {
        private const float NavMeshSampleDistance = 2f;

        protected override void OnEnter(EnemyStateContext context)
        {
            if (context.Perception == null)
            {
                Debug.LogError("[ChaseState] Perception が未設定です。");
            }
        }

        protected override void OnUpdate(EnemyStateContext context)
        {
            if (context.Perception == null)
            {
                return;
            }

            context.Agent.SetDestination(GetChaseDestination(context));
        }

        public override bool CanExit(EnemyStateContext context)
        {
            // Perception が無い場合は追跡できないため、終了を許可する
            if (context.Perception == null)
                return true;

            // 見失っていて、予測の終点まで到達したら終了する
            return !context.Perception.CanSeeTarget
                && context.Perception.IsPredictionExpired
                && HasArrived(context);
        }

        public override bool CanTransition(EnemyStateContext context)
        {
            // Playerが見えたら開始する
            return context.Perception != null && context.Perception.CanSeeTarget;
        }

        protected override void OnExit(EnemyStateContext context)
        {
        }

        private Vector3 GetChaseDestination(EnemyStateContext context)
        {
            if (context.Perception.CanSeeTarget)
            {
                Debug.Log("見えている");
                return context.Player.position;
            }

            // 予測位置が壁の中などになる場合に備え、NavMesh上の最寄り位置へ補正する
            Vector3 predicted = context.Perception.PredictedPosition;
            if (NavMesh.SamplePosition(predicted, out NavMeshHit hit, NavMeshSampleDistance, NavMesh.AllAreas))
            {
                Debug.Log("予測");
                return hit.position;
            }

            Debug.Log("記憶");
            return context.Perception.LastKnownPosition;
        }
    }
}
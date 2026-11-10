using InGame.Bot;
using InGame.Player.Rifle;
using UnityEngine;

namespace InGame.Enemy.StateMachine
{
    /// <summary>
    /// ターゲットが見えている間、BotControllerで向き直して撃つステート。
    /// 弾切れになったらリロードする。
    /// 射程外になる、または見失った状態が続いたら追跡へ戻す
    /// </summary>
    [System.Serializable]
    public class AttackState : EnemyStateBase
    {
        [SerializeField] private float _attackRange = 8f;
        [SerializeField] private float _exitRangeMargin = 2f;
        [SerializeField] private float _lostDuration = 0.5f;

        private float _lostTimer;

        /// <summary>
        /// 開始条件: ターゲットが見えていて、射程内にいること
        /// </summary>
        public override bool CanTransition(StateContext context)
        {
            return IsTargetVisible(context) && IsWithinRange(context, _attackRange);
        }

        protected override void OnEnter(StateContext context)
        {
            _lostTimer = 0f;

            // 向き直して撃つので、移動中の場合はその場で止まる
            context.BotController.StopMove();
        }

        protected override void OnUpdate(StateContext context)
        {
            BotController bot = context.BotController;

            if (bot == null)
            {
                RequestChangeState = nameof(ChaseState);
                return;
            }

            if (bot.RifleData == null)
            {
                Debug.LogError($"[AttackState] 銃を持っていません: {bot.name}");
                RequestChangeState = nameof(ChaseState);
                return;
            }

            // 入る距離より余裕を持たせ、境界のばたつきを防ぐ
            if (!IsWithinRange(context, _attackRange + _exitRangeMargin))
            {
                RequestChangeState = nameof(ChaseState);
                return;
            }

            // 見えているかに関わらず、弾切れならリロードする
            ReloadIfEmpty(bot);

            if (IsTargetVisible(context))
            {
                _lostTimer = 0f;

                // リロード中も向きは追い続け、向き終わって撃てる状態になったら撃つ
                bool isFacing = bot.FaceTarget(context.Player.position);

                if (isFacing && CanShoot(bot))
                {
                    bot.Shot();
                }

                return;
            }

            // 見えていない間は撃たない。見失った状態が続いたら追跡へ戻す
            _lostTimer += Time.deltaTime;

            if (_lostTimer >= _lostDuration)
            {
                RequestChangeState = nameof(ChaseState);
            }
        }

        protected override void OnExit(StateContext context)
        {
            context.BotController.StopMove();
        }

        /// <summary>
        /// 装弾数が0で、リロード中でなければリロードを開始する
        /// </summary>
        private static void ReloadIfEmpty(BotController bot)
        {
            RifleData rifle = bot.RifleData;

            if (!rifle.Reloading.CurrentValue && rifle.RemainingBulletCount.CurrentValue <= 0)
            {
                bot.Reload();
            }
        }

        /// <summary>
        /// リロード中でなく、弾が残っているときだけ撃てる
        /// </summary>
        private static bool CanShoot(BotController bot)
        {
            RifleData rifle = bot.RifleData;

            return !rifle.Reloading.CurrentValue && rifle.RemainingBulletCount.CurrentValue > 0;
        }

        private static bool IsTargetVisible(StateContext context)
        {
            // 視認判定はここに集約。Perceptionの実際のAPIに合わせて差し替える
            return true;
        }

        private static bool IsWithinRange(StateContext context, float range)
        {
            Vector3 toPlayer = context.Player.position - context.Bot.transform.position;
            toPlayer.y = 0f;
            return toPlayer.sqrMagnitude <= range * range;
        }
    }
}
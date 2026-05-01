using UnityEngine;
using WildernessCultivation.Core;
using WildernessCultivation.World;

namespace WildernessCultivation.Mobs.States
{
    /// <summary>Rabbit passive wander + flee + eat FSM. Exemplar cho "flee-type" mob (Deer, Crow tương tự).</summary>
    public static class RabbitStates
    {
        public static readonly IState<RabbitAI> Wander = new RabbitWander();
        public static readonly IState<RabbitAI> Flee = new RabbitFlee();
        public static readonly IState<RabbitAI> Eat = new RabbitEat();
    }

    sealed class RabbitWander : IState<RabbitAI>
    {
        public void OnEnter(RabbitAI r) => r.ReseedWanderTargetIfNeeded();
        public void OnExit(RabbitAI r) { }
        public void OnTick(RabbitAI r, float dt)
        {
            // Gần player quá → flee.
            var hit = Physics2D.OverlapCircle(r.transform.position, r.fleeRange, r.playerMask);
            if (hit != null) { r.SetFleeFrom(hit.transform); r.Fsm.ChangeState(RabbitStates.Flee); return; }

            // Đói → tìm grass-tile gần nhất; nếu thấy + đã trong eatRange → Eat. Ngược lại MoveTowards tile.
            if (r.IsHungry)
            {
                var grass = r.FindNearestGrassTile();
                if (grass != null)
                {
                    float d = Vector2.Distance(r.transform.position, grass.transform.position);
                    if (d <= r.eatRange)
                    {
                        r.SetEatTarget(grass);
                        r.Fsm.ChangeState(RabbitStates.Eat);
                        return;
                    }
                    // Move toward grass thay vì wander random.
                    r.MoveTowards(grass.transform.position);
                    return;
                }
            }

            // Reseed wander target khi tới nơi hoặc hết interval.
            if (Time.time >= r.NextWanderAt || Vector2.Distance(r.transform.position, r.WanderTarget) < 0.2f)
                r.ReseedWanderTarget();

            r.MoveTowards(r.WanderTarget);
        }
    }

    sealed class RabbitFlee : IState<RabbitAI>
    {
        public void OnEnter(RabbitAI r) { }
        public void OnExit(RabbitAI r) { }
        public void OnTick(RabbitAI r, float dt)
        {
            // Player còn trong flee range?
            var hit = Physics2D.OverlapCircle(r.transform.position, r.fleeRange, r.playerMask);
            if (hit == null) { r.Fsm.ChangeState(RabbitStates.Wander); return; }

            Vector2 fleeDir = ((Vector2)r.transform.position - (Vector2)hit.transform.position).normalized;
            r.SetVelocity(fleeDir * (r.moveSpeed * 1.6f));
            r.FlipSpriteToward(fleeDir);
        }
    }

    /// <summary>
    /// Eat — rabbit dừng tại chỗ, head bob 5× via squash punch, sau eatDuration → harvest grass-tile
    /// + restore hunger. Player gần → bỏ Eat → Flee (giữ defensive priority).
    /// </summary>
    sealed class RabbitEat : IState<RabbitAI>
    {
        // Squash period: head bob xuống mỗi (eatDuration / 5) giây.
        const int HeadBobCount = 5;

        public void OnEnter(RabbitAI r)
        {
            r.StopMoving();
            // Head bob đầu tiên ngay khi vào Eat.
            r.Anim?.TriggerSquash(r.eatDuration / HeadBobCount);
        }

        public void OnExit(RabbitAI r)
        {
            r.ClearEatTarget();
        }

        public void OnTick(RabbitAI r, float dt)
        {
            // Player gần → bỏ Eat ngay (defensive priority cao hơn hunger).
            var hit = Physics2D.OverlapCircle(r.transform.position, r.fleeRange, r.playerMask);
            if (hit != null) { r.SetFleeFrom(hit.transform); r.Fsm.ChangeState(RabbitStates.Flee); return; }

            var target = r.EatTarget;
            if (target == null || target.IsEaten)
            {
                r.Fsm.ChangeState(RabbitStates.Wander);
                return;
            }

            float age = Time.time - r.EatStartTime;
            r.StopMoving();

            // Re-trigger squash mỗi period (head bob 5×).
            float period = r.eatDuration / HeadBobCount;
            int currentBob = Mathf.FloorToInt(age / period);
            int prevBob = Mathf.FloorToInt((age - dt) / period);
            if (currentBob > prevBob && currentBob < HeadBobCount)
            {
                r.Anim?.TriggerSquash(period);
            }

            // Hết duration → harvest + restore + back to Wander.
            if (age >= r.eatDuration)
            {
                target.Eat();
                r.RestoreHunger(r.hungerRestorePerEat);
                r.Fsm.ChangeState(RabbitStates.Wander);
            }
        }
    }
}

using UnityEngine;
using UnityEngine.AI;

namespace ClashOfGods.Gameplay.Units
{
    /// <summary>
    /// 单位AI控制器 - 基于行为树实现自动战斗（支持热重载）
    /// Unit AI controller - Behavior tree based auto-combat (Hot reload compatible)
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class UnitAIController : MonoBehaviour
    {
        [Header("AI Settings")]
        public float AggroRange = 10f;
        public float AttackRange = 2f;
        public float ScanInterval = 0.5f;

        [Header("References")]
        public Transform CurrentTarget;
        public NavMeshAgent NavAgent;

        [Header("State")]
        public AIState CurrentState = AIState.Idle;

        [Header("Debug")]
        public bool EnableDebugLogs = false;

        private UnitEntity unitEntity;
        private float scanTimer = 0f;
        private float attackTimer = 0f;

        private void Awake()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            unitEntity = GetComponent<UnitEntity>();
            NavAgent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            if (unitEntity == null || !unitEntity.IsAlive) return;
            UpdateAI();
        }

        private void UpdateAI()
        {
            scanTimer += Time.deltaTime;
            if (scanTimer >= ScanInterval)
            {
                ScanForEnemies();
                scanTimer = 0f;
            }

            switch (CurrentState)
            {
                case AIState.Idle: HandleIdleState(); break;
                case AIState.Moving: HandleMovingState(); break;
                case AIState.Attacking: HandleAttackingState(); break;
                case AIState.Fleeing: HandleFleeingState(); break;
            }
        }

        public void ScanForEnemies()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, AggroRange);
            float closestDistance = float.MaxValue;
            Transform closestEnemy = null;

            foreach (Collider col in colliders)
            {
                UnitEntity otherUnit = col.GetComponent<UnitEntity>();
                if (otherUnit != null && otherUnit.IsAlive && otherUnit.IsPlayer != unitEntity.IsPlayer)
                {
                    float distance = Vector3.Distance(transform.position, col.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEnemy = col.transform;
                    }
                }
            }

            if (closestEnemy != null)
            {
                CurrentTarget = closestEnemy;
                ChangeState(AIState.Moving);
            }
        }

        private void HandleIdleState()
        {
            if (NavAgent != null && NavAgent.isOnNavMesh) NavAgent.isStopped = true;
        }

        private void HandleMovingState()
        {
            if (CurrentTarget == null) { ChangeState(AIState.Idle); return; }
            
            UnitEntity targetUnit = CurrentTarget.GetComponent<UnitEntity>();
            if (targetUnit != null && !targetUnit.IsAlive) { CurrentTarget = null; ChangeState(AIState.Idle); return; }

            float distance = Vector3.Distance(transform.position, CurrentTarget.position);
            if (distance <= AttackRange) { ChangeState(AIState.Attacking); }
            else if (NavAgent != null && NavAgent.isOnNavMesh) { NavAgent.isStopped = false; NavAgent.SetDestination(CurrentTarget.position); }
        }

        private void HandleAttackingState()
        {
            if (CurrentTarget == null) { ChangeState(AIState.Idle); return; }
            
            UnitEntity targetUnit = CurrentTarget.GetComponent<UnitEntity>();
            if (targetUnit != null && !targetUnit.IsAlive) { CurrentTarget = null; ChangeState(AIState.Idle); return; }

            float distance = Vector3.Distance(transform.position, CurrentTarget.position);
            if (distance > AttackRange) { ChangeState(AIState.Moving); return; }

            if (NavAgent != null && NavAgent.isOnNavMesh) NavAgent.isStopped = true;

            Vector3 direction = (CurrentTarget.position - transform.position).normalized;
            if (direction != Vector3.zero) transform.rotation = Quaternion.LookRotation(direction);

            attackTimer += Time.deltaTime;
            float attackInterval = unitEntity.Stats != null ? 1f / unitEntity.Stats.AttackSpeed : 1f;
            if (attackTimer >= attackInterval) { ExecuteAttack(); attackTimer = 0f; }
        }

        private void HandleFleeingState()
        {
            if (CurrentTarget == null) { ChangeState(AIState.Idle); return; }
            Vector3 fleeDirection = (transform.position - CurrentTarget.position).normalized;
            Vector3 fleePosition = transform.position + fleeDirection * 10f;
            if (NavAgent != null && NavAgent.isOnNavMesh) { NavAgent.isStopped = false; NavAgent.SetDestination(fleePosition); }
        }

        public void ExecuteAttack()
        {
            if (CurrentTarget == null || unitEntity == null) return;
            unitEntity.Attack(CurrentTarget.gameObject);
        }

        private void ChangeState(AIState newState)
        {
            if (CurrentState != newState) CurrentState = newState;
        }

        public void ForceTaunt(Transform target)
        {
            CurrentTarget = target;
            ChangeState(AIState.Moving);
        }

        void OnScriptHotReload()
        {
            Debug.Log($"[HotReload] UnitAIController reloaded. State: {CurrentState}");
            InitializeComponents();
            ScanForEnemies();
        }

        static void OnScriptHotReloadNoInstance()
        {
            Debug.Log("[HotReload] UnitAIController static reload");
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, AggroRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, AttackRange);
        }
    }

    public enum AIState { Idle, Moving, Attacking, Fleeing }
}

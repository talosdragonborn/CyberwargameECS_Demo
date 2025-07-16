using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine; 

[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
public partial struct AttackSystem : ISystem
{
    private Unity.Mathematics.Random randomGenerator;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ConfirmedAttackRequest_Component>();
        state.RequireForUpdate<LastAttackResult_Component>();

        // Seed per il generatore di numeri casuali
        uint seed = (uint)System.DateTime.Now.Ticks;
        randomGenerator = new Unity.Mathematics.Random(seed);
        Debug.Log($"ProcessConfirmedAttackSystem OnCreate. Random Seed: {seed}");
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.TempJob);


        foreach (var (request, requestEntity) in
                 SystemAPI.Query<RefRO<ConfirmedAttackRequest_Component>>()
                     .WithEntityAccess())
        {
            AttackCardData card = request.ValueRO.CardUsed;
            Entity targetStructureEntity = request.ValueRO.TargetStructureEntity;

            Debug.Log($"[ATTACK SYSTEM] Processing attack: Card '{card.Name.ToString()}' on Target {targetStructureEntity}");

            LastAttackResult_Component attackResult = new LastAttackResult_Component
            {
                AttackerCardID = card.ID,
                AttackerCardName = card.Name,
                Outcome = AttackOutcome.None,
                DiscoveryStatus = AttackDiscoveryStatus.None,
                DamageDealtC = 0f, DamageDealtI = 0f, DamageDealtA = 0f,
                SuccessRollThreshold = card.SuccessStat,
                StealthRollThreshold = card.StealthStat,
                WasProcessed = false
            };

            if (state.EntityManager.HasComponent<StructureID_Component>(targetStructureEntity))
            {
                attackResult.TargetStructureName = state.EntityManager.GetComponentData<StructureID_Component>(targetStructureEntity).Name;
            }
            else
            {
                attackResult.TargetStructureName = new FixedString128Bytes("Bersaglio Sconosciuto");
            }

            attackResult.SuccessRollValue = randomGenerator.NextInt(1, 11);
            attackResult.StealthRollValue = randomGenerator.NextInt(1, 11);

            Debug.Log($"[ATTACK SYSTEM] Rolls - Success: {attackResult.SuccessRollValue} (Threshold: {card.SuccessStat}), Stealth: {attackResult.StealthRollValue} (Threshold: {card.StealthStat})");

            bool isSuccess = attackResult.SuccessRollValue <= card.SuccessStat;
            attackResult.Outcome = isSuccess ? AttackOutcome.Success : AttackOutcome.Failure;

            bool isStealthy = attackResult.StealthRollValue <= card.StealthStat;
            attackResult.DiscoveryStatus = isStealthy ? AttackDiscoveryStatus.Stealthy : AttackDiscoveryStatus.Discovered;

            if (isSuccess)
            {
                Debug.Log("[ATTACK SYSTEM] Attack SUCCESSFUL!");
                if (state.EntityManager.HasComponent<CurrentHealthCIA_Component>(targetStructureEntity))
                {
                    CurrentHealthCIA_Component currentCIA = state.EntityManager.GetComponentData<CurrentHealthCIA_Component>(targetStructureEntity);
                    
                    float damageToApplyC = card.DamageConfidentiality;
                    float damageToApplyI = card.DamageIntegrity;
                    float damageToApplyA = card.DamageAvailability;

                    currentCIA.Confidentiality = math.max(0f, currentCIA.Confidentiality - damageToApplyC);
                    currentCIA.Integrity = math.max(0f, currentCIA.Integrity - damageToApplyI);
                    currentCIA.Availability = math.max(0f, currentCIA.Availability - damageToApplyA);

                    attackResult.DamageDealtC = damageToApplyC;
                    attackResult.DamageDealtI = damageToApplyI;
                    attackResult.DamageDealtA = damageToApplyA;

                    ecb.SetComponent(targetStructureEntity, currentCIA);
                    Debug.Log($"[ATTACK SYSTEM] Damage Applied. New Health: C:{currentCIA.Confidentiality:F0}, I:{currentCIA.Integrity:F0}, A:{currentCIA.Availability:F0}");
                }
                else { Debug.LogWarning($"[ATTACK SYSTEM] Target entity {targetStructureEntity} no CurrentHealthCIA_Component!"); }
            }
            else { Debug.Log("[ATTACK SYSTEM] Attack FAILED."); }

            SystemAPI.SetSingleton(attackResult);
            ecb.DestroyEntity(requestEntity);
            Debug.Log($"[ATTACK SYSTEM] Attack request {requestEntity} processed and destroyed.");
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    public void OnDestroy(ref SystemState state) { }
}
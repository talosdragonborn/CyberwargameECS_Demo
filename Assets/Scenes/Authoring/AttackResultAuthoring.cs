using Unity.Entities;
using Unity.Collections; // Per FixedString
using UnityEngine;

public class AttackResultAuthoring : MonoBehaviour
{
    // Non abbiamo bisogno di campi da esporre nell'Inspector per questo,
    // perché il componente verrà popolato dinamicamente.

    class Baker : Baker<AttackResultAuthoring>
    {
        public override void Bake(AttackResultAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None); // Entità puramente logica

            // Aggiungi LastAttackResult_Component con valori di default/iniziali
            AddComponent(entity, new LastAttackResult_Component
            {
                AttackerCardID = 0,
                AttackerCardName = new FixedString128Bytes("N/A"),
                TargetStructureName = new FixedString128Bytes("N/A"),
                Outcome = AttackOutcome.None,
                DiscoveryStatus = AttackDiscoveryStatus.None,
                DamageDealtC = 0f,
                DamageDealtI = 0f,
                DamageDealtA = 0f,
                SuccessRollValue = 0,
                SuccessRollThreshold = 0,
                StealthRollValue = 0,
                StealthRollThreshold = 0,
                WasProcessed = true // Inizialmente è "già processato" finché un attacco non lo aggiorna
            });

            // (Opzionale) Potremmo aggiungere un tag specifico per trovarlo più facilmente
            // se non volessimo fare affidamento su SystemAPI.GetSingleton<T>()
            // AddComponent<AttackResultSingleton_Tag>(entity);
        }
    }
}
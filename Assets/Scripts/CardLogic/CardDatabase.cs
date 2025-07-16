using System.Collections.Generic; // Per List<>
using UnityEngine;
using Unity.Collections; // Per Fixed

public class CardDatabase : MonoBehaviour
{
    // Lista  di AttackCardData grazie a [System.Serializable] sulla struct
    public List<AttackCardData> availableAttackCards = new List<AttackCardData>();


    void Awake() // Awake per assicurare che sia popolato prima dello Start di altri script
    {
        // Popoliamo la lista solo se è vuota
        if (availableAttackCards.Count == 0)
        {
            PopulateCardsProgrammatically();
        }
    }

    void PopulateCardsProgrammatically()
    {
        availableAttackCards.Add(new AttackCardData(
            101,                                // ID
            new FixedString64Bytes("Escavation"), // Name 
            2,                                  // ResourceCost
            20, 0, 0,                            // Damage C, I, A
            7,                                  // SuccessStat (soglia D10)
            6                                   // StealthStat (soglia D10)
        ));

        availableAttackCards.Add(new AttackCardData(
            102,
            new FixedString64Bytes("Indentify Spoofing"),
            1,
            20, 20, 0,                           
            6,
            5
        ));

        availableAttackCards.Add(new AttackCardData(
            103,
            new FixedString64Bytes("Adversary in the Middle"),
            3,
            0, 30, 20,                           
            5,
            3
        ));
;

        Debug.Log($"CardDatabase: Popolate {availableAttackCards.Count} carte via codice.");
    }


    // Metodo per ottenere una carta per ID 
    public bool TryGetCardData(int cardID, out AttackCardData cardData)
    {
        foreach (var card in availableAttackCards)
        {
            if (card.ID == cardID)
            {
                cardData = card;
                return true;
            }
        }
        cardData = default; // Restituisce una struct vuota se non trovata
        return false;
    }





}

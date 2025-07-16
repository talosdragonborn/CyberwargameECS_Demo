using Unity.Entities;
using Unity.Collections; // Per FixedStringBytes

public enum AttackOutcome { None, Success, Failure } // Per l'esito generale
public enum AttackDiscoveryStatus { None, Stealthy, Discovered } // Per lo stato di scoperta

public struct LastAttackResult_Component : IComponentData
{
    public int AttackerCardID; // ID della carta che ha attaccato
    public FixedString128Bytes AttackerCardName; // Nome della carta usata
    public FixedString128Bytes TargetStructureName; // Nome della struttura target

    public AttackOutcome Outcome;
    public AttackDiscoveryStatus DiscoveryStatus;

    public float DamageDealtC;
    public float DamageDealtI;
    public float DamageDealtA;

    public int SuccessRollValue; // Valore del dado per il successo
    public int SuccessRollThreshold; // Soglia che doveva superare/eguagliare
    public int StealthRollValue; // Valore del dado per lo stealth
    public int StealthRollThreshold; // Soglia che doveva superare/eguagliare

    public bool WasProcessed; // Flag per indicare se la UI ha già letto questo risultato

}
using UnityEngine;
public interface IUtilityAction
{
    string Name { get; }
    float GetScore(EcosystemManager animal);
    void Execute(EcosystemManager animal);
}
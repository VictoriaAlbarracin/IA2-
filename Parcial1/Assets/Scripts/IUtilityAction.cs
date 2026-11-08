using UnityEngine;
public interface IUtilityAction
{
    string Name { get; }
    float GetScore(Animal animal);
    void Execute(Animal animal);
}
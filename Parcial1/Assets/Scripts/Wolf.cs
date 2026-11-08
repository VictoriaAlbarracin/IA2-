using System.Collections.Generic;
using UnityEngine;

public class Wolf : Animal
{
    protected override void InitBrain()
    {
        _actions = new List<IUtilityAction>
        {
            new PatrolAction(),
            new DrinkAction(),
            new RestAction()
        };
    }
}
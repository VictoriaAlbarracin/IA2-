using System.Collections.Generic;
using UnityEngine;

public class Sheep : Animal
{
    protected override void InitBrain()
    {
        _actions = new List<IUtilityAction>
        {
            new PatrolAction(),
            new EatAction(),
            new DrinkAction(),
            new RestAction()
        };
    }
}

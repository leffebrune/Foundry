using System.Collections.Generic;
using UnityEngine;

namespace Foundry
{
    [CreateAssetMenu(fileName = "EntityBlueprint_", menuName = "MyGame/Entity Blueprint")]
    public class EntityBlueprint : ScriptableObject
    {
        public string EntityType;

        public List<EntityComponentData> Components;
    }
}
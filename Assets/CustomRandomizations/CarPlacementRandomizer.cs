using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Randomizers;
using UnityEngine.Perception.Randomization.Samplers;
using Object = UnityEngine.Object;

namespace CustomRandomizations
{
    /// <summary>
    /// Creates a 2D layer of evenly spaced GameObjects from a given list of prefabs
    /// </summary>
    [Serializable]
    [AddRandomizerMenu("Car Placement Randomizer")]
    public class CarPlacementRandomizer : Randomizer
    {
        /// <summary>
        /// The Samplers used to place objects in 3D space. Defaults to a uniform distribution in x, normal distribution
        /// in y, and constant value in z. These Samplers can be modified from the Inspector or via code.
        /// </summary>
        public Vector3Parameter positionDistribution = new()
        {
            x = new NormalSampler(-1f, 1f, 0f, .5f),
            y = new UniformSampler(-1f, 1f),
            z = new ConstantSampler(2f)
        };

        public FloatParameter Gap;
        /// <summary>
        /// The sampler controlling the number of objects to place.
        /// </summary>
        public IntegerParameter objectCount = new() { value = new ConstantSampler(10f) };

        /// <summary>
        /// The list of Prefabs to choose from
        /// </summary>
        public CategoricalParameter<GameObject> prefabs;

        //The container object that will be the parent of all placed objects from this Randomizer
        GameObject m_Container;
        //This cache allows objects to be reused across placements
        UnityEngine.Perception.Randomization.Utilities.GameObjectOneWayCache m_GameObjectOneWayCache;
        
        public LayerMask SimulationLayerMask;
        private List<GameObject> _iObjects = new() ;

        /// <inheritdoc/>
        protected override void OnAwake()
        {
            m_Container = new GameObject("Objects");
            m_Container.transform.parent = scenario.transform;
            m_GameObjectOneWayCache = new UnityEngine.Perception.Randomization.Utilities.GameObjectOneWayCache(
                m_Container.transform, prefabs.categories.Select(element => element.Item1).ToArray(), this);
        }

        /// <summary>
        /// Generates a foreground layer of objects at the start of each Scenario Iteration
        /// </summary>
        protected override void OnIterationStart()
        {
            var count = objectCount.Sample();
            for (int i = 0; i < count; i++)
            {
                var instance = m_GameObjectOneWayCache.GetOrInstantiate(prefabs.Sample());
                if (instance.TryGetComponent(out Collider col))
                {
                    for (int j = 0; j < 20; j++)
                    {   
                        var sampledPosition = positionDistribution.Sample();
                        var colliders = Physics.OverlapBox(sampledPosition, 
                            col.bounds.extents + Vector3.one * Gap.Sample(),Quaternion.identity,SimulationLayerMask);
                        if (colliders.Length != 0) continue;
                        var obj = Object.Instantiate(instance,sampledPosition, Quaternion.identity);
                        _iObjects.Add(obj);
                        break;
                    }
                }
                
            }
        }

        /// <summary>
        /// Hides all foreground objects after each Scenario Iteration is complete
        /// </summary>
        protected override void OnIterationEnd()
        {
            m_GameObjectOneWayCache.ResetAllObjects();
            _iObjects.ForEach(Object.Destroy);
            _iObjects.Clear();
            
        }
    }
}

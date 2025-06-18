using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Randomizers;
using UnityEngine.Perception.Randomization.Samplers;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace CustomRandomizations
{
    /// <summary>
    /// Creates a 2D layer of evenly spaced GameObjects from a given list of prefabs
    /// </summary>
    [Serializable]
    [AddRandomizerMenu("Vechicle Placement Randomizer")]
    public class VehiclePlacementRandomizer : Randomizer
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
        
        /// <summary>
        /// Sampler for random rotation
        /// </summary>
        public Vector3Parameter RotationDistribution;
        /// <summary>
        /// Gap b/w cars
        /// </summary>
        public FloatParameter Gap;
        /// <summary>
        /// No of placement tries per car
        /// </summary>
        public ConstantSampler Tries;
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
                for (int j = 0; j < Tries.Sample(); j++)
                {
                    var sampledPosition = positionDistribution.Sample();
                    var sampledRotation = RotationDistribution.Sample();
                    instance.transform.position = Vector3.right * 100000;
                    var vehicleTag = instance.GetComponent<VehicleRandomPlacementTag>();
                    bool isSuccess = vehicleTag.Place(sampledPosition, sampledRotation, Gap.Sample() * Vector3.one);
                    if(isSuccess)
                        break;
                }
            }
        }

        /// <summary>
        /// Hides all foreground objects after each Scenario Iteration is complete
        /// </summary>
        protected override void OnIterationEnd()
        {
            m_GameObjectOneWayCache.ResetAllObjects();
        }
    }
}

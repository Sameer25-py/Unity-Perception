using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Randomizers;
using UnityEngine.Perception.Randomization.Samplers;
using Object = UnityEngine.Object;

namespace CustomRandomizations
{   
    [Serializable]
    [AddRandomizerMenu("Vechicle Randomizer")]
    public class VehicleRandomizer : Randomizer
    {
        public List<GameObject> vehicles;
        public Vector3Parameter bounds;
        public IntegerParameter vehicleCount;
        public IntegerParameter tries;
        public LayerMask layerMask;


        private IntegerParameter _indexSampler;
        private List<GameObject> _instantiatedObjects = new();

        protected override void OnAwake()
        {
            _indexSampler = new()
            {
                value = new UniformSampler(0, vehicles.Count),
            };
        }
        protected override void OnIterationStart()
        {
            for (int i = 0; i < vehicleCount.Sample(); i++)
            {
                for (int j = 0; j < tries.Sample(); j++)
                {
                    var sampledSpawnPoint = bounds.Sample();
                    var selectedPrefab = vehicles[_indexSampler.Sample()];
                    var col = selectedPrefab.GetComponent<BoxCollider>();
                    if (col.name.Contains("Truck"))
                    {
                        Debug.Log(col.size);
                    }
                    var adjustedSize = new Vector3 (col.size.x * col.transform.localScale.x,col.size.y * col.transform.localScale.y,col.size.z * col.transform.localScale.z);
                    Debug.Log(adjustedSize);
                    var colliders = Physics.OverlapBox(sampledSpawnPoint, adjustedSize / 2, col.transform.rotation,layerMask);
                    if (colliders.Length == 0)
                    {
                        _instantiatedObjects.Add(Object.Instantiate(selectedPrefab, sampledSpawnPoint,col.transform.rotation));
                        break;
                    }
                }
            }
        }

        protected override void OnIterationEnd()
        {   
            _instantiatedObjects.ForEach(Object.Destroy);
            _instantiatedObjects.Clear();
        }
        
    }
}
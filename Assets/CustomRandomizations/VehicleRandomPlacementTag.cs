using UnityEngine;
using UnityEngine.Perception.Randomization.Randomizers;

namespace CustomRandomizations
{   
    public class VehicleRandomPlacementTag:RandomizerTag
    {
        public Vector2 minMaxRotationY;
        public float yPos;
        public LayerMask layerMask;
        public Bounds bounds;
        public Collider[] colliders;
        
        private Collider _collider;
        private Vector3 PositionTransformer(Vector3 pos)=> new Vector3(pos.x, yPos, pos.z);
        private Vector3 RotationTransformer(Vector3 pos)=> new Vector3(pos.x,pos.y * (minMaxRotationY.y - minMaxRotationY.x) + minMaxRotationY.x, pos.z);
        private void Awake()
        {
            _collider = GetComponent<Collider>();
        }

        private void CalculateBounds()
        {
            var renderers = GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
            }
        }

        public bool Place(Vector3 position, Vector3 rotation, Vector3 gap)
        {   
            position = PositionTransformer(position);
            rotation = RotationTransformer(rotation);
            var cachedPosition = transform.position;
            transform.rotation = Quaternion.Euler(rotation);
            transform.position = position;
            var extents = _collider.bounds.extents;
            _collider.enabled = false;
         
            colliders = Physics.OverlapBox(transform.position, extents + gap , transform.rotation, layerMask);
            if (colliders.Length == 0)
            {   
                transform.position = position;
                _collider.enabled = true;
                return true;
            }

            transform.position = cachedPosition;
            transform.rotation = Quaternion.identity;
            _collider.enabled = true;
            return false;
        }
    }
}
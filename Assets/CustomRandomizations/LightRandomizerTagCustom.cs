using System;
using UnityEngine;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Randomizers;
using UnityEngine.Perception.Randomization.Samplers;

namespace CustomRandomizations
{
    // Add this Component to any GameObject that you would like to be randomized. This class must have an identical name to
// the .cs file it is defined in.
    [RequireComponent(typeof(Light))]
    public class LightRandomizerTagCustom : RandomizerTag
    {
        public float MinIntensity,MaxIntensity;

        public void SetIntensity(float rawIntensity)
        {
            GetComponent<Light>().intensity =  rawIntensity * (MaxIntensity - MinIntensity) + MinIntensity;
        }
    }

    [Serializable]
    [AddRandomizerMenu("Light Randomizer")]
    public class LightRandomizer : Randomizer
    {
        // Sample FloatParameter that can generate random floats in the [0,360) range. The range can be modified using the
        // Inspector UI of the Randomizer.
        public FloatParameter LightIntensity;

        public ColorRgbParameter RGBColor;

        protected override void OnIterationStart()
        {
            var tags = tagManager.Query<LightRandomizerTagCustom>();
            foreach (var tag in tags)
                if(tag.TryGetComponent(out Light light))
                {
                    tag.SetIntensity(LightIntensity.Sample());
                    light.color = RGBColor.Sample();
                }
        }
    }
}
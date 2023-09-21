using System;
using System.Linq;

namespace NeoAnimLib.Nodes
{
    /// <summary>
    /// An animation node that mixes and blends between all child nodes.
    /// </summary>
    public class MixAnimNode : AnimNode
    {
        /// <summary>
        /// If true, then the weights of all direct children are normalized before sampling.
        /// Default value is false.
        /// </summary>
        public bool NormalizeWeights { get; set; }

        /// <inheritdoc/>
        public MixAnimNode() { }

        /// <inheritdoc/>
        public MixAnimNode(string name) : base(name) { }

        /// <summary>
        /// Changes the <see cref="AnimNode.LocalWeight"/> for each direct child node such that they all sum to
        /// 1 whilst keeping their relative proportions.
        /// </summary>
        public void NormalizeChildWeights()
        {
            float sum = DirectChildren.Sum(c => Math.Abs(c.LocalWeight));
            if (Math.Abs(sum - 1f) < 0.001f)
                return;

            if (sum == 0f)
                return;

            foreach (var child in DirectChildren)
            {
                child.LocalWeight /= sum;
            }
        }

        /// <summary>
        /// Samples this animation.
        /// All child nodes should also support sampling.
        /// The output will be a blend of all child outputs, and the method use to blend between samples is defined by
        /// <paramref name="input"/> as well as the individual child weights.
        /// </summary>
        public override AnimSample Sample(in SamplerInput input)
        {
            if (NormalizeWeights)
                NormalizeChildWeights();

            var output = AnimSample.Create(LocalTime);

            foreach (var child in DirectChildren)
            {
                using var childSample = child.Sample(input);

                var temp = output;
                output = AnimSample.Lerp(output, childSample, input.DefaultValueSource, child.LocalWeight, input.MissingPropertyBehaviour);
                temp.Dispose();
            }

            return output;
        }
    }
}

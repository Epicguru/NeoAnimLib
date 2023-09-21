﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace NeoAnimLib
{
    public class AnimSample : IDisposable
    {
        /// <summary>
        /// The number of active borrowed <see cref="AnimSample"/> objects by using the <see cref="Create(float)"/>
        /// method.
        /// </summary>
        public static int BorrowedCount { get; private set; }

        /// <summary>
        /// The number of pooled <see cref="AnimSample"/> objects.
        /// </summary>
        public static int PooledCount
        {
            get
            {
                lock (pool)
                {
                    return pool.Count;
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="AnimSample"/> that is a linear lerp between the two samples <paramref name="a"/> and <paramref name="b"/>
        /// using <paramref name="t"/> as the percentage.
        /// Unless <paramref name="missingPropertyBehaviour"/> is <see cref="MissingPropertyBehaviour.UseKnownValue"/> then <paramref name="defaultValueSource"/> must be provided,
        /// to provide a value when a property is missing from one sample or the other.
        /// Remember to always dispose of anim samples after they are no longer needed.
        /// </summary>
        public static AnimSample Lerp(AnimSample a, AnimSample b, DefaultValueSource defaultValueSource, float t, MissingPropertyBehaviour missingPropertyBehaviour = MissingPropertyBehaviour.UseDefaultValue)
        {
            Debug.Assert(a != b, "Do not lerp between the same anim sample!");

            if (defaultValueSource == null && missingPropertyBehaviour != MissingPropertyBehaviour.UseKnownValue)
                throw new Exception($"{nameof(defaultValueSource)} must be provided unless {nameof(missingPropertyBehaviour)} is set to {MissingPropertyBehaviour.UseKnownValue}");

            var output = Create(a.Time + (b.Time - a.Time) * t);

            var propPaths = a.samples.Keys.Concat(b.samples.Keys).Distinct();

            foreach (var path in propPaths)
            {
                AnimPropertySample? aSample = a.samples.TryGetValue(path, out var found) ? found : (AnimPropertySample?)null;
                AnimPropertySample? bSample = b.samples.TryGetValue(path, out found) ? found : (AnimPropertySample?)null;

                if (aSample != null && bSample != null)
                {
                    output.samples.Add(path, AnimPropertySample.Lerp(aSample.Value, bSample.Value, t));
                }
                else
                {
                    switch (missingPropertyBehaviour)
                    {
                        case MissingPropertyBehaviour.UseDefaultValue:
                            aSample ??= new AnimPropertySample(path, defaultValueSource(path));
                            bSample ??= new AnimPropertySample(path, defaultValueSource(path));
                            output.samples.Add(path, AnimPropertySample.Lerp(aSample.Value, bSample.Value, t));
                            break;

                        case MissingPropertyBehaviour.UseKnownValue:
                            // ReSharper disable once PossibleInvalidOperationException
                            output.samples.Add(path, aSample ?? bSample.Value);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(missingPropertyBehaviour), missingPropertyBehaviour, null);
                    }
                }
            }

            return output;
        }

        private static readonly Queue<AnimSample> pool = new Queue<AnimSample>(128);

        /// <summary>
        /// Gets a new <see cref="AnimSample"/> from the pool.
        /// </summary>
        public static AnimSample Create(float time)
        {
            lock (pool)
            {
                BorrowedCount++;
                if (!pool.TryDequeue(out var got))
                    return new AnimSample { Time = time };

                got.IsDisposed = false;
                got.Time = time;
                return got;
            }
        }

        public IReadOnlyCollection<AnimPropertySample> Samples => samples.Values;
        public bool IsDisposed { get; private set; }
        public float Time { get; private set; }

        private readonly Dictionary<string, AnimPropertySample> samples = new Dictionary<string, AnimPropertySample>(32);

        private AnimSample()
        {

        }

        ~AnimSample()
        {
            throw new Exception("Do not let AnimSamples be garbage collected. Dispose them to return them to the pool.");
        }

        public bool TryGetSample(string propName, out AnimPropertySample sample)
            => samples.TryGetValue(propName, out sample);

        public void SetSample(in AnimPropertySample sample)
        {
            samples[sample.Path] = sample;
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;
            samples.Clear();

            lock (pool)
            {
                pool.Enqueue(this);
                BorrowedCount--;
            }
        }

        public override string ToString()
        {
            var str = new StringBuilder(256);

            str.Append("Time: ").AppendLine(Time.ToString(CultureInfo.InvariantCulture));
            str.AppendLine("Properties:");
            foreach (var prop in samples)
            {
                str.Append(prop.Key).Append(": ").AppendLine(prop.Value.Value.ToString(CultureInfo.InvariantCulture));
            }
            return str.ToString();
        }
    }
}

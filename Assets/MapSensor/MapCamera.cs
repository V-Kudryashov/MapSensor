using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VK.MapSensor
{
    public abstract class MapCamera : MonoBehaviour
    {
        public abstract void Init();
        /// <summary>
        /// The size of the observations [Height, Width, Channels]
        /// For example, a sensor that observes the heightmap would use [Height, Width, 1].
        /// The number of channels is not limited.
        /// </summary>
        /// <returns></returns>
        public abstract int[] GetShape();
        /// <summary>
        /// Visual observation input.
        /// </summary>
        /// <returns>Array corresponding to the shape [x, y, ch]</returns>
        public abstract float[,,] UpdateFrame();
        /// <summary>
        /// Use this property to render a frame
        /// </summary>
        public abstract float[,,] Frame { get; }
        public abstract int Width { get; }
        public abstract int Height { get; }
    }
}
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
        public abstract float[,,] GetFrame();
        /// <summary>
        /// The texture must be updated before rendering the current frame.
        /// </summary>
        public abstract void UpdateTexture();
        /// <summary>
        /// The texture is used for visualisation only.
        /// </summary>
        /// <returns></returns>
        public abstract Texture2D GetTexture();
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Profiling;

namespace VK.MapSensor
{
    public class MapSensor : ISensor
    {
        MapCamera m_Map;
        string m_Name;
        private ObservationSpec m_ObservationSpec;
        int[] m_Shape;

        public MapSensor(MapCamera map, string name)
        {
            m_Map = map;
            m_Name = name;
            map.Init();
            m_Shape = map.GetShape();

            int height = m_Shape[0];
            int width = m_Shape[1];
            int channels = m_Shape[2];
            m_ObservationSpec = ObservationSpec.Visual(height, width, channels, ObservationType.Default);

        }
        public byte[] GetCompressedObservation()
        {
            return new byte[0];
        }

        public CompressionSpec GetCompressionSpec()
        {
            return new CompressionSpec(SensorCompressionType.None);
        }

        /// <inheritdoc/>
        public SensorCompressionType GetCompressionType()
        {
            return SensorCompressionType.None;
        }

        /// <inheritdoc/>
        public string GetName()
        {
            return m_Name;
        }

        /// <inheritdoc/>
        public int[] GetObservationShape()
        {
            return m_Shape;
        }

        public ObservationSpec GetObservationSpec()
        {
            return m_ObservationSpec;
        }

        /// <inheritdoc/>
        public void Reset() { }

        /// <inheritdoc/>
        public void Update() { }
        public int Write(ObservationWriter writer)
        {
            //System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            //stopWatch.Start();
            Profiler.BeginSample("UpdateFrame()");
            float[,,] map = m_Map.UpdateFrame(); // 2.76 ms
            Profiler.EndSample();
            int height = m_Shape[0];
            int width = m_Shape[1];
            int channels = m_Shape[2];
            for (int w = 0; w < width; w++)
                for (int h = 0; h < height; h++)
                    for (int ch = 0; ch < channels; ch++)
                        writer[h, w, ch] = map[h, w, ch];
            //stopWatch.Stop();
            //Double ms = stopWatch.Elapsed.TotalMilliseconds;

            return height * width * channels;
        }
    }
}
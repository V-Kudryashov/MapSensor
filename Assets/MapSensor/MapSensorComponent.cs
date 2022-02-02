using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Serialization;

namespace VK.MapSensor
{
    /// <summary>
    /// Component that wraps a <see cref="MapSensor"/>.
    /// </summary>
    [AddComponentMenu("ML Agents/Map Sensor")]
    public class MapSensorComponent : SensorComponent
    {
        MapSensor m_Sensor;
        [HideInInspector, SerializeField, FormerlySerializedAs("map")]
        MapCamera m_Map;
        public MapCamera Map
        {
            get { return m_Map; }
            set { m_Map = value; }
        }
        [HideInInspector, SerializeField, FormerlySerializedAs("sensorName")]
        string m_SensorName = "MapSensor";
        public string SensorName
        {
            get { return m_SensorName; }
            set { m_SensorName = value; }
        }
        [HideInInspector, SerializeField]
        [Range(1, 50)]
        [Tooltip("Number of frames that will be stacked before being fed to the neural network.")]
        int m_ObservationStacks = 1;

        /// <summary>
        /// Compression type for the render texture observation.
        /// </summary>
        public SensorCompressionType CompressionType
        {
            get { return SensorCompressionType.None; }
        }
        /// <summary>
        /// Whether to stack previous observations. Using 1 means no previous observations.
        /// Note that changing this after the sensor is created has no effect.
        /// </summary>
        public int ObservationStacks
        {
            get { return m_ObservationStacks; }
            set { m_ObservationStacks = value; }
        }

        /// <summary>
        /// Creates the <see cref="MapSensor"/>
        /// </summary>
        /// <returns>The created <see cref="MapSensor"/> object for this component.</returns>
        public override ISensor[] CreateSensors()
        {
            m_Sensor = new MapSensor(Map, SensorName);
            if (ObservationStacks != 1)
            {
                return new ISensor[] { new StackingSensor(m_Sensor, ObservationStacks) };
            }
            return new ISensor[] { m_Sensor };
        }
    }
}
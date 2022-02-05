# MapSensor

**Table of Contents**
- [Description](#description)
- [Files](#files)
- [Example](#example)
- [Dependencies](#dependencies)

## Description
The Map Sensor is designed to use non-visual data as visual observations.

For example, we need the agent to be able to see the heightmap. To solve this problem, you can draw a heightmap on the plane and then get observations using the Camera Sensor. At first I did just that. It works. To solve this problem, the Map Sensor has some advantages.
* Map Sensor faster than Camera Sensor. No camera rendering, no image to array conversion.
* Map Sensor has an unlimited number of channels. Camera Sensor has 1 (greyscale) or 3 (RGB) channels.
* Map Sensor does not use graphics. We can use the `no_graphics` engine setting.
## Files
Map Sensor API contains 4 files:
- `MapSensor.cs` - contains `MapSensor` class.
- `MapSensorComponent.cs` - script to be attached to the Agent object.
- `MapSensorComponentEditor.cs` - editor script.
- `MapCamera.cs` - abstract class that you should implement. Provided example contains simple implementation of this class: `HeightmapCamera.cs`.
## Example
In this example, the car is moving towards the target and avoiding the peaks of the terrain. [Video](https://youtu.be/lVXY7S-cbHY)
## Dependencies

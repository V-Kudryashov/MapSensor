# MapSensor

**Table of Contents**
- [Description](#description)
- [Files](#files)
- [Example](#example)
    - [1 channel camera](#1-channel-camera)
    - [4 channels camera](#4-channels-camera)
- [Dependencies](#dependencies)

## Description
The Map Sensor is designed to use non-visual data as visual observations.

For example, we need the agent to be able to see the heightmap. To solve this problem, you can draw a heightmap on the plane and then get observations using the Camera Sensor. At first I did just that. It works. Then I developed a special Map Sensor similar to the Camera Sensor. Map Sensor has some advantages over Camera Sensor.
* Map Sensor faster than Camera Sensor. No camera rendering, no image to array conversion.
* Map Sensor has an unlimited number of channels. Camera Sensor has 1 (greyscale) or 3 (RGB) channels.
* Map Sensor does not use graphics. We can use the `no_graphics` engine setting.
## Files
Map Sensor API contains 4 files:
- `MapSensor.cs` - contains `MapSensor` class. The `MapSensor` receives one frame from the `MapCamera` and writes it directly to the `ObservationWriter`.
- `MapSensorComponent.cs` - script to be attached to the Agent object.
- `MapSensorComponentEditor.cs` - editor script.
- `MapCamera.cs` - abstract class that you should implement. The main task of this class is to cut a rectangular frame from the map array. Provided examples contains 2 implementations of this class: Simple 1 channel `HeightmapCamera.cs` and more complex 4 channel `TerrainCamera.cs`.
## Examples
### 1 channel camera
In this example, the role of the map is performed by an array of heights obtained from the Terrain. `float[,] H = terrain.terrainData.GetHeights(0, 0, res, res);`
The car is moving towards the target and avoiding the peaks of the terrain. [Video](https://youtu.be/lVXY7S-cbHY)
### 4 channels camera
It is more complex example. The `TerrainMap` script allows you to get a map that includes up to eight channels:
- `Height`
- `NormalX`
- `NormalY`
- `NormalMagnitude`
- `CurvatureX`
- `CurvatureY`
- `CurvatureMagnitude`
- `Objects`

Most of the data comes from `TerrainData`: `terrainData.GetInterpolatedHeights`, `terrainData.GetInterpolatedNormal(X, Y);`. 
`Objects` are moving objects, such as other Agents.

`TerrainCamera` uses 4 channels:
- `NormalX`
- `NormalY`
- `CurvatureMagnitude`
- `Objects`
 
In this example 3 cars  is moving towards the target and avoiding the peaks and other cars.
## Dependencies
- Unity 2020.3.26f1
- ML Agents 2.1.0-exp.1

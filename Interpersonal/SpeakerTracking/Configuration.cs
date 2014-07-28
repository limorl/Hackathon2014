using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeakerTracking
{
    class Configuration
    {
        public readonly AudioConfiguration AudioConfig;
        public readonly CameraConfiguration CameraConfig;


        /// <summary>
        /// Returns default configuration
        /// </summary>
        public Configuration()
        {
            AudioConfig = new AudioConfiguration();
            CameraConfig = new CameraConfiguration();
        }
    }

    class AudioConfiguration
    {
        /// <summary>
        /// Number of milliseconds between each read of audio data from the stream.
        /// Faster polling (few tens of ms) ensures a smoother audio stream visualization.
        /// </summary>
        public const int AudioPollingInterval = 50;

        /// <summary>
        /// Number of samples captured from Kinect audio stream each millisecond.
        /// </summary>
        public const int SamplesPerMillisecond = 16;

        /// <summary>
        /// Number of bytes in each Kinect audio stream sample.
        /// </summary>
        public const int BytesPerSample = 2;

        /// <summary>
        /// Number of audio samples represented by each column of pixels in wave bitmap.
        /// </summary>
        public const int SamplesPerColumn = 40;

        /// <summary>
        /// Width of bitmap that stores audio stream energy data ready for visualization.
        /// </summary>
        public const int EnergyBitmapWidth = 780;

        /// <summary>
        /// Height of bitmap that stores audio stream energy data ready for visualization.
        /// </summary>
        public const int EnergyBitmapHeight = 195;
    }

    class CameraConfiguration
    {
    }
}

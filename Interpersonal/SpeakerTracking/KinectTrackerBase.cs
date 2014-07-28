using Microsoft.Kinect;
using Microsoft.Samples.Kinect.WpfViewers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeakerTracking
{
    abstract class KinectTrackerBase
    {
        private KinectSensorManager kinectSensorManager;
        public KinectSensorManager KinectSensorManager
        {
            get
            {
                return kinectSensorManager;
            }
            set
            {
                KinectSensorManagerChanged(this.kinectSensorManager, value);
                this.kinectSensorManager = value;
            }
        }
                  
        /// <summary>
        /// Virtual method which can be used to react to changes of the underlying KinectSensor.
        /// </summary>
        /// <param name="sender">The current KinectSensorManager</param>
        /// <param name="args">The args, which contain the old and new values</param>
        protected virtual void OnKinectSensorChanged(object sender, KinectSensorManagerEventArgs<KinectSensor> args)
        {
        }

        /// <summary>
        /// Virtual method which can be used to react to status changes of the underlying KinectSensor.
        /// </summary>
        /// <param name="sender">The current KinectSensorManager</param>
        /// <param name="args">The args, which contain the old and new values</param>
        protected virtual void OnKinectStatusChanged(object sender, KinectSensorManagerEventArgs<KinectStatus> args)
        {
        }

        /// <summary>
        /// Virtual method which can be used to react to running state changes of the underlying KinectSensor.
        /// </summary>
        /// <param name="sender">The current KinectSensorManager</param>
        /// <param name="args">The args, which contain the old and new values</param>
        protected virtual void OnKinectRunningStateChanged(object sender, KinectSensorManagerEventArgs<bool> args)
        {
        }

        /// <summary>
        /// Virtual method which can be used to react to app conflict state changes of the underlying KinectSensor.
        /// </summary>
        /// <param name="sender">The current KinectSensorManager</param>
        /// <param name="args">The args, which contain the old and new values</param>
        protected virtual void OnKinectAppConflictChanged(object sender, KinectSensorManagerEventArgs<bool> args)
        {
        }

        /// <summary>
        /// Virtual method which can be used to react to changes of the SkeletonEngine which would 
        /// necessitate reseting AudioStream-dependent state.
        /// Workaround for Microsoft.Kinect.dll 1.X bug where enabling/diabling the SkeletonEngine resets audio.
        /// </summary>
        /// <param name="sender">The current KinectSensorManager</param>
        /// <param name="args">The args, which contain no useful information</param>
        protected virtual void OnAudioWasResetBySkeletonEngine(object sender, EventArgs args)
        {
        }

        /// <summary>
        /// This callback helps us call the virtual On*Changed events.  These can occur because of changes
        /// in the KinectSensor - for which we register notifiers - or because the KinectSensorManager itself
        /// has changed.
        /// </summary>
        /// <param name="sender">The DependencyObject on which the property changed.</param>
        /// <param name="args">The args describing the change.</param>
        private static void KinectSensorManagerChanged(KinectSensorManager oldValue, KinectSensorManager newValue)
        {
            KinectSensor oldSensor = null;
            KinectSensor newSensor = null;

            if (null != oldValue)
            {
                oldSensor = oldValue.KinectSensor;
            }

            if (null != newValue)
            {
                newSensor = newValue.KinectSensor;
            }

            var oldStatus = KinectStatus.Undefined;
            var newStatus = KinectStatus.Undefined;

            bool oldRunningState = false;
            bool newRunningState = false;

            bool oldAppConflict = false;
            bool newAppConflict = false;

            if (null != oldSensor)
            {
                oldStatus = oldSensor.Status;
                oldRunningState = oldSensor.IsRunning;
                oldAppConflict = oldValue.KinectSensorAppConflict;
            }

            if (null != newSensor)
            {
                newStatus = newSensor.Status;
                newRunningState = newSensor.IsRunning;
                newAppConflict = newValue.KinectSensorAppConflict;
            }
        }
    }
}

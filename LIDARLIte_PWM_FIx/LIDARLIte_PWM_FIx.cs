using System;
using System.Threading;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace WoodRobotics.NetDuino.LIDAR
{
    
    /// <summary>
    /// LIDAR-Lite controller
    /// </summary>
    public class LIDARLite
    {

        #region Constructor
        /// <summary>
        /// One and only constructor
        /// </summary>
        /// <param name="pwm">LIDAR-Lite PWM pin</param>
        /// <param name="enable">LIDAR-Lite enable pin</param>
        public LIDARLite(Cpu.Pin pwm, Cpu.Pin enable)
        {
            m_PWM = pwm;
            m_Enable = enable;
        }
        #endregion

        #region Fields
        private Cpu.Pin m_PWM;
        private Cpu.Pin m_Enable;

        private InterruptPort m_LIDAR_PWM;
        private OutputPort m_LIDAR_PWN_EN;
        private ManualResetEvent m_LIDAR_Scanned = new ManualResetEvent(false);

        private double m_LIDAR_Distance = 0;
        private double m_LIDAR_Start_Time = 0;
        #endregion

        #region Properties
        #endregion

        #region Events
        public delegate void dOnMesurement(object state, double mesurement);
        public event dOnMesurement OnMesurement;

        private void LIDAR_PWM_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            
            /// Falling edge
            /// This marks the end of our Mesurement
            if (data2 == 0)
            {
                
                /// get the width of the HIGH-LOW pulse
                var l_PulseWidth = time.Ticks - m_LIDAR_Start_Time;

                /// using the width of the pulse
                /// We can determin the centemeters
                /// then convert that to feet
                var l_MesuredFeet = ((l_PulseWidth / 100.0) / 2.54) / 12;

                /// Hold our distance mesurement
                m_LIDAR_Distance = l_MesuredFeet;
                
                /// Notify the waiting thread that we successfuly got a mesurement
                m_LIDAR_Scanned.Set();
                
            }
            else
                /// Risinig edge
                /// We start mesuring starting here
                /// Hold the current DateTime ticks
                m_LIDAR_Start_Time = time.Ticks;
            
        }
        #endregion

        #region Methods
        /// <summary>
        /// Call this to start monitoring ranges
        /// </summary>
        public void Start()
        {

            /// Initalize our Enable port and our PWM port
            m_LIDAR_PWM = new InterruptPort(m_PWM, true, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeBoth);
            m_LIDAR_PWN_EN = new OutputPort(m_Enable, false);

            /// Register the Interupt event to see rising and falling edges
            m_LIDAR_PWM.OnInterrupt += LIDAR_PWM_OnInterrupt;

            /// create and start our monitoring thread
            var l_MesureThread = new Thread(new ThreadStart(MesureDistanceThread));
            l_MesureThread.Start();

        }

        /// <summary>
        /// Thread to constantly scan
        /// </summary>
        private void MesureDistanceThread()
        {

            /// Local var to keep track of a successful mesurement
            var l_Success = false;

            while (true)
            {

                /// Reset our wait event
                m_LIDAR_Scanned.Reset();

                /// Turn the LIDAR-Lite sensor on
                m_LIDAR_PWN_EN.Write(true);

                /// Clear our start time
                m_LIDAR_Start_Time = 0;

                /// wait for the LIDAR to wake up
                Thread.Sleep(5);

                /// Wait for a successful mesurement
                /// or if it takes to long, ignore it
                l_Success = WaitHandle.WaitAll(new WaitHandle[] { m_LIDAR_Scanned }, 40, true);

                /// Shutdown our LIDAR
                m_LIDAR_PWN_EN.Write(false);

                Thread.Sleep(5);

                /// If we had a successful mesurement
                /// send it to the monitoring class
                if (l_Success && OnMesurement != null)
                    OnMesurement(this, m_LIDAR_Distance);

            }

        }
        #endregion

    }
    
}

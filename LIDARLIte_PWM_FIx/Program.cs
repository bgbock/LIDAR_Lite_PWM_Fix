using System;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace WoodRobotics.Netduino.LIDAR
{
    
    public class Program
    {
        
        #region Main
        public static void Main()
        {
            
            /// Setup our serial port using the second avaliable com port
            m_Serial = new SerialPort(SerialPorts.COM2, 19200, Parity.None, 8, StopBits.One);
            
            /// Create our LIDAr-Lite object, register its event, and start monitoring
            m_LIDAR = new LIDAR_Lite(Pins.GPIO_PIN_D0, Pins.GPIO_PIN_D1);
            m_LIDAR.OnMesurement += LIDAR_OnMesurement;
            m_LIDAR.Start();
            
            /// Sleep for ever to prevent the code from exiting
            /// and GC trashing our objects
            Thread.Sleep(Timeout.Infinite);
            
        }
        #endregion
        
        #region Fields
        private static LIDAR_Lite m_LIDAR;
        private static SerialPort m_Serial;
        #endregion
        
        #region Events
        /// <summary>
        /// Event raised when we get a new mesurement from the LIDAR-Lite unit
        /// </summary>
        /// <param name="state">LIDAR_Lite object</param>
        /// <param name="mesurement">The mesurement in centimeters</param>
        static void LIDAR_OnMesurement(object state, double mesurement)
        {
            
            var l_Mesurement = System.Text.UTF8Encoding.UTF8.GetBytes(mesurement.ToString());
            m_Serial.Write(l_Mesurement, 0, l_Mesurement.Length);
            
        }
        #endregion
        
    }
    
}

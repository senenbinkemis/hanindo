/* RFID module
 * 
 * Created by:
 *      Yosef Didik
 * Date:
 *      April 3rd, 2014
 */

using System;
using System.IO.Ports;
using System.Windows.Forms;

namespace RFID
{
    /*! \brief Class of RFID MF700
     * 
     * Example usage on C#.NET:
     @code
      namespace ConsoleApplication1
        {
            class Program
            {
                static void Main(string[] args)
                {
                    RFID.MF700 rfid = new RFID.MF700();

                    rfid.tagDetection += rfidEvent;
                    if (rfid.Open("COM1") == 0)
                    {
                        Console.WriteLine("Press any key to continue...");
                        Console.WriteLine();
                        Console.ReadKey();
                        rfid.Close();
                    }
                }

                static void rfidEvent(object sender, RFID.RFIDEventArgs e)
                {
                    Console.WriteLine("Card ID: " + e.CardID);
                }
            }
        }
     @endcode
     *
     * 
     * Example usage on VB.NET:
     @code
       Module Module1

            Sub Main()
                Dim rfid As New RFID.MF700

                AddHandler rfid.tagDetection, AddressOf rfidEvent

                If rfid.Open("COM1", 9600) = 0 Then
                    Console.WriteLine("Press any key to continue...")
                    Console.WriteLine()
                    Console.ReadKey()
                    rfid.Close()
                End If

            End Sub

            Private Sub rfidEvent(ByVal sender As Object, ByVal e As RFID.RFIDEventArgs)
                Console.WriteLine("Card ID: " + e.cardID)
            End Sub

        End Module
     @endcode
     */
    public class MF700
    {
        // Object of serial port descriptor
        static SerialPort rfidPort;
        // Public event of RFID tag detection
        public event EventHandler<RFIDEventArgs> tagDetection;

        /*! \brief Open RFID port and start listening.
         * 
         * @param defaultPortName   String of port Name i.e "COM1"
         * @return 0 if success. Otherwise failed.
         */
        public int Open(string defaultPortName)
        {
            // Sanity
            if (defaultPortName == null)
                return -1;

            // Create new serial Port object
            rfidPort = new SerialPort();

            // Assignment parameter
            rfidPort.PortName = defaultPortName;
            rfidPort.BaudRate = 9600;
            rfidPort.DataBits = 8;
            rfidPort.Parity = Parity.None;
            rfidPort.StopBits = StopBits.One;
            rfidPort.Handshake = Handshake.None;
            rfidPort.DtrEnable = true;
            rfidPort.RtsEnable = true;

            // Received event
            rfidPort.DataReceived += new SerialDataReceivedEventHandler(rfidPort_DataReceivedHandler);

            // Open port
            if (rfidPort.IsOpen == true)
                return -2;

            try
            {
                rfidPort.Open();
            }
            catch (Exception)
            {
                MessageBox.Show("Cannot Open " + defaultPortName + "!");
                return -3;
            }
            
            return 0;
        }


        /*! \brief Overloads open RFID port and start listening.
         * 
         * @param defaultPortName   String of port Name i.e "COM1"
         * @param defaultBaudrate   Baudrate of serial
         * @return 0 if success. Otherwise failed.
         */
        public int Open(string defaultPortName, int defaultBaudrate)
        {
            if (defaultPortName == null)
                return -1;

            if (defaultBaudrate <= 0)
                return -1;

            // Create new serial Port object
            rfidPort = new SerialPort();

            // Assignment parameter
            rfidPort.PortName = defaultPortName;
            rfidPort.BaudRate = defaultBaudrate;
            rfidPort.DataBits = 8;
            rfidPort.Parity = Parity.None;
            rfidPort.StopBits = StopBits.One;
            rfidPort.Handshake = Handshake.None;
            rfidPort.DtrEnable = true;
            rfidPort.RtsEnable = true;

            // Received event
            rfidPort.DataReceived += new SerialDataReceivedEventHandler(rfidPort_DataReceivedHandler);

            // Open port
            if (rfidPort.IsOpen == true)
                return -2;

            try
            {
                rfidPort.Open();
            }
            catch (Exception)
            {
                MessageBox.Show("Cannot Open " + defaultPortName + "!");
                return -3;
            }

            return 0;
        }


        /*! \brief Close RFID port
         */
        public void Close()
        {
            rfidPort.Close();
        }


        /* Received handler.
         * This function is private. It is handle of received data from serial port and trigger event to higher level
         * indicating tag detection.
         */
        private void rfidPort_DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            string cardData = null;
            SerialPort sp = (SerialPort)sender;
            int ByteToRead, ByteOffet;
            bool IsFrameComplete = false;
            int i, j, k;

            ByteOffet = i = k= 0;
            if (sp.IsOpen == true)
                ByteToRead = sp.BytesToRead;
            else
                ByteToRead = 0;
            
            if ((ByteToRead < 40) && (ByteToRead > 0))
            {
                char[] inputData = new char[40];

                // Read all packet
                while (!IsFrameComplete)
                {
                    if (i < 40)
                    {
                        // Wait until end of frame received
                        if (inputData[i] != 0x03)
                        {
                            // Read data and store
                            if (sp.IsOpen == true)
                            {
                                if (ByteToRead > 0)
                                {
                                    sp.Read(inputData, ByteOffet, ByteToRead);
                                    k += ByteOffet;
                                    ByteOffet = k + ByteToRead;
                                }     
                            }
                            else
                            {
                                // Exit with error
                                IsFrameComplete = true;
                                ByteOffet = 100;
                            }

                            i = ByteOffet - 1;
                            if (sp.IsOpen == true)
                            {
                                ByteToRead = sp.BytesToRead; 
                                j = 40 - i;
                                if ((ByteToRead > 40) || (ByteToRead > j))
                                {
                                    // Exit with error
                                    IsFrameComplete = true;
                                    i = 100;
                                }
                            }    
                            else
                            {
                                // Exit with error
                                IsFrameComplete = true;
                                i = 100;
                            }    
                        }
                        else
                        {
                            IsFrameComplete = true;
                        }
                    }
                    else
                    {
                        IsFrameComplete = true;
                    }
                }

                if (i < 40)
                {
                    if (ByteOffet > 0)
                    {
                        // Parse data
                        i = 0;
                        while ((inputData[i] != 0x02) && (i < ByteOffet))
                            i++;

                        if (i < ByteOffet)
                        {
                            ByteOffet = ByteOffet - i;
                            i++;
                            j = i;
                            while (((inputData[j] != 0x0D) || (inputData[j + 1] != 0x0A)) && (j < ByteOffet))
                                j++;

                            if (j < ByteOffet)
                            {
                                for (k = 0; i < j; k++, i++)
                                {
                                    inputData[k] = inputData[i];
                                }

                                for (; k < 40; k++)
                                {
                                    inputData[k] = '\0';
                                }

                                cardData = new string(inputData);
                            }
                        }
                    }
                }
            }

            if (cardData != null)
            {
                tagDetectionEvent(cardData);
            }
        }


        protected virtual void tagDetectionEvent(string cardID)
        {
            RFIDEventArgs arg = new RFIDEventArgs(cardID);
            EventHandler<RFIDEventArgs> handler = tagDetection;
            if (handler != null)
            {
                handler(this, arg);
            }
        }
    }
}

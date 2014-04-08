using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RFID
{
    /*! \brief Class of RFID event argument.
     */
    public class RFIDEventArgs : EventArgs
    {
        private string data;

        /*! \brief Constructor of class
         * @param inputData String of RFID data
         */
        public RFIDEventArgs(string inputData)
        {
            data = inputData;
        }

        /*! \brief Return the RFID data to higher level event caller
         */
        public string CardID
        {
            get { return data; }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BitPackingExamples
{
    class BitStreamWriter
    {

        private int bitsInBuffer = 0;
        private byte[] buffer = new byte[1];

        public BitStreamWriter()
        {
            // clear the buffer
            buffer[0] = 0x00;
        }

        public int WriteBits(uint inputBits, int numBits, Stream outputStream)
        {
            if (numBits > 32) numBits = 32;
            int bitsWritten = 0;

            for (int i = 0; i < numBits; i++)
            {
                uint bit = (inputBits >> i) & 0x00000001;
                WriteToBuffer(bit, outputStream);
                bitsWritten++;
            }
            
            return bitsWritten;
        }

        public void WriteFlush(Stream outputStream)
        {
            if (bitsInBuffer != 0) {
                outputStream.Write(buffer, 0, 1);
            }
            buffer[0] = 0;
            bitsInBuffer = 0;
        }

        private void WriteToBuffer(uint bit, Stream outputStream)
        {
            if (bitsInBuffer == 8) WriteFlush(outputStream);
            if (bit != 0) // only write 1 bits. buffer is initialized to 0
                buffer[0] |= (byte)(bit << bitsInBuffer);
            bitsInBuffer++;
        }
    }

    class BitStreamReader
    {
        private int bitsInBuffer = 0;
        private byte[] buffer = new byte[1];

        public uint ReadBits(int numBits, Stream inputStream)
        {
            uint bits = 0;
            for (int i = 0; i<numBits;++i)
            {
                bits |= (ReadBit(inputStream) << i);

                // we have used up all the data!
                if (bitsInBuffer == 0 && !inputStream.CanRead) break;
            }
            return bits;
        }

        private int FillBuffer(Stream inputStream)
        {
            if (inputStream.Read(buffer, 0, 1) == 1)
            {
                bitsInBuffer = 8;
                return 0;
            }
            return -1;
        }

        private uint ReadBit(Stream inputStream)
        {
            if (bitsInBuffer == 0) {
                FillBuffer(inputStream);
            }
            if (bitsInBuffer > 0)
            {
                uint value = (uint)(buffer[0] >> (8 -bitsInBuffer)) & 1;
                bitsInBuffer--;
                return value;
            }
            return 0;
        }
    }
}

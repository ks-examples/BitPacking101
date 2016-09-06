using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BitPackingExamples
{
    class Program
    {
        // function that finds the topmost bit
        static uint CountBits(uint value)
        {
            int bitOffset = 31;
            while ((value & (1 << bitOffset)) == 0)
            {
                bitOffset--;
            }
            return (uint)bitOffset+1;
        }


        // here are the values we want to compress
        static uint[] values =
        {
            0x0000FDEE,
            0x000000A1,
            0x0000EF2F,
            0x02A3120E,
            0x00001147,
            0x000000F1,
            0x00000907,
            0x000013F7,
            0x000003A3,
            0x00000407,
            0x00000067,
            0x0000F25A,
            0x0041FB7E,
            0x075B1C78,
            0x0CCC20D6
        };


        // simple 1-bit header for a single division
        static void Example1_SimplePack()
        {

            const int MAX_BITS = 28;
            const int MIN_BITS = 16;
            MemoryStream packedData = new MemoryStream();
            BitStreamWriter bitWriter = new BitStreamWriter();

            // bit packing the values: 
            // bit header 1, then value bits = 16, bit header 0, value bits = 32
            foreach (uint v in values)
            {
                uint size = CountBits(v);

                // if there are 16 bits or fewer, write only 16+1 bits
                if (size <= MIN_BITS)
                {
                    bitWriter.WriteBits(1, 1, packedData);
                    bitWriter.WriteBits(v, MIN_BITS, packedData);
                }
                else
                {
                    bitWriter.WriteBits(0, 1, packedData);
                    bitWriter.WriteBits(v, MAX_BITS, packedData);
                }
            }
            bitWriter.WriteFlush(packedData);

            Console.WriteLine("Original size = {0}, packed size = {1}", values.Length * sizeof(uint), packedData.Length);

            // now read the data back
            //
            // rewind our stream so we can read it back
            packedData.Seek(0, SeekOrigin.Begin);
            BitStreamReader bitReader = new BitStreamReader();
            uint[] output = new uint[values.Length];

            for (int i = 0; i < output.Length; ++i)
            {
                uint header = bitReader.ReadBits(1, packedData);
                switch (header)
                {
                    case 1:
                        output[i] = bitReader.ReadBits(MIN_BITS, packedData);
                        break;
                    case 0:
                        output[i] = bitReader.ReadBits(MAX_BITS, packedData);
                        break;
                }
                if (output[i] == values[i]) Console.WriteLine("Position {0} matches!", i);
            }
        }

        // more complex 1 or 2 bit header (possible values are 1b, 01b and 00b) smallest header used for most common
        // length range where bits <= 16
        static void Example2_2Divisions()
        {
            MemoryStream packedData = new MemoryStream();
            BitStreamWriter bitWriter = new BitStreamWriter();

            const int MAX_BITS = 32;
            const int MED_BITS = 16;
            const int MIN_BITS = 13;

            // let's pack them into a stream!
            foreach (uint v in values)
            {
                uint size = CountBits(v);

                // if there are 16 bits or fewer, write only 16+1 bits
                if (size <= MED_BITS)
                {
                    if (size <= MIN_BITS)
                    {
                        bitWriter.WriteBits(1, 1, packedData);
                        bitWriter.WriteBits(v, MIN_BITS, packedData);
                    }
                    else
                    {
                        bitWriter.WriteBits(2, 2, packedData);
                        bitWriter.WriteBits(v, MED_BITS, packedData);
                    }
    
                }
                else
                {
                    bitWriter.WriteBits(0, 2, packedData);
                    bitWriter.WriteBits(v, MAX_BITS, packedData);
                }
            }
            bitWriter.WriteFlush(packedData);

            Console.WriteLine("Original size = {0}, packed size = {1}", values.Length * sizeof(uint), packedData.Length);

            // now read the data back
            //
            // rewind our stream so we can read it back
            packedData.Seek(0, SeekOrigin.Begin);
            BitStreamReader bitReader = new BitStreamReader();
            uint[] output = new uint[values.Length];

            for (int i = 0; i < output.Length; ++i)
            {
                uint header = bitReader.ReadBits(1, packedData);
                switch (header)
                {
                    case 1:
                        output[i] = bitReader.ReadBits(MIN_BITS, packedData);
                        break;
                        // if the bit is 0, there is a second bit with length subsections
                    case 0:
                        uint secondBit = bitReader.ReadBits(1, packedData);
                        if (secondBit == 0)
                            output[i] = bitReader.ReadBits(MAX_BITS, packedData);
                        else
                            output[i] = bitReader.ReadBits(MED_BITS, packedData);
                        break;
                }
                if (output[i] == values[i]) Console.WriteLine("Position {0} matches!", i);
            }
        }

        // We could just explicitly write the total number of bits prior to each value, so each header is 5 bits,
        // but then the value is perfectly packed every time
        static void Example3_ExplicitHeaders()
        {
            MemoryStream packedData = new MemoryStream();
            BitStreamWriter bitWriter = new BitStreamWriter();

            // packing
            foreach (uint v in values)
            {
                uint header = CountBits(v);
                bitWriter.WriteBits(header, 5, packedData);
                bitWriter.WriteBits(v, (int)header, packedData);
            }
            bitWriter.WriteFlush(packedData);

            Console.WriteLine("Original size = {0}, packed size = {1}", values.Length * sizeof(uint), packedData.Length);

            // now read the data back
            //
            // rewind our stream so we can read it back
            packedData.Seek(0, SeekOrigin.Begin);
            BitStreamReader bitReader = new BitStreamReader();
            uint[] output = new uint[values.Length];

            for (int i = 0; i < output.Length; ++i)
            {
                uint header = bitReader.ReadBits(5, packedData);
                output[i] = bitReader.ReadBits((int)header, packedData);
                if (output[i] == values[i]) Console.WriteLine("Position {0} matches!", i);
            }
        }

        static void Main(string[] args)
        {
            Example1_SimplePack();
            Example2_2Divisions();
            Example3_ExplicitHeaders();
        }
    }
}

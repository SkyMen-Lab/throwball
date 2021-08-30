using ThrowBall.Models;

namespace ThrowBall.TCP
{
    public static class Coder
    {
        
        //TODO: change byte usage
        // 1 byte for event type, 3 bytes for message size
        public static int BigEndianSizeShift(byte[] array)
        {
            return array[0] << 24 |
                   array[1] << 16 |
                   array[2] << 8 |
                   array[3];
        }

        public static void BigEndianSizeToBytesShift(int value, byte[] array)
        {
            array[0] = (byte)(value >> 24);
            array[1] = (byte)(value >> 16);
            array[2] = (byte)(value >> 8);
            array[3] = (byte)(value);
        }
        
    }
}
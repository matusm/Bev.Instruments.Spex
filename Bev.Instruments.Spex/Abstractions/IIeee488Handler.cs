namespace Bev.Instruments.Spex
{
    public interface IIeee488Handler
    {
        int DeviceAddress { get; }
        void SendBytes(byte[] b);
        void SendBytes(byte b);
        byte ReadByte();
        byte[] ReadBytes();
    }
}

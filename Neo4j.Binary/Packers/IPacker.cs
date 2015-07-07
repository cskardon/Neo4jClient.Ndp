namespace Neo4jNdpClient
{
    public interface IPacker<T>
    {
        byte[] Pack(T content);
        T Unpack(byte[] content);
    }
}
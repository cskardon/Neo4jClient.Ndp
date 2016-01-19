namespace Neo4jBoltClient
{
    public interface IPacker<T>
    {
        byte[] Pack(T content);
        T Unpack(byte[] content);
    }
}
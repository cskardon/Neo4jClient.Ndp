namespace Neo4jBoltClient
{
    using System;

    /// <summary>Instructs the NDP Client not to serialize the public field or public read/write property value.</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class BoltIgnoreAttribute : Attribute { }

    /// <summary>Instructs the Bolt Client on serialization specifics</summary>
    [AttributeUsage(AttributeTargets.Field|AttributeTargets.Property, AllowMultiple = false)]
    public sealed class BoltProperty : Attribute
    {
        /// <summary>The name to use for serialization / deserialization.</summary>
        public string Name { get; }
        
        public BoltProperty(string name = null)
        {
            Name = name;
        }
    }
}
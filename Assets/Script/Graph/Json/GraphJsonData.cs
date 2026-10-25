using UnityEngine;

namespace Graph.Json
{
    [System.Serializable]
    public class GraphJsonData
    {
        public NodeJsonData[] Node;
        public OutputPortJsonData[] Output;
        public InputPortJsonData[] Input;
    }

    public interface IGraphJsonData
    {
        public uint ID { get; set; }
    }

    public struct NodeJsonData : IGraphJsonData
    {
        [SerializeField] private uint Id;
        public uint ID { get => Id; set => Id = value; }

        public uint[] OutputPortID;
        public uint[] InputPortID;
    }

    public struct OutputPortJsonData : IGraphJsonData
    {
        [SerializeField] private uint Id;
        public uint ID { get => Id; set => Id = value; }
        public uint ConnectID;
        public ConditionGroup[] _condition;
    }

    public struct InputPortJsonData : IGraphJsonData
    {
        [SerializeField] private uint Id;
        public uint ID { get => Id; set => Id = value; }
    }
}

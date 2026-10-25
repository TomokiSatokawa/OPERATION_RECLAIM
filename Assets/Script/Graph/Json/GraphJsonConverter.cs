using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Graph.Json
{
    public class GraphJsonConverter : MonoBehaviour
    {
        private Dictionary<Node, uint> NodeDataDic = new();
        private Dictionary<OutputPort, uint> OutputPortDic = new();
        private Dictionary<InputPort, uint> InputPortDic = new();

        private static readonly JsonSerializerSettings Settings = new()
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        public void Convert()
        {
            var jsonData = new GraphJsonData();
            var nodeData = new NodeJsonData[GraphDataContainer.Nodes.Count];
            var outputPort = new OutputPortJsonData[GraphDataContainer.Output.Count];
            var inputPort = new InputPortJsonData[GraphDataContainer.Input.Count];

            CreateJsonData(nodeData, IDType.Node,
                GraphDataContainer.Nodes, NodeDataDic);

            CreateJsonData(outputPort, IDType.Output,
                GraphDataContainer.Output.Keys.ToList(), OutputPortDic);

            CreateJsonData(inputPort, IDType.Input,
              GraphDataContainer.Input, InputPortDic);

            jsonData.Node = nodeData;
            jsonData.Output = outputPort;
            jsonData.Input = inputPort;

            Debug.Log(ToJson(jsonData));
        }

        private void CreateJsonData<T1, T2>(T1[] nodeData, IDType iDType, IReadOnlyList<T2> elements, Dictionary<T2, uint> idDic)
            where T1 : struct, IGraphJsonData where T2 : class
        {
            for (uint i = 0; i < elements.Count; i++)
            {
                nodeData[i] = new T1();
                var id = CreateID(iDType, i);
                nodeData[i].ID = id;

                idDic[elements[(int)i]] = id;
            }
        }

        private uint CreateID(IDType type, uint id)
        {
            return ((uint)type) * 1_000_000_000 + id;
        }
        private static string ToJson<T>(T data)
        {
            return JsonConvert.SerializeObject(data, Formatting.Indented, Settings);
        }

        private static T FromJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, Settings);
        }


        private enum IDType
        {
            Node = 1,
            Output,
            Input
        }
    }
}
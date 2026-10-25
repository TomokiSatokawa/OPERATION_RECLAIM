using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Graph
{
    public static class GraphDataContainer
    {
        private static Dictionary<OutputPort, ConditionGroup[]> _output = new();
        private static List<InputPort> _input = new();
        private static List<Node> _nodes = new();
        public static IReadOnlyDictionary<OutputPort, ConditionGroup[]> Output => _output;
        public static IReadOnlyList<InputPort> Input => _input;
        public static IReadOnlyList<Node> Nodes => _nodes;

        public static void SaveOutput(OutputPort port, ConditionGroup[] conditions)
        {
            _output[port] = conditions;
        }

        public static void SaveInput(InputPort input)
        {
            if (_input.Contains(input))
            {
                Debug.LogError($"[GraphDataContainer]@InputPort‚ªŠù‚É“o˜^‚³‚ê‚Ä‚¢‚Ü‚·B input{input}");
                return;
            }
            _input.Add(input);
        }

        public static void SaveNode(Node node)
        {
            if (_nodes.Contains(node))
            {
                Debug.LogError($"[GraphDataContainer]@Node‚ªŠù‚É“o˜^‚³‚ê‚Ä‚¢‚Ü‚·B node{node}");
                return;
            }
            _nodes.Add(node);
        }
    }
}

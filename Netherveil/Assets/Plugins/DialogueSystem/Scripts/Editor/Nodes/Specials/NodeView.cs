using System;
using UnityEditor.Experimental.GraphView;

namespace DialogueSystem.Editor
{
    public abstract class NodeView : Node
    {
        public abstract Type type { get; }

        public string GUID;
        public GraphView graphView;

        public NodeView(string uiFile, GraphView graphView)
            : base(uiFile)
        {
            GUID = Guid.NewGuid().ToString();
            this.graphView = graphView;
        }

        public Port AddPort(Direction portDirection, Port.Capacity capacity = Port.Capacity.Single, string portName = "")
        {
            Port newPort = InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
            newPort.portName = portName;

            switch (portDirection)
            {
                case Direction.Input:
                    inputContainer.Add(newPort);
                    break;
                case Direction.Output:
                    outputContainer.Add(newPort);
                    break;
            }

            RefreshExpandedState();
            RefreshPorts();

            return newPort;
        }
    }
}

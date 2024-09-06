using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DialogueSystem.Runtime
{
    [CreateAssetMenu(fileName = "New Dialogue Tree", menuName = "Dialogue System/Dialogue Tree")]
    public class DialogueTree : ScriptableObject
    {
        public RootNode root;
        public List<Node> nodes = new List<Node>();

        public Node currentNode;

        public void ResetTree()
        {
            Process(root.child);
        }

        public void Process(Node nextNode)
        {
            currentNode = nextNode;
        }

#if UNITY_EDITOR
        public Node CreateNode(System.Type type)
        {
            Node node = CreateInstance(type) as Node;
            node.name = type.Name;
            node.GUID = GUID.Generate().ToString();
            nodes.Add(node);

            AssetDatabase.AddObjectToAsset(node, this);
            AssetDatabase.SaveAssets();
            return node;
        }

        public void DeleteNode(Node node)
        {
            nodes.Remove(node);
            AssetDatabase.RemoveObjectFromAsset(node);
            AssetDatabase.SaveAssets();
        }
#endif
    }
}

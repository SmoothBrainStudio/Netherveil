using DialogueSystem.Runtime;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem.Editor
{
    public class GraphSaveUtility
    {
        private DialogueTreeView targetGraphView;

        private DialogueTree tree => targetGraphView.tree;
        private List<Edge> edges => targetGraphView.edges.ToList();
        private List<NodeView> nodes => targetGraphView.nodes.ToList().Cast<NodeView>().ToList();

        public static GraphSaveUtility GetInstance(DialogueTreeView _targetGraphView)
        {
            return new GraphSaveUtility
            {
                targetGraphView = _targetGraphView
            };
        }

        public void SaveGraph()
        {
            string path = EditorUtility.SaveFilePanelInProject("Save dialogue tree", "new Dialogue", "asset",
                "Please enter a file name to save your Dialogue Tree");

            SaveGraph(path);
        }

        public void SaveGraph(string path)
        {
            if (!SaveNodes()) return;

            // Check if the asset already exists
            DialogueTree existingTree = AssetDatabase.LoadAssetAtPath<DialogueTree>(path);

            if (existingTree != null)
            {
                // Asset already exists, update its properties
                EditorUtility.CopySerialized(tree, existingTree);
                AssetDatabase.SaveAssets();
            }
            else
            {
                // Asset doesn't exist, create a new one
                AssetDatabase.CreateAsset(tree, path);
                AssetDatabase.SaveAssets();
            }
        }

        private bool SaveNodes()
        {
            tree.root = null;
            tree.nodes.ToList().ForEach(n => tree.DeleteNode(n));

            // Root
            //RootNodeView rootView = nodes.Where(x => x is RootNodeView).First() as RootNodeView;

            foreach (NodeView nodeView in nodes)
            {
                {
                    RootNodeView rootView = nodeView as RootNodeView;
                    if (rootView != null)
                    {
                        RootNode root = tree.CreateNode(nodeView.type) as RootNode;
                        root.GUID = nodeView.GUID;
                        root.position = nodeView.GetPosition().position;
                        tree.root = root;
                        continue;
                    }
                }

                {
                    SimpleDialogueNodeView dialogueView = nodeView as SimpleDialogueNodeView;
                    if (dialogueView != null)
                    {
                        SimpleDialogueNode dialogue = tree.CreateNode(nodeView.type) as SimpleDialogueNode;
                        dialogue.dialogueData = (nodeView as SimpleDialogueNodeView).DialogueData;
                        dialogue.GUID = nodeView.GUID;
                        dialogue.position = nodeView.GetPosition().position;
                        continue;
                    }
                }

                {
                    ChoiceDialogueNodeView dialogueView = nodeView as ChoiceDialogueNodeView;
                    if (dialogueView != null)
                    {
                        ChoiceDialogueNode dialogue = tree.CreateNode(nodeView.type) as ChoiceDialogueNode;
                        dialogue.dialogueData = (nodeView as ChoiceDialogueNodeView).DialogueData;
                        dialogue.GUID = nodeView.GUID;
                        dialogue.position = nodeView.GetPosition().position;
                        continue;
                    }
                }

                {
                    EventDialogueNodeView dialogueView = nodeView as EventDialogueNodeView;
                    if (dialogueView != null)
                    {
                        EventDialogueNodeView eventNodeView = nodeView as EventDialogueNodeView;
                        EventDialogueNode dialogue = tree.CreateNode(nodeView.type) as EventDialogueNode;
                        dialogue.dialogueData = eventNodeView.DialogueData;
                        dialogue.eventTag = eventNodeView.EventTag;
                        dialogue.GUID = nodeView.GUID;
                        dialogue.position = nodeView.GetPosition().position;
                        continue;
                    }
                }

                {
                    QuestDialogueNodeView dialogueView = nodeView as QuestDialogueNodeView;
                    if (dialogueView != null)
                    {
                        QuestDialogueNodeView eventNodeView = nodeView as QuestDialogueNodeView;
                        QuestDialogueNode dialogue = tree.CreateNode(nodeView.type) as QuestDialogueNode;
                        dialogue.dialogueData = eventNodeView.DialogueData;
                        dialogue.questTag = eventNodeView.QuestTag;
                        dialogue.difficulty = eventNodeView.Difficulty;
                        dialogue.GUID = nodeView.GUID;
                        dialogue.position = nodeView.GetPosition().position;
                        continue;
                    }
                }
            }

            edges.OrderBy(x => x.input.worldTransform.GetPosition().y).ToList().ForEach(edge =>
            {
                NodeView outputNodeView = edge.output.node as NodeView;
                NodeView inputNodeView = edge.input.node as NodeView;

                Runtime.Node outputNode = tree.nodes.Find(x => x.GUID == outputNodeView.GUID);
                Runtime.Node inputNode = tree.nodes.Find(x => x.GUID == inputNodeView.GUID);

                if (outputNode is RootNode)
                {
                    (outputNode as RootNode).child = inputNode;
                }
                if (outputNode is SimpleDialogueNode)
                {
                    (outputNode as SimpleDialogueNode).child = inputNode;
                }
                if (outputNode is ChoiceDialogueNode)
                {
                    (outputNode as ChoiceDialogueNode).AddOption(edge.output.portName, inputNode);
                }
                if (outputNode is EventDialogueNode)
                {
                    (outputNode as EventDialogueNode).child = inputNode;
                }
                if (outputNode is QuestDialogueNode)
                {
                    (outputNode as QuestDialogueNode).child = inputNode;
                }
            });

            return true;
        }

        public void LoadGraph()
        {
            LoadNodes();
            LinkNodes();
        }

        private void LoadNodes()
        {
            tree.nodes.ForEach(n =>
            {
                targetGraphView.CreateNode(n, n.position);
            });
        }

        private void LinkNodes()
        {
            tree.nodes.ForEach(n =>
            {
                RootNode root = n as RootNode;
                SimpleDialogueNode simple = n as SimpleDialogueNode;
                ChoiceDialogueNode choice = n as ChoiceDialogueNode;
                EventDialogueNode eventN = n as EventDialogueNode;
                QuestDialogueNode quest = n as QuestDialogueNode;

                List<NodeView> nodesGraph = nodes.ToList().Cast<NodeView>().ToList();

                if (root != null && root.child != null)
                {
                    Edge edge = nodesGraph.Find(x => x.GUID == root.GUID).outputContainer[0].Q<Port>()
                        .ConnectTo(nodesGraph.Find(x => x.GUID == root.child.GUID).inputContainer[0].Q<Port>());
                    targetGraphView.AddElement(edge);
                }
                else if (simple != null && simple.child != null)
                {
                    Edge edge = nodesGraph.Find(x => x.GUID == simple.GUID).outputContainer[0].Q<Port>()
                        .ConnectTo(nodesGraph.Find(x => x.GUID == simple.child.GUID).inputContainer[0].Q<Port>());
                    targetGraphView.AddElement(edge);
                }
                else if (choice != null && choice.options.Any())
                {
                    for (int i = 0; i < choice.options.Count; i++)
                    {
                        ChoiceDialogueNodeView outputNodeView = nodesGraph.Find(x => x.GUID == choice.GUID) as ChoiceDialogueNodeView;
                        NodeView inputNodeView = nodesGraph.Find(x => x.GUID == choice.options[i].child.GUID);

                        outputNodeView.AddChoicePort(choice.options[i].option);

                        Edge edge = outputNodeView.outputContainer[i].Q<Port>()
                            .ConnectTo(inputNodeView.inputContainer[0].Q<Port>());
                        targetGraphView.AddElement(edge);
                    }
                }
                else if (eventN != null && eventN.child != null)
                {
                    EventDialogueNodeView outputNodeView = nodesGraph.Find(x => x.GUID == eventN.GUID) as EventDialogueNodeView;
                    NodeView inputNodeView = nodesGraph.Find(x => x.GUID == eventN.child.GUID);

                    Edge edge = outputNodeView.outputContainer[0].Q<Port>()
                        .ConnectTo(inputNodeView.inputContainer[0].Q<Port>());
                    targetGraphView.AddElement(edge);
                }
                else if (quest != null && quest.child != null)
                {
                    QuestDialogueNodeView outputNodeView = nodesGraph.Find(x => x.GUID == quest.GUID) as QuestDialogueNodeView;
                    NodeView inputNodeView = nodesGraph.Find(x => x.GUID == quest.child.GUID);

                    Edge edge = outputNodeView.outputContainer[0].Q<Port>()
                        .ConnectTo(inputNodeView.inputContainer[0].Q<Port>());
                    targetGraphView.AddElement(edge);
                }
            });
        }
    }
}

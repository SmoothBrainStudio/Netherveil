using UnityEditor;

namespace StateMachine
{
    public static class CreateStateMachineTemplate
    {
        [MenuItem("Tools/StateMachine/MonoBehaviour")]
        public static void CreateMonoBehaviourStateMachine()
        {
            string templatePath = "Assets/Plugins/StateMachine/Templates/StateMachineTemplate.cs.txt";
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, "SampleStateMachine.cs");
        }

        [MenuItem("Tools/StateMachine/State")]
        public static void CreateState()
        {
            string templatePath = "Assets/Plugins/StateMachine/Templates/StateTemplate.cs.txt";
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, "SampleState.cs");
        }
    }
}

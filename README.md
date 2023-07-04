Created by Rob [VR Bits](http://www.vr-bits.com) 07.2023 and is 100% based on [UnityGraphViewTemplate](https://github.com/RobTranquillo/UnityGraphViewTemplate).

In heavy development, not ready for production use!

# Basic GraphView Example

Here is a simple template with a running GraphView example. The example contains 4 concrete type of nodes (Debug, Repeat, Sequence, Wait) that derrive from 3 types of basic nodes (Action, Decorator, Composite). The example also contains a custom graph editor window that allows you to create and edit graphs. The whole Graph is persisted as ScriptableObjects asset in the project.

## Usage

All you need is to put the BehaviourTreeRunner on an MonoBehaviour and add a SceneFlowConfiguration ScriptableObject to it.
The SceneFlowConfiguration can be created via the CreateAssetMenu under the folder "vrbits".




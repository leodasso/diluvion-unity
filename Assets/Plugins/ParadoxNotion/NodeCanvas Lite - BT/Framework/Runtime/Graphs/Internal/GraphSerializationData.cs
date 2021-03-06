using System.Collections.Generic;
using UnityEngine;

namespace NodeCanvas.Framework.Internal{

	///The model used to serialize and deserialize graphs. This class serves no other purpose
	[System.Serializable]
	public class GraphSerializationData {

		private const float SERIALIZATION_VERSION = 2.6f;

		public float version;
		public System.Type type;
		public string name                      = string.Empty;
		public string comments                  = string.Empty;
		public Vector2 translation              = new Vector2(-5000, -5000);
		public float zoomFactor                 = 1f;
		public List<Node> nodes                 = new List<Node>();
		public List<Connection> connections     = new List<Connection>();
		public Node primeNode                   = null;
		public List<CanvasGroup> canvasGroups   = null;
		public BlackboardSource localBlackboard = null;

		public object derivedData;

		//required
		public GraphSerializationData(){}

		//Construct
		public GraphSerializationData(Graph graph){

			this.version         = SERIALIZATION_VERSION;
			this.type            = graph.GetType();
			// this.name            = graph.name;
			this.comments        = graph.graphComments;
			this.translation     = graph.translation;
			this.zoomFactor      = graph.zoomFactor;
			this.nodes           = graph.allNodes;
			this.canvasGroups    = graph.canvasGroups;
			this.localBlackboard = graph.localBlackboard;

			//connections are serialized seperately and not part of their parent node
			var structConnections = new List<Connection>();
			for (var i = 0; i < nodes.Count; i++){
				for (var j = 0; j < nodes[i].outConnections.Count; j++){
					structConnections.Add(nodes[i].outConnections[j]);
				}
			}

			this.connections = structConnections;
			this.primeNode   = graph.primeNode;

			//serialize derived data
			this.derivedData = graph.OnDerivedDataSerialization();
		}

		///MUST reconstruct before using the data
		public void Reconstruct(Graph graph){

			//check serialization versions here in the future if needed

			//re-link connections for deserialization
			for (var i = 0; i < this.connections.Count; i++){
				connections[i].sourceNode.outConnections.Add(connections[i]);
				connections[i].targetNode.inConnections.Add(connections[i]);
			}

			//re-set the node's owner and ID
			for (var i = 0; i < this.nodes.Count; i++){
				nodes[i].graph = graph;
				nodes[i].ID = i + 1;
			}

			//deserialize derived data
			graph.OnDerivedDataDeserialization(derivedData);
		}
	}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SpiderWeb;

namespace Diluvion
{
    public class PathNode
    {
        public PathNode parentNode;
        public string name;
        public PathMono thisWP;
        public List<PathMono> neighBours;

        public float g;
        public float h;
        public float f;


        public PathNode()
        {
            SetScores(0, 0);
            name = "";
            parentNode = null;
            thisWP = null;
            neighBours = new List<PathMono>();
        }


        public PathNode(PathNode parent, PathMono wp)
        {
            if (!wp) return;
            thisWP = wp;
            name = wp.gameObject.name;
            neighBours = wp.Neighbours;
            parentNode = parent;

            if (parent == null)
                SetScores(0, 0);

        }


        public void SetScores(float gScore, float hScore)
        {
            g = gScore;
            h = hScore;
            f = g + h;
            // Debug.Log(name + " scores are = G:" + g + " H:" + h + " F:" + f);
        }
    }



    #region Pathfinding
    [ExecuteInEditMode]
    public struct PathFind
    {
        PathMono targetWP;
        List<PathMono> allWPS;

        List<PathNode> openList;
        List<PathMono> closedList;
        Vector3 myHeading;
        bool pathFound;
        bool noPossiblePath;


        //Construct a path of Vector3
     /*   public PathFind()
        {
            openList = new List<PathNode>();
            closedList = new List<PathMono>();

        }*/
 

        #region Methods

        public List<PathMono> PatrolToTarget(PathMono startNode, PathMono twp)
        {
            return PatrolFind(startNode, twp);
        }


        /// <summary>
        /// New Pathfinding caster
        /// </summary>
        /// <param name="targetPoint"></param>
        /// <param name="startPoint"></param>
        /// <returns></returns>
        public List<Vector3> OpenPathToVector3(Vector3 targetPoint, Vector3 startPoint)
        {
            List<Vector3> returnPathList = new List<Vector3>();
            
            RaycastHit hit;
            float distance = Vector3.Distance(startPoint, targetPoint);
            Ray forwardRay = new Ray(startPoint, targetPoint - startPoint);
            LayerMask terrainMask = LayerMask.GetMask("Terrain");


            ///If the way is clear, go straight for it
            if (!Physics.Raycast(forwardRay, out hit, distance, terrainMask))
            {
                returnPathList.Add(startPoint);
                returnPathList.Add(targetPoint);
                //Debug.Log("Path is clear, returning only target point");
                return returnPathList;
            }


            PathMono closestToMe = NavigationManager.Get().ClosestLOSPathMonoToPosition(hit.point+ hit.normal, startPoint);
            if (closestToMe == null)
            {
                Debug.Log("No pathmono LOS to starting point");
                return null;
            }

            Vector3 closestNodePos = closestToMe.transform.position;

            //Reverse Ray
            Ray backRay = new Ray(targetPoint, closestNodePos - targetPoint);
            float backDistance = Vector3.Distance(closestNodePos, targetPoint);


            //Debug.Log("Casting ray from end to closest node : Dist: " + backDistance);


            //Quick check to see if the first node we find has LOS and if not store the reverse hit point
            if (!Physics.Raycast(backRay, out hit, backDistance, terrainMask))
            {
               // Debug.Log("The second point is visible to the end, store it and return");
                //Debug.DrawRay(backRay.origin + Vector3.up*3, backRay.direction - Vector3.up * 3, Color.red, 5);
                returnPathList.Add(startPoint);
                returnPathList.Add(closestNodePos);
                returnPathList.Add(targetPoint);
                return returnPathList;
            }


            //Find the closest pathMono to that point
            PathMono closestToThere = NavigationManager.Get().ClosestLOSPathMonoToPosition(hit.point + hit.normal, targetPoint);

            if (closestToThere == null)
            {
                Debug.LogError("No LOS pathMono to target");
                return null;
            }
            List<Vector3> foundPath = new List<Vector3>();

            //Gets a sorted list of paths that connect
            foundPath = FindPath(closestToMe, closestToThere); // TODO Make overload that gets ONLY positions
            if (foundPath == null)
            {
                Debug.Log("Could not find a path between " + closestToMe.name + " and " + closestToThere.name, closestToMe);
                return null;
            }

            //Debug.Log("Found a path with total " + foundPath.Count + " points from " + closestToMe.name + " To " + closestToThere.name);

            //Prune from start to End

            foundPath = PruneIfLOS(foundPath, startPoint, terrainMask);
            //Debug.Log("Pruned the list to" + foundPath.Count);
            /*
           //Reverse the list to run this from end to start
           foundPath.Reverse();

           //Prune from end to start
           foundPath = PruneIfLOS(foundPath, startPoint, terrainMask);
           // Debug.Log("Pruned to " + foundPath.Count + " pathnodes");
           foundPath.Reverse();
           //for (int i = foundPath.Count-1; i>-1; i--)
           */
            returnPathList.Add(startPoint);
            for (int i = 0; i < foundPath.Count; i++)
            {
                returnPathList.Add(foundPath[i]);
            }
            returnPathList.Add(targetPoint);
            
            //Debug.Log("Returning a PathList with: " + returnPathList.Count);
            return returnPathList;

        }

        /// <summary>
        /// Removes all points except the first that have LOS of the target
        /// </summary> 
        List<Vector3> PruneIfLOS(List<Vector3> listToPrune, Vector3 target, LayerMask mask)
        {
            listToPrune.Insert(0, target);
            List<Vector3> returnList = new List<Vector3>();
            List<Vector3> validList = new List<Vector3>();
            //Find the first point with LOS TO the endPoint
            for (int i = 0; i < listToPrune.Count; i++)
            {
                Vector3 currentMonoPos = listToPrune[i];
                int lastGoodPoint = i;
                validList.Clear();

                for (int j = i; j < listToPrune.Count; j++)
                {
                    if (listToPrune[j] == listToPrune[i]) continue;

                    Vector3 nextMonoPos = listToPrune[j];

                    Ray forRay = new Ray(currentMonoPos, nextMonoPos - currentMonoPos);

                    //If this point hits terrain before hitting the end, it is a necessary point
                    if (!Physics.Raycast(forRay, Vector3.Distance(nextMonoPos, currentMonoPos), mask))
                    {
                        validList.Add(listToPrune[j]);
                        lastGoodPoint = j;
                        //Debug.DrawLine(currentMonoPos, nextMonoPos, Color.cyan, 5);
                    }
                    //else
                    //Debug.DrawLine(currentMonoPos, nextMonoPos, Color.magenta, 5);

                }

                //Debug.Log("Step:"+ i + "Found " + validList.Count + " LOS connections to " + currentMonoPos);
                //Add the very last entry of visible points on the path
                if (validList.Count > 0)
                {
                    i = lastGoodPoint - 1;
                    returnList.Add(validList.Last());
                }

            }

            return returnList;

        }



        //This class pathfinds through a list of character nodes, the lastNode and targetWP need to be inside the same list
        //It returns a sorted list of characterWaypoints from [0] to [n] which represents the path to take

        List<Vector3> PathToVector3(Vector3 endPoint, Vector3 startPoint)
        {
            List<Vector3> returnList = new List<Vector3>();

            //TODO 1 Find closest pathMono To terrain break point (Raycast

            PathMono closestToMe = Calc.FindNearestPathMono(allWPS, startPoint);

            //TODO 2 Find closest Pathmono to reverse terrain break point (raycast
            PathMono closestToThere = Calc.FindNearestPathMono(allWPS, endPoint);
            Vector3 firstWPDir = (endPoint - startPoint).normalized;

            Vector3 secondWPDir = (closestToMe.transform.position - startPoint).normalized;

            // Debug.DrawLine(startPoint, endPoint, Color.blue, 0.1f);
            //   Debug.DrawLine(startPoint, closestToMe.transform.position, Color.cyan, 5);

            float dirDot = Vector3.Dot(firstWPDir, secondWPDir);

            if ((closestToMe.transform.position - startPoint).sqrMagnitude > (endPoint - startPoint).sqrMagnitude || dirDot < 0)//If the target point is closer than the first pathmono
                if (!Physics.Raycast(startPoint, endPoint - startPoint, Calc.IncludeLayer("Terrain").value))//and there is nothing in the way
                {//Return a short list
                    returnList.Add(startPoint);
                    returnList.Add(endPoint);
                    Debug.Log("Much Closer Point, returning without pathfinding");
                    return returnList;
                }



            List<PathMono> wpPathList = PathToTarget(closestToMe, closestToThere);

            //Construct the pathfind list
            returnList.Add(startPoint);

            foreach (PathMono pw in wpPathList)
                returnList.Add(pw.transform.position);

            returnList.Add(endPoint);
            return returnList;
        }

        //Returns the closest position inside the mesh




        List<PathMono> PathToTarget(PathMono startNode, PathMono twp) //Pass in the last Waypoint the character passed
        {
            List<PathMono> sortedPath = new List<PathMono>();
            //sortedPath = FindPath(startNode, twp);
            return sortedPath;
        }


        //Finds the path between two points in the patrolPath
        List<Vector3> FindPath(PathMono startWp, PathMono endNode)// finds the path in the interiorWaypointList
        {
            List<Vector3> returnPath = new List<Vector3>();

            closedList = new List<PathMono>();
            openList = new List<PathNode>();
            targetWP = endNode;


            PathNode newNode = new PathNode(null, startWp);
            openList.Add(newNode);

            pathFound = false;
            noPossiblePath = false;
            int endBreak = 1000;
            //Iterates until it finds its target or if it discovers that there are no possible routes
            while (!pathFound || !noPossiblePath)
            {
                // always check the lowest F score first
                PathNode checkNode = FindCheapestNode(openList);

                if (!CheckNodes(checkNode))
                {
                    if (noPossiblePath)
                        break;
                }
                else
                {
                    //Debug.Log ("The Final node was: " + checkNode.name);
                    returnPath = SortedPathList(newNode, checkNode);
                    pathFound = true;
                    break;
                }
                endBreak--;
                if (endBreak < 1)
                {
                    Debug.Log("Tried 1000 times, no solution in sight");
                    break;
                }
            }
            if (pathFound)
                return returnPath;

            if (noPossiblePath)
                Debug.Log("No possible path to node");

            return null;
        }

        //Function checks the targets neigthbours and picks a direction based on the facing of the pathfinder
        PathNode GetBestPatrolDir(PathNode endNode)
        {
            float dot = -1;
            PathNode returnNode = new PathNode();
            foreach (PathMono wn in endNode.neighBours)
            {
                float tempDot = Vector3.Dot((wn.transform.position - endNode.thisWP.transform.position).normalized, myHeading.normalized);
                if (tempDot > dot)
                {
                    returnNode = new PathNode(endNode, wn);
                    dot = tempDot;
                }
            }

            return returnNode;
        }

        List<PathMono> PatrolFind(PathMono startWp, PathMono endNode)// finds the path in the interiorWaypointList
        {
            List<PathMono> returnPath = new List<PathMono>();

            closedList = new List<PathMono>();
            openList = new List<PathNode>();
            targetWP = endNode;

            PathNode newNode = new PathNode(null, targetWP);
            PathNode bestNeighbour = GetBestPatrolDir(newNode);
            openList.Add(bestNeighbour); // add the preferred neighbournode to openlist so we can start the pathfind

            closedList.Add(targetWP);//add the target node to closed for now so we dont instantly complete our patrol

            int initCheck = bestNeighbour.neighBours.Count;//lets us check the neighbours of the bestNeighbour( one of whitch is the final point ) before allowing us to find the final point(for circular path)
            PathNode previousNode = null;
            int breakCatch = 1000;

            //PathNode firstNode = null;
            pathFound = false;
            noPossiblePath = false;
            // bool lookForStart = false;

            //Iterates until it finds its target or if it discovers that there are no possible routes
            while (!pathFound || !noPossiblePath)
            {
                if (breakCatch < 1)
                {
                    Debug.Log("PATHED " + breakCatch + " TIMES, NO RESULT");
                    break;
                }

                if (initCheck < 1)
                {
                    //lookForStart = true;
                    closedList.Remove(targetWP);
                }

                // always check the lowest F score first
                PathNode checkNode = FindCheapestNode(openList);//All our scores are going to be uniform, we dont 
                                                                //Debug.Log("PatrolFind Solver " + (1000 - breakCatch) + ", CheckNode is: " + checkNode.thisWP.name + " openList has: " + openList.Count + " nodes to check" ) ;
                if (!CheckNodes(checkNode))
                {
                    previousNode = checkNode;
                    initCheck--;
                    if (noPossiblePath)
                        break;
                }
                else
                {
                    //Debug.Log("The Final node was: " + checkNode.name);
                    checkNode.parentNode = previousNode;
                    returnPath = SortedPatrolList(newNode, checkNode);
                    pathFound = true;
                    break;
                }

                breakCatch--;
            }
            if (pathFound)
                return returnPath;

            if (noPossiblePath)
                Debug.Log("No possible path to node");
            return null;
        }

        List<Vector3> SortedPathList(PathNode firstNode, PathNode endNode)// returns the characterWP list that the character needs to follow
        {
            List<Vector3> returnList = new List<Vector3>();
            // Debug.Log("First node: " + firstNode.name + " End node: " + endNode.name);
            PathNode pNode = endNode;

            returnList.Insert(0, pNode.thisWP.transform.position); //inserts
            while (pNode != firstNode) // Iterates through parents back to the start
            {
                pNode = pNode.parentNode;
                returnList.Insert(0, pNode.thisWP.transform.position); //inserts
                                                                       // Debug.Log(pNode.name + " is one of the nodes for the path!");
            }
            //Debug.Log("Sorted path returnlist is: " + returnList.Count);
            return returnList;
        }


        List<PathMono> SortedPatrolList(PathNode firstNode, PathNode endNode)// returns the characterWP list that the character needs to follow
        {
            List<PathMono> returnList = new List<PathMono>();

            PathNode pNode = endNode.parentNode;

            if (pNode == null) return returnList;

            returnList.Insert(0, pNode.thisWP); //insert at 0 to reverse list   
            while (pNode != firstNode) // Iterates through parents back to the start
            {
                pNode = pNode.parentNode;
                returnList.Insert(0, pNode.thisWP); //inserts
            }
            return returnList;
        }

        PathNode FindCheapestNode(List<PathNode> pNList)// Takes a Pathnode list and finds the lowest F cost
        {
            if (pNList.Count < 1) return null;
            PathNode returnNode = null;
            List<PathNode> sortList = pNList.OrderBy(pn => pn.f).ToList();
            returnNode = sortList[0];
            return returnNode;
        }

        PathNode FindCheapestPatrolNode(List<PathNode> pNList)// Takes a Pathnode list and finds the lowest F cost
        {
            PathNode returnNode = null;
            foreach (PathNode p in pNList)
            {
                if (p.f < 9999999)
                {
                    returnNode = p;
                }
            }
            return returnNode;
        }


        bool CheckNodes(PathNode thisNode) // checks the node for neighbours and adds them to openList while removing itself from openlist
        {
            if (thisNode == null) return false;

            if (thisNode.thisWP == targetWP)//We found the target
            {
                return true;
            }

            if (openList.Count < 1) //we ran out of open nodes, i.e we cant find a path
            {
                noPossiblePath = true;
                return false;
            }

            if (thisNode.neighBours == null)
                Debug.Log(thisNode.name + " has no neighbours " + thisNode.thisWP);
            foreach (PathMono neighbour in thisNode.neighBours) //Check for neighbours in Node
            {
                if (neighbour == null) continue;
                if (!closedList.Contains(neighbour))
                {
                    PathNode neighBourNode = new PathNode(thisNode, neighbour);
                    float dist = Vector3.Distance(neighbour.transform.position, targetWP.transform.position); //Get distance for h score
                    float prevG = thisNode.g;

                    neighBourNode.SetScores(prevG + 1, dist);
                    openList.Add(neighBourNode);
                }
            }

            openList.Remove(thisNode);
            closedList.Add(thisNode.thisWP);

            return false;
        }

        //TODO Fix for patrols

        bool CheckPatrol(PathNode thisNode, bool lookForStart) //Checks the node for neighbours and if this is the final node, EXCEPT FOR THE FIRST ITERATION
        {

            if (thisNode == null) return false;
            //Debug.Log("Checking Patrol: " + thisNode.name);

            if (thisNode.thisWP == targetWP)//We found the target  //TODO this might be decrepit as we are making a weighted list from the start      
                if (lookForStart)
                    return true;


            if (openList.Count < 1) //we ran out of open nodes, i.e we cant find a path
            {
                noPossiblePath = true;
                return false;
            }

            if (thisNode.neighBours.Count == 1)
            {
                //this node has only one neighbour, the one you came from, its a dead end
                // Debug.Log("singleNeighbour");
                return false;
            }

            foreach (PathMono neighbour in thisNode.neighBours) //Check for neighbours in Node
            {
                if (!closedList.Contains(neighbour))
                {
                    PathNode neighBourNode = new PathNode(thisNode, neighbour);
                    //float dist = -Vector3.SqrMagnitude(targetWP.transform.position - neighbour.transform.position); //Get distance from neighbour to target
                    //This will only work with a single directional patrol, if we have alternate routes it might be iffy              

                    neighBourNode.SetScores(1, 1);
                    openList.Add(neighBourNode);
                }
            }


            openList.Remove(thisNode);
            closedList.Add(thisNode.thisWP);

            return false;
        }

        #endregion
    }
}

#endregion


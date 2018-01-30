using System.Collections.Generic;
using UnityEngine;

//Good tutorial at: http://www.policyalmanac.org/games/aStarTutorial.htm
public class PathPlanningClass {
    //constructors
    public PathPlanningClass(int newStopCondition)
    {
        //initialize
        nodesToCheck = new List<NodeClass>();
        nodesChecked = new List<NodeClass>();
        stopCondition = newStopCondition;
        originNode = new NodeClass();
        destinationNode = new NodeClass();
        cellRadius = GameObject.Find("GameController").GetComponent<GameController>().cellRadius;
        terrainLimits = new Vector3(GameObject.Find("Terrain").GetComponent<Terrain>().terrainData.size.x, 0, GameObject.Find("Terrain").GetComponent<Terrain>().terrainData.size.z);
    }
    
    //open list of cells to check
    private List<NodeClass> nodesToCheck;
    //closed list, which will be used to determine the path
    private List<NodeClass> nodesChecked;
    //destination node
    private NodeClass destinationNode;
    //origin node
    private NodeClass originNode;
    //stop condition
    private int stopCondition;
    //cell radius
    private float cellRadius;
    //terrain limits
    private Vector3 terrainLimits;

    //find a path between two points
    public List<GameObject> FindPath(GameObject cellOrigin, GameObject cellDestination)
    {
        //add the origin cell to the open list
        NodeClass newNode = new NodeClass();
        newNode.cell = cellOrigin;
        nodesToCheck.Add(newNode);

        //set the origin node
        originNode.cell = cellOrigin;

        //set the destination node
        destinationNode.cell = cellDestination;

        //to control the stop condition
        int nrIt = 0;
        bool foundPath = false;
        //while there are nodes to check, repeat
        while(nodesToCheck.Count > 0)
        {
            nrIt++;
            //order the list
            ReorderCheckList();

            //check the neighbour cells of the first node of the list and create their nodes
            FindNodes(nodesToCheck[0]);

            //if arrived at destination, finished
            if(nodesChecked[nodesChecked.Count - 1].cell.name == destinationNode.cell.name)
            {
                foundPath = true;
                break;
            }

            //if nrIt is bigger than the stop condition, byyye
            if (nrIt > stopCondition) break;
        }

        //path
        List<GameObject> path = new List<GameObject>();

        //add the destination
        path.Add(destinationNode.cell);

        //if found Path, make the reverse way to mount it
        //else, path is empty. Agent tries to go directly towards it
        if (foundPath)
        {
            NodeClass nodi = nodesChecked[nodesChecked.Count - 1];
            while(nodi.parent != null)
            {
                //add to path
                path.Add(nodi.parent.cell);
                //update node with the parent
                nodi = nodi.parent;
            }
        }

        //revert it
        path.Reverse();

        //clear lists
        nodesChecked.Clear();
        nodesToCheck.Clear();

        //now the full path is ready, find only the corners
        List<GameObject> cornerPath = new List<GameObject>();
        for(int i = 1; i < path.Count - 1; i++)
        {
            //difference between next position and actual position
            float nextDiffX = path[i + 1].transform.position.x - path[i].transform.position.x;
            float nextDiffZ = path[i + 1].transform.position.z - path[i].transform.position.z;

            //difference between actual position and last position
            float lastDiffX = path[i].transform.position.x - path[i - 1].transform.position.x;
            float lastDiffZ = path[i].transform.position.z - path[i - 1].transform.position.z;

            //if the difference just calculated is equal than the difference between actual position and last position, it is following a straight line. So, no need for corner
            //otherwise, add it
            if(nextDiffX != lastDiffX || nextDiffZ != lastDiffZ)
            {
                cornerPath.Add(path[i]);
            }
        }

        //if goal is not already in the list, add it
        if (!cornerPath.Contains(cellDestination))
        {
            cornerPath.Add(cellDestination);
        }

        return cornerPath;
    }

    //reorder the nodes to check list, placing the lowest f at first
    private void ReorderCheckList()
    {
        for(int i = 0; i < nodesToCheck.Count; i++)
        {
            for (int j = 0; j < nodesToCheck.Count; j++)
            {
                //if second one is higher??? (worked...) than the first one, change places
                if(nodesToCheck[j].f > nodesToCheck[i].f)
                {
                    NodeClass auxNode = nodesToCheck[i];
                    nodesToCheck[i] = nodesToCheck[j];
                    nodesToCheck[j] = auxNode;
                }
            }
        }
    }

    //find nodes around the chosen node
    private void FindNodes(NodeClass chosenNode)
    {
        //find all neighbours cells
        float cellSize = cellRadius * 2;
        float startX = (chosenNode.cell.transform.position.x - cellSize);
        float startZ = (chosenNode.cell.transform.position.z - cellSize);
        float endX = (chosenNode.cell.transform.position.x + cellSize);
        float endZ = (chosenNode.cell.transform.position.z + cellSize);

        //see if it is in some border
        if (chosenNode.cell.transform.position.x == cellRadius)
        {
            startX = chosenNode.cell.transform.position.x;
        }
        if (chosenNode.cell.transform.position.z == cellRadius)
        {
            startZ = chosenNode.cell.transform.position.z;
        }
        if (chosenNode.cell.transform.position.x == terrainLimits.x - cellRadius)
        //if ((int)cell.transform.position.x == 29)
        {
            endX = chosenNode.cell.transform.position.x;
        }
        if (chosenNode.cell.transform.position.z == terrainLimits.z - cellRadius)
        //if ((int)cell.transform.position.z == 29)
        {
            endZ = chosenNode.cell.transform.position.z;
        }

        //iterate around the chosen node cell
        for (float i = startX; i <= endX; i = i + cellSize)
        {
            for (float j = startZ; j <= endZ; j = j + cellSize)
            {
                //find the cell
                //GameObject neighbourCell = GameObject.Find("Cell" + i + "X" + j);
                GameObject neighbourCell = CellController.GetCellByName("Cell" + i + "X" + j);

                //if it exists..
                if (neighbourCell)
                {
                    //and if it is not the chosen cell
                    if (neighbourCell.name != chosenNode.cell.name)
                    {
                        //see it this node is not already in closed list
                        bool goAhead = true;

                        foreach (NodeClass nd in nodesChecked)
                        {
                            if (nd.cell.name == neighbourCell.name)
                            {
                                goAhead = false;
                                break;
                            }
                        }

                        //if it is not
                        if (goAhead)
                        {
                            //check if this node is not already on the open node list
                            int alreadyInside = -1;
                            for (int z = 0; z < nodesToCheck.Count; z++)
                            {
                                if (nodesToCheck[z].cell.name == neighbourCell.name)
                                {
                                    alreadyInside = z;
                                    break;
                                }
                            }

                            //if it is, check to see if this chosen path is better
                            if (alreadyInside > -1)
                            {
                                //if the g value of chosenNode, plus the cost to move to this neighbour, is lower than the nodeG value, this path is better. So, update
                                //otherwise, do nothing.
                                int extraCost = 14;
                                if (nodesToCheck[alreadyInside].cell.transform.position.x == chosenNode.cell.transform.position.x ||
                                    nodesToCheck[alreadyInside].cell.transform.position.z == chosenNode.cell.transform.position.z)
                                {
                                    extraCost = 10;
                                }

                                if ((chosenNode.g + extraCost) < nodesToCheck[alreadyInside].g)
                                {
                                    //re-calculate the values
                                    nodesToCheck[alreadyInside].g = (chosenNode.g + extraCost);
                                    nodesToCheck[alreadyInside].f = nodesToCheck[alreadyInside].g + nodesToCheck[alreadyInside].h;
                                    //change parent
                                    nodesToCheck[alreadyInside].parent = chosenNode;
                                }
                            }//else, just create it
                            else
                            {
                                //create its node
                                NodeClass newNode = new NodeClass();
                                //initialize
                                newNode.cell = neighbourCell;
                                newNode.g = 14;
                                //set its values
                                //h value
                                newNode.h = EstimateDestination(newNode);
                                //g value
                                //if x or z axis is equal to the chosen node cell, it is hor/ver movement, so it costs only 10
                                if (newNode.cell.transform.position.x == chosenNode.cell.transform.position.x || newNode.cell.transform.position.z == chosenNode.cell.transform.position.z)
                                {
                                    newNode.g = 10;
                                }

                                //density confort: just sum up the qnt of agents on this cell node, * 50
                                //TO SEE: it gets cells which have agents in that instant, but agents may be still moving
                                //newNode.dc = newNode.cell.GetComponent<CellController>().agents.Count * 50;

                                //f, just sums h with g
                                //new stuff: adds up the thermal comfort too
                                newNode.f = newNode.h + newNode.g;
                                //set the parent node
                                newNode.parent = chosenNode;

                                //add this node in the open list
                                nodesToCheck.Add(newNode);
                            }
                        }
                    }
                }
            }
        }

        //done with this one
        nodesChecked.Add(chosenNode);
        nodesToCheck.Remove(chosenNode);
    }

    //estimate the h node value
    private int EstimateDestination(NodeClass checkingNode)
    {
        int manhattanWay = 0;

        //since it is a virtual straight path, just sum up the differences in axis x and z
        float differenceX = Mathf.Abs(destinationNode.cell.transform.position.x - checkingNode.cell.transform.position.x);
        float differenceZ = Mathf.Abs(destinationNode.cell.transform.position.z - checkingNode.cell.transform.position.z);

        //sum up and multiply by the weight (10)
        manhattanWay = (int)(differenceX + differenceZ) * 10;

        return manhattanWay;
    }
}

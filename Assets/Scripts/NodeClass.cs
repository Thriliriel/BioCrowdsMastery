using UnityEngine;

public class NodeClass {
    //constructors
    public NodeClass()
    {
        //initialize
        f = g = h = 0;
    }

    //g + h. The node with the lowest f is used as the next node to look at in the open list
    public int f;
    //movement cost. For horizontal/vertical movement, value = 10. For diagonal movement, value = 14.
    public int g;
    //estimated cost to reach the destination. Manhattan method can be used, where you calculate the total number of squares moved horizontally and vertically 
    //to reach the target square from the current square, ignoring diagonal movement, and ignoring any obstacles that may be in the way. OFC, multiply it for 10 (g cost)
    public int h;
    //position
    public GameObject cell;
    //parent node
    public NodeClass parent;
}

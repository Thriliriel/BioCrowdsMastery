using UnityEngine;

public class GoalController : MonoBehaviour
{
    public GameObject cell;

    public GameObject GetCell()
    {
        return cell;
    }

    public void SetCell(GameObject newCell)
    {
        cell = newCell;
    }
}

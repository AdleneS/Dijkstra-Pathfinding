using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public int tileX;
    public int tileY;
    public LevelGenerator LG;
    public List<Node> currentPath = null;

    private bool m_moving;
    private int moveSpeed = 1;


    void Start()
    {
        LG = GameObject.Find("LevelGenerator").GetComponent<LevelGenerator>();
    }
    void Update()
    {
        if (currentPath != null)
        {
            int currNode = 0;
            while (currNode < currentPath.Count - 1)
            {
                Vector3 start = LG.TileCoordToWorldCoord(currentPath[currNode].x, currentPath[currNode].y) + new Vector3(0, 0, -1f);
                Vector3 end = LG.TileCoordToWorldCoord(currentPath[currNode + 1].x, currentPath[currNode + 1].y) + new Vector3(0, 0, -1f);

                Debug.DrawLine(start, end, Color.blue);

                currNode++;
            }
        }

        /*if (LG.showTiles && currentPath != null )
        {
            int p = 0;
            foreach (Node tilesInMyPath in currentPath)
            {
                p++;
                LG.ShowVisualPath(currentPath[p-1].x, currentPath[p-1].y);
            }
        }*/
    }

    private void OnMouseUp()
    {
        LG.GetCharacter(gameObject);
    }

    public void StartMove()
    {
        //Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.Locked;
        StartCoroutine(LetsMove());
    }

    IEnumerator LetsMove()
    {

        while (currentPath != null)
        {
            yield return new WaitForSeconds(0.5f);
            MoveNextTile();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        yield return null;
    }

    public void MoveNextTile()
    {
        float remainingMovement = moveSpeed;

        while(remainingMovement > 0)
        {
            if (currentPath == null)
            {
                return;
            }

            //Get cost from current tile to next tile
            remainingMovement -= LG.CostToEnterTile(currentPath[0].x, currentPath[0].y, currentPath[1].x, currentPath[1].y);

            //Move us to the next tile in the sequence
            tileX = currentPath[1].x;
            tileY = currentPath[1].y;
            transform.position = LG.TileCoordToWorldCoord(tileX,tileY) + new Vector3(0, 0, transform.position.z);


            //Remove the old "current" tile
            currentPath.RemoveAt(0);

            if (currentPath.Count == 1)
            {
                currentPath = null;
            }
        }
    }

    
}

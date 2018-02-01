using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableTile : MonoBehaviour {

    public int tileX;
    public int tileY;
    public LevelGenerator LG;


    void OnMouseUp()
    {
        if(LG.characterSelect != null)
            LG.GeneratePathTo(tileX, tileY);
    }
}

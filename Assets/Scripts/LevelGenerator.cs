using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class LevelGenerator : MonoBehaviour {

    public GameObject characterSelect;

    public Texture2D map;
    public ColorToPrefab[] colorMappings;
    public GameObject[,] visibleTile;
    public bool showTiles = false;
    public Button moveBtn;

    int[,] tiles;
    Node[,] graph;

    

    void Start ()
    {
        //characterSelect.GetComponent<Character>().tileX = (int)characterSelect.transform.position.x;
        //characterSelect.GetComponent<Character>().tileY = (int)characterSelect.transform.position.y;
        //characterSelect.GetComponent<Character>().LG = this;
        moveBtn = GameObject.Find("MoveBtn").GetComponent<Button>();
        GenerateLevel();
        GeneratePathfindingGraph();

    }

    public void GetCharacter(GameObject chara)
    {
        characterSelect = chara;
        chara.GetComponent<Character>().tileX = (int)chara.transform.position.x;
        chara.GetComponent<Character>().tileY = (int)chara.transform.position.y;
        chara.GetComponent<Character>().LG = this;
        moveBtn.onClick.AddListener(chara.GetComponent<Character>().StartMove);
    }

    public float CostToEnterTile(int sourceX, int sourceY,int targetX, int targetY)
    {
        ColorToPrefab CTP = colorMappings[tiles[targetX, targetY]];

        float cost = CTP.movementCost;

        if (CharacterCanEnterTile(targetX, targetY) == false)
        {
            return Mathf.Infinity;
        }
        return cost;
    }

    void GeneratePathfindingGraph()
    {
        //Initialisze the array
        graph = new Node[map.width, map.height];

        //Initialize a node for each spot in the array
        for (int x = 0; x < map.width; x++)
        {
            for (int y = 0; y < map.height; y++)
            {
                graph[x, y] = new Node();            
                graph[x, y].x = x;
                graph[x, y].y = y;
            }
        }
        //Now that all the nodes exist, calculate their neighbours
        for (int x = 0; x < map.width; x++)
        {
            for (int y = 0; y < map.height; y++)
            {
                //We Have 4-way connected map
                if (x > 0)
                    graph[x, y].neighbours.Add(graph[x - 1, y]);
                if (x < map.width-1)
                    graph[x, y].neighbours.Add(graph[x + 1, y]);
                if (y > 0)
                    graph[x, y].neighbours.Add(graph[x, y-1]);
                if (y < map.height - 1)
                    graph[x, y].neighbours.Add(graph[x, y+1]);

            }
        }
    }

    public void ShowVisualPath(int x,int y)//GameObject colorTile)
    {
        visibleTile[x, y].GetComponent<SpriteRenderer>().color = new Color(255, 0, 0);
    }
    
    public void GenerateLevel()
    {
        tiles = new int[map.width, map.height];
        visibleTile = new GameObject[map.width,map.height];

        int x, y;

        string holderName = "Generated Map";

        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        for ( x = 0; x < map.width; x++)
        {
            for ( y = 0; y < map.height; y++)
            {               
                Color pixelColor = map.GetPixel(x, y);
                if (pixelColor.a == 0)
                {
                    return;
                }

                foreach (ColorToPrefab colorMapping in colorMappings)
                {
                    if (colorMapping.color.Equals(pixelColor))
                    {
                        Vector2 position = new Vector2(x, y);
                        GameObject newTile = Instantiate(colorMapping.Prefab, position, Quaternion.identity, mapHolder);
                        tiles[x, y] = colorMapping.number;
                        ClickableTile ct = newTile.GetComponent<ClickableTile>();
                        ct.tileX = x;
                        ct.tileY = y;
                        ct.LG = this;
                        visibleTile[x, y] = newTile;
                        //ShowVisualPath(x, y);
                    }
                }
            }
        }
    }

    public Vector3 TileCoordToWorldCoord(int x, int y)
        {
            return new Vector3(x, y,0);
        }

    public bool CharacterCanEnterTile(int x, int y)
    {
        return colorMappings[tiles[x, y]].isWalkable;
    }

    public void GeneratePathTo(int x, int y)
    {
        //Clear out our char old path
        characterSelect.GetComponent<Character>().currentPath = null;

        if (CharacterCanEnterTile(x,y) == false )
        {
            return;
        }

        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();

        //List of nodes we haven't checked yet
        List<Node> unvisited = new List<Node>();

        Node source = graph[
            characterSelect.GetComponent<Character>().tileX, 
            characterSelect.GetComponent<Character>().tileY
            ];
        Node target = graph[x, y];

        dist[source] = 0;
        prev[source] = null;

        //Initialize everything to have Infinity distance, since we don't any better right now
        foreach (Node v in graph)
        {
            if (v != source)
            {
                dist[v] = Mathf.Infinity;
                prev[v] = null;
            }

            unvisited.Add(v);
        }

        while (unvisited.Count > 0)
        {
            // u is going to be the unvisited node with the smallest distance

            Node u = null;

            foreach (Node possibleU in unvisited)
            {
                if (u == null || dist[possibleU] < dist[u])
                {
                    u = possibleU;
                }
            }

            if (u == target)
            {
                break;
            }

            unvisited.Remove(u);

            foreach (Node v in u.neighbours)
            {
                //float alt = dist[u] + u.DistanceTo(v);
                float alt = dist[u] + CostToEnterTile(u.x,u.y,v.x,v.y);

                if (alt < dist[v])
                {
                    dist[v] = alt;
                    prev[v] = u;
                }
            }
        }

        //If we get there, the either we founf the shortest route 
        //to our target, or there is not route at all to our target
        if (prev[target] == null)
        {
            //No route between our target and the source !
            return;
        }
        List<Node> currentPath = new List<Node>();
        Node curr = target;

        //step through the prev chain and add it to our path
        while (curr !=null)
        {
            currentPath.Add(curr);
            curr = prev[curr];
        }

        //Right now, currentPath describes a route from out target to our source
        //So we need to invert it !
        currentPath.Reverse();

        characterSelect.GetComponent<Character>().currentPath = currentPath;

    }
}

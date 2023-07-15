using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private Transform navMeshFloor;
    [SerializeField] private Transform tilePrefab;
    [SerializeField] private Transform obstaclePrefab;
    [SerializeField] private Transform navMeshMaskPrefab;

    [SerializeField] private Vector2 maxMapSize;
    [SerializeField] private float tileSize; // to make map bigger or smaller
    [SerializeField] [Range(0,1)] public float outlinePercent;
    
    [SerializeField] private Map[] maps;
    [SerializeField] private int mapIndex; 
    Map currentMap;

    Transform[,] tileMap; //to store the transform of all the tiles generated
    Queue<Coordinate> shuffledOpenTilesCoordinate;

    List<Coordinate> allTilesCoordinate;
    Queue<Coordinate> shuffledTilesCoordinate;

    [System.Serializable]
    public class Map{

        public Coordinate mapSize;
        [Range(0,1)] public float obstaclePercent;
        public int seed;
        public float minObstacleHeight;
        public float maxObstacleHeight;
        public Color foreGroundColor;
        public Color backGroundColor;
        
        public Coordinate mapCentre{
            get{
                return new Coordinate (mapSize.x/2, mapSize.y/2);
            }
        }
    }

    private void Awake() {
        GenerateMap();
    }

    public void GenerateMap(){
        currentMap = maps[mapIndex];
        System.Random prng = new System.Random(currentMap.seed); 
        //declaring the size of box collider of the map so player can walk on it
        GetComponent<BoxCollider>().size = new Vector3(currentMap.mapSize.x * tileSize, 0.5f, currentMap.mapSize.y * tileSize);
        tileMap = new Transform[currentMap.mapSize.x, currentMap.mapSize.y];

        // Generating and adding coordinates to the list
        allTilesCoordinate = new List<Coordinate>();
        for(int x = 0; x < currentMap.mapSize.x; x++){
            for(int y = 0; y < currentMap.mapSize.y; y ++){
                allTilesCoordinate.Add(new Coordinate(x, y));
            }
        }

        shuffledTilesCoordinate = new Queue<Coordinate>(Utility.FisherRatesShuffle(allTilesCoordinate.ToArray(), currentMap.seed));

        //deleting the existing map when we change the editor
        string holderName = "Generated Map";
        if(transform.Find(holderName)){
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        //generating map
        for(int x = 0; x < currentMap.mapSize.x; x++){
            for(int y = 0; y < currentMap.mapSize.y; y ++){
                //starting from bottm left
                Vector3 tilePosition = CoordinateToPosition(x, y); 
                Transform tileTransform = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90));
                tileTransform.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                tileTransform.parent = mapHolder;

                tileMap[x,y] = tileTransform; // allocating tileMap
            }
        }

        navMeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * tileSize;

        SpawnNavMeshMask(mapHolder);
        SpawnObstacles(mapHolder, prng, currentMap);
    }

    //Spawning Nav Mesh Mask on the edges of map
    private void SpawnNavMeshMask(Transform mapHolder){
        //creating left Nav Mesh Mask
        Transform leftMask = Instantiate(navMeshMaskPrefab, Vector3.left * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity);
        leftMask.parent = mapHolder;
        leftMask.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;

        //creating right Nav Mesh Mask
        Transform rightMask = Instantiate(navMeshMaskPrefab, Vector3.right * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity);
        rightMask.parent = mapHolder;
        rightMask.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;

        //creating top Nav Mesh Mask
        Transform topMask = Instantiate(navMeshMaskPrefab, Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity);
        topMask.parent = mapHolder;
        topMask.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;

        //creating bottom Nav Mesh Mask
        Transform bottmMask = Instantiate(navMeshMaskPrefab, Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity);
        bottmMask.parent = mapHolder;
        bottmMask.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;
    }

    private void SpawnObstacles(Transform mapHolder, System.Random prng, Map currentMap){
        bool[,] obstacleMap = new bool[(int)currentMap.mapSize.x, (int)currentMap.mapSize.y];
        int currentObstacleCount = 0;
        //first we have all tiles and when spawn a oobstackle we delete that tile from list 
        List<Coordinate> allOpenCoordinates = new List<Coordinate> (allTilesCoordinate);

        // for the number of obstacles we go to shuufled Queue and get one and Instanciate it
        int obstacleNumbers = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent);
        for(int i =0; i < obstacleNumbers; i++ ){
            Coordinate randomCoord = getRandomCoordinate();
            obstacleMap [randomCoord.x, randomCoord.y] = true; //???
            currentObstacleCount ++;

            if(randomCoord != currentMap.mapCentre && MapIsFullyAccessible(obstacleMap, currentObstacleCount)){
                //calculating obstacle height base on random number
                float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float)prng.NextDouble());

                Vector3 obstaclePosition = CoordinateToPosition(randomCoord.x, randomCoord.y);
                Transform obstacleTransform = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * obstacleHeight/2f, Quaternion.identity);
                obstacleTransform.localScale = new Vector3((1 - outlinePercent) * tileSize, obstacleHeight, (1 - outlinePercent) * tileSize);
                obstacleTransform.parent = mapHolder;

                //managing the color of obstacles using shared material
                Renderer obstacleRenderer = obstacleTransform.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
                //calculating colot percent base on the position of the obstacle
                float colorPercent = randomCoord.y / (float)currentMap.mapSize.y;
                obstacleMaterial.color = Color.Lerp(currentMap.foreGroundColor, currentMap.backGroundColor, colorPercent);
                obstacleRenderer.material = obstacleMaterial;

                allOpenCoordinates.Remove(randomCoord);

            }else{
                // map will not be accesible and we return the process
                obstacleMap [randomCoord.x, randomCoord.y] = false; 
                currentObstacleCount --;
            }   
        }

        shuffledOpenTilesCoordinate = new Queue<Coordinate>(Utility.FisherRatesShuffle(allOpenCoordinates.ToArray(), currentMap.seed));
    }

    //floodfill algorithm
    private bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount){
        //for saving witch tiles we have seen
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
        Queue<Coordinate> coorQueue = new Queue<Coordinate>();

        // starting with the map centre and its always true
        coorQueue.Enqueue(currentMap.mapCentre);
        mapFlags[currentMap.mapCentre.x, currentMap.mapCentre.y] = true;

        int accesibleTileCount = 1; // we have already added mapCentre

        //floodfill
        while(coorQueue.Count > 0){
            //get the first coordinate of the Queue
            Coordinate tile = coorQueue.Dequeue();

            //loop through each of the adjacent tiles 
            for(int x = -1; x <= 1; x ++){
                for(int y = -1; y <= 1; y ++){
                    int neighborX = tile.x + x;
                    int neighborY = tile.y + y;
                    // we don't want to check the diagnal which both x and y are not 0
                    if(x == 0 || y == 0){
                        // check if the adjacent tile is in the obstacle map
                        if(neighborX >= 0 && neighborX < obstacleMap.GetLength(0) && neighborY >= 0 && neighborY < obstacleMap.GetLength(1)){
                            //if we have not check it before and is not obstacle map
                            if(!mapFlags[neighborX,neighborY] && !obstacleMap[neighborX,neighborY]){
                                mapFlags[neighborX,neighborY] = true; 
                                coorQueue.Enqueue(new Coordinate(neighborX, neighborY)); // adding to Queue so will be checked its neighbors
                                accesibleTileCount ++;
                            }
                        }
                    }
                }
            }
        }
        //the max number of accessible tiles according to number of obstacle we have
        int targetAccessibleTileCount = (int)((currentMap.mapSize.x * currentMap.mapSize.y) - currentObstacleCount); 
        return targetAccessibleTileCount == accesibleTileCount;
    }

    public Coordinate getRandomCoordinate(){
        //getting random coordinate and adding it to the end of queue to not loose it
        Coordinate randomCoord = shuffledTilesCoordinate.Dequeue();
        shuffledTilesCoordinate.Enqueue(randomCoord);
        return randomCoord;
    }

    public Transform getRandomOpenTile(){
        Coordinate randomCoord = shuffledOpenTilesCoordinate.Dequeue();
        shuffledOpenTilesCoordinate.Enqueue(randomCoord);
        return tileMap[randomCoord.x, randomCoord.y];
    }

    private Vector3 CoordinateToPosition(int x, int y){
        return new Vector3(-currentMap.mapSize.x/2f + 0.5f + x, 0, -currentMap.mapSize.y/2f + 0.5f + y) * tileSize;
    }

    // this is reverse of the CoordinateToPosition function
    public Transform GetTileFromPosition(Vector3 position){
        int x = Mathf.RoundToInt(position.x / tileSize + (currentMap.mapSize.x-1) / 2f);
        int y = Mathf.RoundToInt(position.z / tileSize + (currentMap.mapSize.y-1) / 2f);
        // making sure that x,y are in the tilemap and not outside
        x = Mathf.Clamp(x, 0, tileMap.GetLength(0) -1); 
        y = Mathf.Clamp(y, 0, tileMap.GetLength(1) -1);
        return tileMap[x,y];
    }

    // saving all the coordinates in map
    [System.Serializable]
    public struct Coordinate {
        public int x;
        public int y;

        public Coordinate(int _x, int _y){
            x = _x;
            y = _y;
        }

        public static bool operator == (Coordinate c1, Coordinate c2){
            return c1.x == c2.x && c1.y == c2.y;
        }

        public static bool operator != (Coordinate c1, Coordinate c2){
            return ! (c1 == c2);
        }
    }

    
}

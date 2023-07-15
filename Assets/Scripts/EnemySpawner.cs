using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // storing every data a wave needs
    [System.Serializable]
    public class Wave{
        public int enemyCount;
        public float timeBetweenSpawns;
    }

    [SerializeField] private Transform enemyPrefab;
    [SerializeField] private Wave[] waves;
    private LivingEntity playerEntity;
    private Transform playerTransform;

    private MapGenerator map;

    Wave currentWave;
    private int currentWaveNumber;
    private int enemiesRemainingToSpawn;
    private int enemiesRemainingAlive;
    private float nextSpawnTime = 0f;

    //camping values
    private float timeBetweenCampingCheck = 2f;
    private float campThreshholdDistance = 1.5f;
    private float nextCampCheckTime;
    Vector3 campPositionOld;
    bool isCamping;
    bool isDisabled = false;

    private void Start() {
        playerEntity = FindAnyObjectByType<Player>();
        playerTransform = playerEntity.transform;

        nextCampCheckTime = timeBetweenCampingCheck + Time.time;
        campPositionOld = playerTransform.position;


        map = FindObjectOfType<MapGenerator>();
        NextWave();
    }

    private void Update() {
        if(!isDisabled){
            //checking if player has moved in the time we given to him
            if(Time.time > nextCampCheckTime){
                nextCampCheckTime = Time.time + timeBetweenCampingCheck;
                isCamping = (Vector3.Distance(playerTransform.position, campPositionOld)) < campThreshholdDistance;
                campPositionOld = playerTransform.position;
                playerEntity.OnDeath += OnPlayerDeath;
            }

            if(enemiesRemainingToSpawn > 0 && Time.time > nextSpawnTime){
                enemiesRemainingToSpawn --;
                nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;
                StartCoroutine(SpawnEnemy());
            }
        } 
    }

    IEnumerator SpawnEnemy(){
        float spawnDelay = 1f; // how long it flashes before spawning the enemy
        float tileFlashSpeed = 4f; //how many times it will flash

        Transform spawnTile = map.getRandomOpenTile();

        if(isCamping){
            spawnTile = map.GetTileFromPosition(playerTransform.position);
        }
        Material tileMaterial = spawnTile.GetComponent<Renderer>().material;
        Color initalColor = tileMaterial.color; // storing original color
        Color flahsColor = Color.red;
        float spawnTimer = 0;
        
        while(spawnTimer < spawnDelay){
            tileMaterial.color = Color.Lerp(initalColor, flahsColor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1)); // ping pong method
            spawnTimer += Time.deltaTime;
            yield return null;
        }

        Transform enemyTransform = Instantiate(enemyPrefab, spawnTile.position + Vector3.up, Quaternion.identity);
        enemyTransform.GetComponent<Enemy>().OnDeath += OnEnemeyDeath; 
    }

    private void OnPlayerDeath()
    {
        isDisabled = true;
    }

    private void OnEnemeyDeath()
    {
        // tracking of alive enemies and calling next wave when no enemy is alive
        enemiesRemainingAlive --;
        if(enemiesRemainingAlive == 0){
            NextWave();
        }
    }

    private void NextWave(){
        //tracking of the wave number and allocating the starting values for that specific wave
        currentWaveNumber ++;
        if(currentWaveNumber - 1 < waves.Length){
            currentWave = waves[currentWaveNumber - 1];
            enemiesRemainingToSpawn = currentWave.enemyCount;
            enemiesRemainingAlive = enemiesRemainingToSpawn;
        }
    }
}

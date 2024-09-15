using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Transform ball;

    [SerializeField] private GameObject startSubLevelPrefab;
    [SerializeField] private GameObject[] subLevelPrefabs;
    private Queue<GameObject> subLevelQueue;

    [SerializeField] private int subLevelsToLoadAtOnce = 10;
    private int subLevelBallIsIn;
    private int currentLevelCenter;

    private void Awake()
    {
        subLevelBallIsIn = 0;
        currentLevelCenter = (int)(subLevelsToLoadAtOnce / 2f);

        subLevelQueue = new Queue<GameObject>();
        //subLevelQueue.Enqueue(Instantiate(startSubLevelPrefab, viewport));

        

        for (int i = 1; i < subLevelsToLoadAtOnce; i++)
        {
            int randomLevelIndex = Random.Range(0, subLevelPrefabs.Length);
            GameObject newSubLevel = Instantiate(subLevelPrefabs[randomLevelIndex]);
            newSubLevel.transform.position = new Vector3(i * 16, 0, 0);

            subLevelQueue.Enqueue(newSubLevel);
        }

        // For starting in Unity editor
        Options.Read();
    }

    private void Update()
    {
        //Check ball's progress and spawn more of the level every time it goes the length of a level
        subLevelBallIsIn = (int)(ball.position.x / 16f);

        while (subLevelBallIsIn >= currentLevelCenter)
        {
            int randomLevelIndex = Random.Range(0, subLevelPrefabs.Length);
            GameObject newSubLevel = Instantiate(subLevelPrefabs[randomLevelIndex]);
            newSubLevel.transform.position = new Vector3((currentLevelCenter + (subLevelsToLoadAtOnce / 2)) * 16, 0, 0);

            subLevelQueue.Enqueue(newSubLevel);
            Destroy(subLevelQueue.Dequeue());

            currentLevelCenter++;
        }
    }
}

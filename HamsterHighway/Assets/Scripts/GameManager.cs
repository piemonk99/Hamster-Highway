using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Transform viewport;
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
        subLevelQueue.Enqueue(Instantiate(startSubLevelPrefab, viewport));

        

        for (int i = 1; i < subLevelsToLoadAtOnce; i++)
        {
            int randomLevelIndex = Random.Range(0, subLevelPrefabs.Length);
            GameObject newSubLevel = Instantiate(subLevelPrefabs[randomLevelIndex], viewport);
            newSubLevel.transform.localPosition = new Vector3(i * 16, 0, 0);

            subLevelQueue.Enqueue(newSubLevel);
        }
    }

    private void Update()
    {
        //Check ball's progress and spawn more of the level every time it goes the length of a level
        subLevelBallIsIn = (int)(ball.localPosition.x / 16f);


        Debug.Log($"{subLevelBallIsIn} > {currentLevelCenter}? {subLevelBallIsIn > currentLevelCenter}");
        while (subLevelBallIsIn > currentLevelCenter)
        {
            int randomLevelIndex = Random.Range(0, subLevelPrefabs.Length);
            GameObject newSubLevel = Instantiate(subLevelPrefabs[randomLevelIndex], viewport);
            newSubLevel.transform.localPosition = new Vector3(currentLevelCenter * (int)(subLevelsToLoadAtOnce / 2f) * 16, 0, 0);

            Debug.Log("Loaded level at x = " + currentLevelCenter * (int)(subLevelsToLoadAtOnce / 2f) * 16);

            subLevelQueue.Enqueue(newSubLevel);

            currentLevelCenter++;
        }
    }
}

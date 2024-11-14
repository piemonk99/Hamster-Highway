using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEnteredTrigger : MonoBehaviour
{
    private GameManager gameManager;
    private Hamster hamster;

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        hamster = gameManager.GetComponent<Hamster>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            hamster.RedirectVelocity();
            gameManager.IncrementSubLevelBallIsIn();
            Destroy(gameObject);
        }
    }
}

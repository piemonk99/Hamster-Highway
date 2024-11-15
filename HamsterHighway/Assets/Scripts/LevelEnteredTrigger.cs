using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEnteredTrigger : MonoBehaviour
{
    private GameManager gameManager;

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gameManager.IncrementSubLevelBallIsIn();
            other.GetComponent<Hamster>().RedirectVelocity();
            Destroy(gameObject);
        }
    }
}

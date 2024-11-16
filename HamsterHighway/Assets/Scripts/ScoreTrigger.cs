using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreTrigger : MonoBehaviour
{
    [SerializeField] private bool isLastTrigger;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ScoreTracker.Instance.score++;
            GameManager.Instance.scoreText.text = ScoreTracker.Instance.score.ToString();
            Destroy(gameObject);

            if (isLastTrigger) { Destroy(transform.parent); }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    GameManager gameManager;
    float distance;

    public void SetValues(GameManager gm, float dis)
    {
        distance = dis * dis;
        gameManager = gm;
    }

    void Update()
    {
        if (transform.position.sqrMagnitude > distance)
            gameManager.DesactivateAsteroid(this);
    }

    public void Desactivate()
    {
        gameManager.DesactivateAsteroid(this);
    }
}

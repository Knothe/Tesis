using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour
{
    public int id;
    public List<GameObject> drops;
    public List<float> probability;
    public float maxLife = 100;
    public Transform dropPos;

    public bool rotateWithFace;

    float currentLife;

    public void Activate()
    {
        currentLife = maxLife;
        if (rotateWithFace)
        {
            RaycastHit hit;
            if (Physics.Raycast(dropPos.position, transform.up * -1, out hit, Mathf.Infinity))
            transform.up = hit.normal;
        }
    }

    public bool DealDamage(float damage)
    {
        currentLife -= damage;
        if (currentLife < 0)
        {
            gameObject.SetActive(false);
            GameObject obj = Instantiate(drops[GetDrop()], transform.parent);
            obj.transform.position = dropPos.position;
            obj.transform.rotation = dropPos.rotation;
            return true;
        }
        return false;
    }

    int GetDrop()
    {
        int lastIndex = probability.Count - 1;
        float v = UnityEngine.Random.Range(0.0f, probability[lastIndex]);
        for(int i = 0; i < lastIndex; i++)
        {
            if (v < probability[i])
                return i;
        }
        return lastIndex;
    }
}

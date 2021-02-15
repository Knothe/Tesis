using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;

public class PlanetsManager : MonoBehaviour
{
    public List<TerrainManager> planets;

    Transform inactiveTree;
    Transform inactiveChunk;
    Transform inactiveTreeHolder;

    Queue<Chunk> inactiveChunkList;
    Dictionary<int, Queue<Tree>> inactiveTreeList;
    Queue<Transform> inactiveTreeHolderList;
    
    private void Start()
    {
        
    }

    public void Initialize()
    {
        if (inactiveTree == null)
            inactiveTree = CreateGameObject("InactiveTree");
        if (inactiveChunk == null)
            inactiveChunk = CreateGameObject("InactiveChunk");
        if (inactiveTreeHolder == null)
            inactiveTreeHolder = CreateGameObject("InactiveTreeHolder");

        if (inactiveChunkList == null)
            inactiveChunkList = new Queue<Chunk>();
        if (inactiveTreeList == null)
            inactiveTreeList = new Dictionary<int, Queue<Tree>>();
        if (inactiveTreeHolderList == null)
            inactiveTreeHolderList = new Queue<Transform>();
    }

    Transform CreateGameObject(string name)
    {
        GameObject g = new GameObject(name);
        g.transform.parent = transform;
        return g.transform;
    }

    public void DesactivateChunk(Node n)
    {
        n.inGameChunk.Desactivate();
        n.inGameChunk.transform.parent = inactiveChunk;
        inactiveChunkList.Enqueue(n.inGameChunk);
        n.inGameChunk = null;
    }

    public Chunk GetChunk()
    {
        Chunk c;
        if(inactiveChunkList.Count > 0)
            c = inactiveChunkList.Dequeue();
        else
        {
            GameObject g = new GameObject("Chunk", typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider), typeof(Chunk));
            c = g.GetComponent<Chunk>();
            g.layer = 8;
        }
        c.gameObject.SetActive(true);
        return c;
    }

    public Tree GetTree(GameObject prefab)
    {
        if (prefab == null)
            return null;
        Tree t = prefab.GetComponent<Tree>();
        Tree r;
        GameObject g;
        if (inactiveTreeList.ContainsKey(t.id))
        {
            if(inactiveTreeList[t.id].Count > 0)
                r = inactiveTreeList[t.id].Dequeue();
            else
                r = Instantiate(prefab).GetComponent<Tree>();
        }
        else
            r = Instantiate(prefab).GetComponent<Tree>();
        r.gameObject.SetActive(true);
        return r;
    }

    public void DesactivateTree(Tree t)
    {
        int id = t.id;
        if (!inactiveTreeList.ContainsKey(id))
        {
            inactiveTreeList.Add(id, new Queue<Tree>());
        }
        inactiveTreeList[id].Enqueue(t);
        t.gameObject.transform.parent = inactiveTree;
        t.gameObject.SetActive(false);
    }

    public Transform GetTreeHolder()
    {
        Transform t;
        if (inactiveTreeHolderList.Count > 0)
            t = inactiveTreeHolderList.Dequeue();
        else
            t = new GameObject("TreeHolder").transform;
        t.gameObject.SetActive(true);
        return t;
    }

    public void DesactivateTreeHolder(Transform th)
    {
        inactiveTreeHolderList.Enqueue(th);
        th.gameObject.SetActive(false);
    }

}

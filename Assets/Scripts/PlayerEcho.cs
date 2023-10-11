using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerEcho : MonoBehaviour
{
    public Player player;
    public float timeBtwSpawns = 0.05f;
    public GameObject echosParent;
    List<GameObject> echos = new List<GameObject>();

    private void Start()
    {
        for(int i = 0; i < echosParent.transform.childCount; i++)
        {
            echos.Add(echosParent.transform.GetChild(i).gameObject);
        }
    }

    public void StartAfterImages()
    {
        StartCoroutine(SetAfterImages());
    }


    IEnumerator SetAfterImages()
    {
        for(int i = 0; i < echos.Count; i++)
        {
            echos[i].transform.position = player.transform.position;
            if (player.playerDirection.x > 0)
            {
                echos[i].transform.localScale = new Vector3(Mathf.Abs(transform.lossyScale.x), transform.lossyScale.y, transform.lossyScale.z);
            }
            else if (player.playerDirection.x < 0)
            {
                echos[i].transform.localScale = new Vector3(-1 * Mathf.Abs(transform.lossyScale.x), transform.lossyScale.y, transform.lossyScale.z);
            }
            echos[i].GetComponent<SpriteRenderer>().color = new Color(255f, 255f, 255f, 0.7f);
            echos[i].SetActive(true);
            echos[i].GetComponent<SpriteRenderer>().DOColor(Color.clear, 0.5f);
            yield return new WaitForSeconds(timeBtwSpawns);
        }
    }

    public void DisableAfterImages()
    {
        StartCoroutine(UnSetAfterImages());
    }

    IEnumerator UnSetAfterImages()
    {
        yield return new WaitForSeconds(1.2f);
        for (int i = 0; i < echos.Count; i++)
        {
            echos[i].SetActive(false);
        }
    }
}

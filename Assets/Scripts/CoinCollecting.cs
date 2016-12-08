using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class CoinCollecting : MonoBehaviour
{

    public Text CoinCounter;
    private int _coinCount;

    void Start()
    {
        _coinCount = 0;
        CoinCounter = GameObject.FindWithTag("CoinCounter").GetComponent<Text>();
    }

    void AddCoin()
    {
        _coinCount++;
        CoinCounter.text = _coinCount.ToString();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Coin"))
        {
            AddCoin();
            AudioSource.PlayClipAtPoint(
                other.gameObject.GetComponent<AudioSource>().clip,
                transform.position
            );
            Destroy(other.gameObject.transform.parent.gameObject.transform.parent.gameObject);
        }
    }

}

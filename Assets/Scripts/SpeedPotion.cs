using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SpeedPotion : ItemEffect
{

    public float speedIncrease;
    public float buffTime;
    public Texture buffIcon;

    public override void UseItem()
    {
        StartCoroutine(SpeedPotionCoroutine());
    }

    IEnumerator SpeedPotionCoroutine()
    {
        caster.m_Character.m_AnimSpeedMultiplier = caster.m_Character.m_AnimSpeedMultiplier * speedIncrease;
        caster.m_Character.m_MoveSpeedMultiplier = caster.m_Character.m_MoveSpeedMultiplier * speedIncrease;
        GameObject buffList = GameObject.Find("BuffList");
        Object buffTimerPrefab = Resources.Load("Prefabs/BuffTimer");
        GameObject buffTimer = (GameObject) Instantiate(buffTimerPrefab, buffList.transform);
        buffTimer.transform.FindChild("BuffIcon").GetComponent<RawImage>().texture = buffIcon;
        Text seconds = buffTimer.transform.FindChild("Seconds").GetComponent<Text>();
        seconds.text = string.Format("{0} s", buffTime);
        for (int i = 0; i < buffTime; i++)
        {
            seconds.text = string.Format("{0} s", buffTime - i);
            yield return new WaitForSeconds(1.0f);
        }
        Destroy(buffTimer);
        caster.m_Character.m_AnimSpeedMultiplier = caster.m_Character.m_AnimSpeedMultiplier / speedIncrease;
        caster.m_Character.m_MoveSpeedMultiplier = caster.m_Character.m_MoveSpeedMultiplier / speedIncrease;
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using TMPro;
using UnityEngine;

public class DamageFloating : MonoBehaviour
{
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private CanvasGroup canvasGroup;

    public void SetDamage(int damage)
    {
        damageText.text = damage.ToString();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 1f;
        StartCoroutine(FloatingText());
    }

    private IEnumerator FloatingText()
    {
        float elapsedTime = 0f;
        float duration = 0.8f;

        while (elapsedTime < duration)
        {
            transform.position += Vector3.up * Time.deltaTime * 0.8f;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        while (elapsedTime > 0f)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);
            elapsedTime -= Time.deltaTime;
            yield return null;
        }

        ObjectPoolManager.Return(gameObject);
    }
}

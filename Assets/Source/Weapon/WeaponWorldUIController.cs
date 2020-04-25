using UnityEngine;
using UnityEngine.UI;

public class WeaponWorldUIController : MonoBehaviour
{
    [SerializeField]Color low = Color.red;
    [SerializeField]Color high = Color.blue;

    [SerializeField]Text[] texts;
    [SerializeField]Image[] images;
    [SerializeField]Light[] lights;

    void Awake()
    {
        if(texts.Length > 0)
        {
            Material tm = texts[0].material;

            //detta kostar minne som fan, byt ut mot [PerRendererData] senare
            for (int i = 0; i < texts.Length; i++)
                texts[i].material = Material.Instantiate(tm);
        }
        
        if(images.Length > 0)
        {
            Material im = images[0].material;

            for (int i = 0; i < images.Length; i++)
                images[i].material = Material.Instantiate(im);
        }
    }
    public void UpdateUI(int shotsLeft, int clipSize)
    {
        Color c = Color.Lerp(low, high, shotsLeft / (float)clipSize);

        for (int i = 0; i < texts.Length; i++)
        {
            texts[i].material.SetColor("_Color", c);
            texts[i].material.SetColor("_Emission", c * 16f);
            texts[i].color = c;
            texts[i].text = shotsLeft == 0 ? "RELOAD" : shotsLeft.ToString();
        }
        for (int i = 0; i < images.Length; i++)
        {
            images[i].material.SetColor("_Color", c);
            images[i].material.SetColor("_Emission", c * 16f);
            images[i].color = c;
            images[i].fillAmount = shotsLeft / (float)clipSize;
        }
        for (int i = 0; i < lights.Length; i++)
            lights[i].color = c;
    }
    public void OnReload()
    {

    }
}

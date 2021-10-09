using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class OscilateAlpha : MonoBehaviour
{

    [ToggleLeft]
    public bool randomOffset = true;

    [Range(-1, 1), HideIf("randomOffset")]
    public float offset = 0;

    [Space]
    public float speed = 1;
    [Range(0, 1)]
    public float alphaRange = .2f;

    float initAlpha;
    float alpha;
    float time = 0;
    float sin = 0;
    Color spriteColor;
    SpriteRenderer sr;
    Image image;
    CanvasGroup cg;

    // Use this for initialization
    void Start ()
    {
        sr = GetComponent<SpriteRenderer>();
        image = GetComponent<Image>();
        cg = GetComponent<CanvasGroup>();

        if (!sr && !image && !cg)
        {
            enabled = false;
            return;
        }

        if (sr)
        {
            spriteColor = sr.color;
            initAlpha = sr.color.a;
        }
        else if (image)
        {
            spriteColor = image.color;
            initAlpha = image.color.a;
        }
        else if (cg)
        {
            initAlpha = cg.alpha;
        }

        if (randomOffset) offset = Random.Range(-1.0f, 1.0f);
    }

    // Update is called once per frame
    void Update ()
    {
        time += Time.deltaTime * speed;
        sin = Mathf.Sin(time + offset * speed);

        alpha = initAlpha + sin * alphaRange;
        alpha = Mathf.Clamp01(alpha);
        spriteColor = new Color(spriteColor.r, spriteColor.g, spriteColor.b, alpha);

        if (sr)
            sr.color = spriteColor;

        if (image)
            image.color = spriteColor;

        if (cg) cg.alpha = alpha;
    }
}

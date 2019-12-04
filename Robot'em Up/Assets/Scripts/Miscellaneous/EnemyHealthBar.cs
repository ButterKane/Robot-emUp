using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private RectTransform FillRect;
    public EnemyBehaviour Enemy;
    private float _initialWidth;
    private Rect _rect;
    private Camera _mainCamera;
    private RectTransform _self;
    private RawImage BarBackground;
    private RawImage BarFill;

    // Start is called before the first frame update
    void Start()
    {
        _self = GetComponent<RectTransform>();
        _mainCamera = Camera.main;
        _rect = FillRect.rect;
        _initialWidth = _rect.width;

        BarBackground = GetComponent<RawImage>();
        BarFill = FillRect.GetComponent<RawImage>();

        ActivateHealthBar(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Enemy.Health < Enemy.MaxHealth)
        {
            ActivateHealthBar(true);

            FillRect.sizeDelta = new Vector2(((float)Enemy.Health / Enemy.MaxHealth) * _initialWidth, _rect.height);

            _self.position = _mainCamera.WorldToScreenPoint(Enemy.HealthBarRef.position);
        }
        if(Enemy.Health <= 0)
        {
            Destroy(this.gameObject);
        }
    }

    void ActivateHealthBar(bool value)
    {
        BarBackground.enabled = value;
        BarFill.enabled = value;
    }
}

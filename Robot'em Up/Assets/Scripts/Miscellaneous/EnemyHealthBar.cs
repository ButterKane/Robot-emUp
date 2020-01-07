using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private RectTransform fillRect;
    public EnemyBehaviour enemy;
    public TurretBehaviour Turret;
    public Gradient healthBarGradient;
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
        if (fillRect == null)
        {
            fillRect = GetComponentInChildren<RectTransform>();
        }
        _rect = fillRect.rect;
        _initialWidth = _rect.width;

        BarBackground = GetComponent<RawImage>();
        BarFill = fillRect.GetComponent<RawImage>();

        ActivateHealthBar(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(enemy != null)
        {
            if (enemy.currentHealth < enemy.maxHealth)
            {
                ActivateHealthBar(true);

                fillRect.sizeDelta = new Vector2(((float)enemy.currentHealth / enemy.maxHealth) * _initialWidth, _rect.height);
                BarFill.color = healthBarGradient.Evaluate(fillRect.sizeDelta.magnitude / _initialWidth);
                _self.position = _mainCamera.WorldToScreenPoint(enemy.healthBarRef.position);
            }
            if (enemy.currentHealth <= 0)
            {
                Destroy(this.gameObject);
            }
        }
        else if(Turret != null)
        {
            if (Turret.currentHealth < Turret.maxHealth)
            {
                ActivateHealthBar(true);

                fillRect.sizeDelta = new Vector2(((float)Turret.currentHealth / Turret.maxHealth) * _initialWidth, _rect.height);
                BarFill.color = healthBarGradient.Evaluate(fillRect.sizeDelta.magnitude / _initialWidth);
                _self.position = _mainCamera.WorldToScreenPoint(Turret.healthBarRef.position);
            }
            if (Turret.currentHealth <= 0)
            {
                Destroy(this.gameObject);
            }
        }
        else
        {
            Destroy(this.gameObject);
        }
            
    }

    void ActivateHealthBar(bool _value)
    {
        BarBackground.enabled = _value;
        BarFill.enabled = _value;
    }
}

using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [SerializeField] private bool _alwaysShow = true;
    [SerializeField] private Color _maxHealthColor;
    [SerializeField] private Color _minHealthColor;
    
    [SerializeField] private GameObject _barObj;
    [SerializeField] private Transform _fillBox;
    [SerializeField] private SpriteRenderer _fillSprite;
    [SerializeField] private UnityEvent _onDeath;

    private int _currentHealth;
    private int _maxHealth;

    public int CurrentHealth => _currentHealth;

    public void Initialize(int health)
    {
        _currentHealth = _maxHealth = health;

        Activate(true);
        Show(false);
        UpdateView();
    }

    public void TakeDamage(int damage)
    {
        Show(true);

        _currentHealth -= damage;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

        if (_currentHealth == 0)
            _onDeath?.Invoke();

        UpdateView();
    }

    private void UpdateView()
    {
        var healthPercentage = (float)_currentHealth / _maxHealth;

        // _fillBox.transform.DOKill();
        // _fillBox.transform.DOScaleX(healthPercentage, 0.2f);
        _fillBox.transform.localScale = Vector3.one * healthPercentage; 

        _fillSprite.color = Color.Lerp(_minHealthColor, _maxHealthColor, healthPercentage);
    }

    public void Activate(bool s) => gameObject.SetActive(s);
    private void Show(bool s) => _barObj.SetActive(_alwaysShow || s);

    private void Update()
    {
        transform.LookAt(Camera.main!.transform);
    }
}
using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [SerializeField] private Transform _fillBox;
    [SerializeField] private SpriteRenderer _fillSprite;

    [SerializeField] private int _currentHealth;
    [SerializeField] private int _maxHealth;
    
    [SerializeField] private UnityEvent _onDeath;

    public void Initialize(int health)
    {
        _currentHealth = _maxHealth = health;
        
        Activate(true);

        UpdateView();
    }

    public void TakeDamage(int damage)
    {
        Debug.Log($"{gameObject.name} takes {damage} damage!");
        
        _currentHealth -= damage;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

        _onDeath?.Invoke();

        UpdateView();
    }

    private void UpdateView()
    {
        var healthPercentage = (float)_currentHealth / _maxHealth;

        _fillBox.transform.DOKill();
        _fillBox.transform.DOScaleX(healthPercentage, 0.2f);

        _fillSprite.color = Color.Lerp(Color.red, Color.green, healthPercentage);
    }

    public void Activate(bool s) => gameObject.SetActive(s); 

    private void Update()
    {
        transform.LookAt(Camera.main!.transform);
    }
}
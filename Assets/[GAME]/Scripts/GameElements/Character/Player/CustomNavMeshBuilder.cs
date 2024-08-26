using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class CustomNavMeshBuilder : MonoBehaviour
{
    [SerializeField]
    private NavMeshSurface _surface;
    [SerializeField]
    private float _updateRate = 0.1f;
    [SerializeField]
    private float _distanceThreshold = 3f;
    [SerializeField]
    private Vector3 _navMeshBuildSize = new Vector3(20, 1, 20);
    
    private PlayerSystem _playerSystem;

    private NavMeshData _navMeshData;
    private List<NavMeshBuildSource> _sources;
    private List<NavMeshBuildMarkup> _markups;

    private Vector3 _lastBakeCenter;
    private float _distanceThresholdSqr;
    private CancellationTokenSource _cts;

    private void Awake()
    {
        _playerSystem = ServiceLocator.Resolve<PlayerSystem>();
        _distanceThresholdSqr = _distanceThreshold * _distanceThreshold;

        _sources = new();
        _markups = new();
        
        _navMeshData = new NavMeshData();
        NavMesh.AddNavMeshData(_navMeshData);
    }

    private void OnEnable()
    {
        Events.GameStates.OnLevelStarted += Activate;
        Events.GameStates.OnLevelEnd += Deactivate;
    }
    
    private void OnDisable()
    {
        Events.GameStates.OnLevelStarted -= Activate;
        Events.GameStates.OnLevelEnd -= Deactivate;
    }

    private void Activate()
    {
        _cts = new CancellationTokenSource();
        BuilderLoop().Forget();
    }
    private void Deactivate(bool s) => _cts.Cancel();

    private async UniTask BuilderLoop()
    {
        while (!_cts.IsCancellationRequested)
        {
            if ((_playerSystem.Position - _lastBakeCenter).sqrMagnitude >= _distanceThresholdSqr)
            {
                BuildNavMesh();
                _lastBakeCenter = _playerSystem.Position;
            }
            
            await UniTask.WaitForSeconds(_updateRate, cancellationToken: _cts.Token);
        }
    }

    private void BuildNavMesh()
    {
        var bounds = new Bounds(_playerSystem.Position, _navMeshBuildSize);
        
        NavMeshBuilder.CollectSources(transform, _surface.layerMask, _surface.useGeometry, _surface.defaultArea, _markups, _sources);
        
        NavMeshBuilder.UpdateNavMeshDataAsync(_navMeshData, _surface.GetBuildSettings(), _sources, bounds);
    }
}

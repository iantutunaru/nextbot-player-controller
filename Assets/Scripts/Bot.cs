using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Bot : MonoBehaviour
{
    private static bool GetRandomNavMeshPoint(Vector3 center, float radius, out Vector3 result,
        int areaMask = NavMesh.AllAreas, int tries = 30)
    {
        for (var i = 0; i < tries; i++)
        {
            var randomPoint = center + Random.insideUnitSphere * radius;

            if (!NavMesh.SamplePosition(randomPoint, out var hit, 1.0f, areaMask)) continue;
            
            result = hit.position;
            
            return true;
        }
        
        result = Vector3.zero;
        
        return false;
    }
    
    public int targetDetectionRadius = 10;
    public LayerMask targetDetectionLayerMask;
    public int maxDetectableTargets = 10;
    public float roamFindDestRadius = 10;
    public int roamFindDestAreaMask = NavMesh.AllAreas;
    public int roamFindDestTries = 30;
    
    private GameObject _target;
    private NavMeshAgent _agent;
    
    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        Do();
    }

    private void Do()
    {
        if (!IsTargetValid())
        {
            var targets = GetTargets();
            _target = GetTarget(targets);
        }

        if (!IsTargetValid())
        {
            Roam();

            return;
        }
        
        Chase();
    }

    private void Roam()
    {
        if (HasDestination())
        {
            if (IsMoving()) return;
            
            _agent.ResetPath();
            
            return;
        }
        
        var roamDestination = GetRandomRoamDestination();

        if (roamDestination.Equals(Vector3.negativeInfinity))
        {
            return;
        }

        _agent.SetDestination(roamDestination);
        transform.LookAt(roamDestination);
    }

    private void Chase()
    {
        _agent.ResetPath();
        var targetTransform = _target.transform;

        _agent.SetDestination(targetTransform.position);
        transform.LookAt(targetTransform);
    }
    
    private bool HasDestination()
    {
        return (_agent.pathPending || _agent.hasPath) && !_agent.destination.Equals(transform.position) && !_agent.destination.Equals(Vector3.positiveInfinity);
    }

    private bool IsMoving()
    {
        var pathPending = _agent.pathPending;
        var remainingDistance = _agent.remainingDistance;
        var stoppingDistance = _agent.stoppingDistance;
        
        return pathPending || remainingDistance > stoppingDistance || (_agent.hasPath && _agent.velocity.sqrMagnitude > 0);
    }

    private Vector3 GetRandomRoamDestination()
    {
        var pointFound = GetRandomNavMeshPoint(transform.position, roamFindDestRadius,
            out var randomDestination, roamFindDestAreaMask, roamFindDestTries);

        return !pointFound ? Vector3.negativeInfinity : randomDestination;
    }
    
    private GameObject GetTarget(GameObject[] targets)
    {
        var activeTargets = (from target in targets where target.activeInHierarchy select target).ToList();
    
        if (activeTargets.Count <= 0)
        {
            return null;
        }
        
        var randomPlayerIndex = Random.Range(0, activeTargets.Count);
    
        return activeTargets[randomPlayerIndex];
    }
    
    private GameObject[] GetTargets()
    {
        var hitColliders = new Collider[maxDetectableTargets];
        var size = Physics.OverlapSphereNonAlloc(transform.position, targetDetectionRadius, hitColliders, 
            targetDetectionLayerMask);
        var targets = new GameObject[size];

        for (var c = 0; c < size; c++)
        {
            var hitCollider = hitColliders[c];
            targets[c] = hitCollider.gameObject;
        }

        return targets;
    }
    
    private bool IsTargetValid()
    {
        return _target && _target.activeInHierarchy && Vector3.Distance(transform.position, _target.transform.position) <= targetDetectionRadius;
    }
}

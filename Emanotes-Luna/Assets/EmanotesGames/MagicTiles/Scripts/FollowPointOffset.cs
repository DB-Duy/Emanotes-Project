using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPointOffset : MonoBehaviour
{
    [SerializeField]
    private Transform _point;
    private Vector3 offset;
    private void Start()
    {
        offset = transform.position - _point.position;
    }
    void Update()
    {
        transform.position = _point.position + offset;
    }
}

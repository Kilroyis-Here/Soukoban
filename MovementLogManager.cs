using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementLogManager : MonoBehaviour
{

    [SerializeField] MapManager MM;
    public List<Vector2Int> BlockPositions = new List<Vector2Int>();
        private Vector2Int BPos = default;
}

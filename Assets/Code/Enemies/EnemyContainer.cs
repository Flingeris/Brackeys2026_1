using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyContainer : MonoBehaviour
{
    public List<EnemyInstance> enemies;

    private void OnValidate()
    {
        if (enemies == null || enemies.Count == 0 || enemies.Count != transform.childCount)
        {
            enemies = GetComponentsInChildren<EnemyInstance>().ToList();
        }
    }
}
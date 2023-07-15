using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagebale   //using Interface for any Living Object that have health
{
    void TakeDamage(float damage, RaycastHit raycastHit);

    void PlayerTakeDamage(float damage);
}

using Photon.Pun;
using UnityEngine;

public class PlayerAttackGenerater : MonoBehaviour
{
    [SerializeField] float bulletLifeTime = 3f;
    Vector2 playerDirection;
internal void Fire(Vector2 direction, Vector2 playerDir)
    {
        if (direction == Vector2.zero)
        {
            direction = playerDir;
        }

        Vector2 spawnPosition = new Vector2(transform.position.x + (direction.x * 1.1f), transform.position.y + (direction.y * 1.1f));
        //GameObject bullet=  PhotonNetwork.Instantiate("Bullet", transform.position, Quaternion.identity);
       PhotonNetwork.Instantiate("Bullet", spawnPosition, Quaternion.identity,0,new object[] { direction});

    }

     }

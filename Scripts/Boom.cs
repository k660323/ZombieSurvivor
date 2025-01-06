using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boom : MonoBehaviour
{
    public GameObject Particle;
    public MeshRenderer mesh;
    public Rigidbody rigid;
    public float damage;

    [SerializeField]
    AudioClip audioClip;

    public bool isEnemy;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(BoomGrenade());
    }

    IEnumerator BoomGrenade()
    {
        yield return new WaitForSeconds(3.0f);
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        rigid.isKinematic = false;
        mesh.enabled = false;
        SoundManager.instance.SFXPlay(audioClip,"Boom");
        Particle.SetActive(true);

        if (!isEnemy)
        {
            RaycastHit[] ray = Physics.SphereCastAll(transform.position, 5f, Vector3.up, 0f, LayerMask.GetMask("Enemy"));
            foreach(RaycastHit hit in ray)
            {          
                hit.transform.GetComponent<ZombieController>().HitGrenade(transform.position, damage);
            }
        }
        else
        {
            RaycastHit[] ray = Physics.SphereCastAll(transform.position, 5f, Vector3.up, 0f, LayerMask.GetMask("Player"));
            foreach (RaycastHit hit in ray)
            {
                hit.transform.GetComponent<PlayerController>().HitGrenade(transform.position, damage);
            }
        }
        Destroy(gameObject, 5.0f);
    }
}

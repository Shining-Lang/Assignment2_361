using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public Animator animator;
    public int HP;
    public PlayerController player;
    public NavMeshAgent agent;
    public float cd;
    private float timer;
    public int attackValue;

    public AudioSource audioSource;
    public AudioClip hitAudio;
    public AudioClip attackAudio;
    public AudioClip deathAudio;

    private bool isDead;
    
    public GameObject prefabSkeleton;
    public float spawnInterval;
    public Vector3 spawnAreaMin;
    public Vector3 spawnAreaMax;


    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("SpawnPrefab", spawnInterval, spawnInterval);
    }

    // Update is called once per frame
    void Update()
    {
        if(isDead){
            return;
        }

        if(Vector3.Distance(transform.position,player.transform.position) <= 1){
            Attack();
        } else {
            agent.isStopped = false;
         agent.SetDestination(player.transform.position); 
        }
    }
    
    public void TakeDamage(int attackValue){
        animator.SetTrigger("Hit");
        PlaySound(hitAudio);
        HP -= attackValue;
        if(HP <= 0){
            isDead = true;
            agent.isStopped = true;
            animator.SetBool("Death",true);
            PlaySound(deathAudio);
            // gameObject.SetActive(false);
        }
    }

    public void PlaySound(AudioClip ac){
        audioSource.PlayOneShot(ac);
    }

    private void Attack(){
        agent.isStopped = true;
            if(Time.time - timer >= cd){
                animator.SetTrigger("Attack");
                PlaySound(attackAudio);
                timer = Time.time;
                player.TakeDamage(attackValue);
            }
    }

    public void SpawnPrefab()
    {
        Vector3 randomPosition = new Vector3(
            Random.Range(spawnAreaMin.x, spawnAreaMax.x),
            Random.Range(spawnAreaMin.y, spawnAreaMax.y),
            Random.Range(spawnAreaMin.z, spawnAreaMax.z)
        );
        Instantiate(prefabSkeleton, randomPosition, Quaternion.identity);
    }
}

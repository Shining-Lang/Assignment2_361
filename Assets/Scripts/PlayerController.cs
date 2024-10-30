using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public enum GUNTYPE{
    SINGLESHOT,
    AUTO,
    SNIPING
}
    public float moveSpeed;
    public float rotateSpeed;
    private float angleY;
    private float angleX;
    public float jumpForce;
    public float cd;
    public float timer;
    public Animator animator;
    public Rigidbody rb;
    public Transform gunPoint;
    public GameObject bloodEffect;
    public GameObject otherEffect;
    public GUNTYPE gunType;

    private Dictionary<GUNTYPE,int> bullets_bag = new Dictionary<GUNTYPE,int>();
    private Dictionary<GUNTYPE,int> bullets_clip = new Dictionary<GUNTYPE,int>();
    private Dictionary<GUNTYPE,int> Gun_hurt = new Dictionary<GUNTYPE,int>();


    public int maxSingleShotBullets;
    public int maxAutoShotBullets;
    public int maxSnipingShotBullets;

    private bool isReloading;

    public GameObject[] gunGameObject;

    public GameObject extension;
    public int HP;

    public AudioSource audioSource;
    public AudioClip singleShotAudio;
    public AudioClip autoShotAudio;
    public AudioClip snipingShotAudio;
    public AudioClip reloadAudio;
    public AudioClip otherAudio;
    public AudioClip jumpAudio;

    public bool mJump;

    public Text playerHPText;
    public GameObject[] gunUI;

    public Text TextBullet;
    public GameObject bloodUI;
    public GameObject scopeUI;

    public GameObject gameOverPanel;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        bullets_bag.Add(GUNTYPE.SINGLESHOT,100);
        bullets_bag.Add(GUNTYPE.AUTO,300);
        bullets_bag.Add(GUNTYPE.SNIPING,6);
        bullets_clip.Add(GUNTYPE.SINGLESHOT,maxSingleShotBullets);
        bullets_clip.Add(GUNTYPE.AUTO,maxAutoShotBullets);
        bullets_clip.Add(GUNTYPE.SNIPING,maxSnipingShotBullets);
        Gun_hurt.Add(GUNTYPE.SINGLESHOT,10);
        Gun_hurt.Add(GUNTYPE.AUTO,5);
        Gun_hurt.Add(GUNTYPE.SNIPING,50);
        ChangeGunGameObject(0);
        mJump = true;

    }

    // Update is called once per frame
    void Update()
    {
        PlayerMove();
        LookAround();
        Attack();
        Jump();
        ChangeGun();
        if(Input.GetKeyDown(KeyCode.R)){
            Reload();
        }
        telescope();
    }

    void OnCollisionEnter(Collision collision) {
    if (collision.gameObject.CompareTag("Ground")) {
        mJump = true;
    }
}

    private void PlayerMove(){
        // vertical move
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 movementV = transform.forward * verticalInput * moveSpeed * Time.deltaTime;
        transform.position += movementV;

        // horizontal move
        float horizontalInput = Input.GetAxis("Horizontal");
        Vector3 movementH = transform.right * horizontalInput * moveSpeed * Time.deltaTime;
        transform.position += movementH;

        animator.SetFloat("MoveX", horizontalInput);
        animator.SetFloat("MoveY", verticalInput);

    }

    private void LookAround(){
        // look left and right
        float mouseX = Input.GetAxis("Mouse X");
        float lookHAngleY = mouseX * rotateSpeed;
        angleY = angleY + lookHAngleY;

        //look up and down
        float mouseY = -Input.GetAxis("Mouse Y");
        float lookVAngleX = mouseY * rotateSpeed;
        angleX = Mathf.Clamp(angleX + lookVAngleX,-30,30);
        
        transform.eulerAngles = new Vector3(angleX,angleY,0);

    }

    private void Attack(){
        if(!isReloading){
            switch(gunType){
            case GUNTYPE.SINGLESHOT:
            SingleShotAttack();
            break;
            case GUNTYPE.AUTO:
            AutoAttack();
            break;
            case GUNTYPE.SNIPING:
            SnipingAttack();
            break;
            default:
            break;  
    }
        }
    }

    private void SingleShotAttack(){
        if(Input.GetMouseButtonDown(0) && Time.time - timer >= cd){
            if(bullets_clip[gunType] > 0){
                PlaySound(singleShotAudio);
             bullets_clip[gunType]--;
             TextBullet.text = "X" + bullets_clip[gunType].ToString();
            animator.SetTrigger("SingleAttack");
            Invoke("GunAttack",0.25f);
        } else {
            Reload();
        }
        }
    }


    private void AutoAttack(){
        if(Input.GetMouseButton(0) && Time.time - timer >= cd){
            if(bullets_clip[gunType] > 0){
                PlaySound(autoShotAudio);
             bullets_clip[gunType]--;
             TextBullet.text = "X" + bullets_clip[gunType].ToString();
           animator.SetBool("AutoAttack",true);
           GunAttack();
        } else {
            Reload();
        }
            
        } else if(Input.GetMouseButtonUp(0))
        {
            animator.SetBool("AutoAttack",false);
        }
}

    private void SnipingAttack(){
        if(Input.GetMouseButtonDown(0) && Time.time - timer >= cd){
            if(bullets_clip[gunType] > 0){
                PlaySound(snipingShotAudio);
             bullets_clip[gunType]--;
             TextBullet.text = "X" + bullets_clip[gunType].ToString();
            animator.SetTrigger("SnipingAttack");
           Invoke("GunAttack",0.25f);
        } else {
            Reload();
        }   
        }
    }

    private void Jump(){
        if(Input.GetKeyDown(KeyCode.Space) && mJump){
            rb.AddForce(jumpForce * Vector3.up);
            PlaySound(jumpAudio);
            mJump = false;
        }
    }

    private void GunAttack(){
         RaycastHit hit;
            timer = Time.time;
            if(Physics.Raycast(gunPoint.position,gunPoint.forward,out hit,4)){
                if(hit.collider.CompareTag("Enemy")){
                    Instantiate(bloodEffect,hit.point,Quaternion.identity);
                    hit.transform.GetComponent<Enemy>().TakeDamage(Gun_hurt[gunType]);
                } else {
                    PlaySound(otherAudio);
                    Instantiate(otherEffect,hit.point,Quaternion.identity);
                }
            }
    }

    private void ChangeGun(){
        if(Input.GetKeyDown(KeyCode.C)){
            gunType++;
            if(gunType > GUNTYPE.SNIPING){
                gunType = 0;
            }
            switch(gunType){
            case GUNTYPE.SINGLESHOT:
            ChangeGunGameObject(0);
            cd = 0.5f;
            break;
            case GUNTYPE.AUTO:
            ChangeGunGameObject(1);
            cd = 0.1f;
            break;
            case GUNTYPE.SNIPING:
            ChangeGunGameObject(2);
            cd = 1f;
            break;
        }
        }
    }

    private void ChangeGunGameObject(int number){
        for(int i = 0; i < gunGameObject.Length; i++){
            gunGameObject[i].SetActive(false);
            gunUI[i].SetActive(false);
        }
        gunGameObject[number].SetActive(true);
        gunUI[number].SetActive(true);
        TextBullet.text = "X" + bullets_clip[gunType].ToString();
    }

    private void Reload(){
        if(bullets_bag[gunType] > 0){
            PlaySound(reloadAudio);
            isReloading = true;
            Invoke("Recover",2.667f);
            animator.SetTrigger("Reload");
            switch(gunType){
            case GUNTYPE.SINGLESHOT:
            if(bullets_bag[gunType] >= maxSingleShotBullets){
                if(bullets_clip[gunType] > 0){
                    int addBullet = maxSingleShotBullets - bullets_clip[gunType];
                    bullets_bag[gunType] -= addBullet;
                    bullets_clip[gunType] += addBullet;
                } else {
                    bullets_bag[gunType] -= maxSingleShotBullets;
                    bullets_clip[gunType] += maxSingleShotBullets; 
                }
                
            } else {
                bullets_clip[gunType] += bullets_bag[gunType];
                bullets_bag[gunType] = 0;
            }
            TextBullet.text = "X" + bullets_clip[gunType].ToString();
            break;
            case GUNTYPE.AUTO:
            if(bullets_bag[gunType] >= maxAutoShotBullets){
                if(bullets_clip[gunType] > 0){
                    int addBullet = maxAutoShotBullets - bullets_clip[gunType];
                    bullets_bag[gunType] -= addBullet;
                    bullets_clip[gunType] += addBullet;
                } else {
                    bullets_bag[gunType] -= maxAutoShotBullets;
                    bullets_clip[gunType] += maxAutoShotBullets;
                }
                
            } else {
                bullets_clip[gunType] += bullets_bag[gunType];
                bullets_bag[gunType] = 0;
            }
            TextBullet.text = "X" + bullets_clip[gunType].ToString();
            break;
            case GUNTYPE.SNIPING:
            if(bullets_bag[gunType] >= maxSnipingShotBullets){
                if(bullets_clip[gunType] > 0){
                    int addBullet = maxSnipingShotBullets - bullets_clip[gunType];
                    bullets_bag[gunType] -= addBullet;
                    bullets_clip[gunType] += addBullet;
                } else {
                    bullets_bag[gunType] -= maxSnipingShotBullets;
                    bullets_clip[gunType] += maxSnipingShotBullets;
                }
                
            } else {
                bullets_clip[gunType] += bullets_bag[gunType];
                bullets_bag[gunType] = 0;
            }
            TextBullet.text = "X" + bullets_clip[gunType].ToString();
            break;
            default:
            break;
            }
        } else {
            animator.SetBool("AutoAttack",false);
        }
    }

    private void Recover(){
        isReloading = false;
    }

    private void telescope(){
        if(Input.GetMouseButton(1) && gunType == GUNTYPE.SNIPING){
            extension.SetActive(true);
            scopeUI.SetActive(true);
        } else {
            extension.SetActive(false);
            scopeUI.SetActive(false);
        }
    }

    public void TakeDamage(int value){
        bloodUI.SetActive(true);
        HP -= value;
        if(HP <= 0){
            HP = 0;
            gameOverPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 0;
        }
        playerHPText.text = HP.ToString();
        Invoke("HideBloodUI",0.5f);
        
    }
    
    public void PlaySound(AudioClip ac){
        audioSource.PlayOneShot(ac);
    }

    private void HideBloodUI(){
        bloodUI.SetActive(false);
    }

    public void Replay(){
        SceneManager.LoadScene(0);
    }
}


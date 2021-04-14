﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour {
	public int playerSpeed = 10;
	public int jumpForce = 1250;
	public float downRaySize = 0.8f;
	public Transform swordTransform;
    public GameObject ledgeTrigger;
	public GameObject maincollider;

	Rigidbody2D m_playerRb;
	SpriteRenderer m_playerSpriteRenderer;
	Animator m_animator;
	GameManager gameManagerScript;
	InputController m_input;

	float m_moveX;
	Vector2 prevPosition;
	[SerializeField]
	int MAX_HEALTH = 100;
	float currentHealth;
    BoxCollider2D boxCollider;
 

    // Use this for initialization
    void Awake () {
		m_input = GetComponent <InputController> ();
		m_playerRb = GetComponent <Rigidbody2D> ();
		m_playerSpriteRenderer = GetComponent <SpriteRenderer> ();
		m_animator = GetComponent <Animator> ();
        boxCollider = GetComponent<BoxCollider2D>();
		gameManagerScript = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager> ();
		prevPosition = transform.position;
		currentHealth = MAX_HEALTH;

	}

	void Update () {
		m_moveX = m_input.m_horizontal;
		CheckIfAirborne ();
		CheckIfFalling ();
		ResetIfDead ();
		prevPosition = transform.position;
	}

    // Update is called once per frame
    void FixedUpdate () {
		MovePlayer ();
		PlayerRaycast ();

        if (!CheckIfGrabCorner())
        {
            ModifyGravity();
        }
    }

	void ModifyGravity () {
		if (m_input.isFalling) {
			m_playerRb.gravityScale = 2.5f;
		}

		if (m_input.isOnGround) {
			m_playerRb.gravityScale = 4f;
		}
	}

    public bool CheckIfGrabCorner()
    {
        return m_input.grabCorner ||
               m_animator.GetCurrentAnimatorStateInfo(0).IsName("CornerGrab");
    }

	void MovePlayer () {
        if (!CheckIfGrabCorner())
        {
		    if (m_input.m_crouchPressed) {
			    if (m_input.isOnGround) {
				    m_playerRb.velocity = Vector2.zero;
			    }
		    } else {
			    if (m_input.m_jumpPressed && m_input.isOnGround) {
				    Jump();
			    }

			    bool attack1Active = m_animator.GetCurrentAnimatorStateInfo(0).IsName("Attack State.Attack1");
			    bool attack2Active = m_animator.GetCurrentAnimatorStateInfo(0).IsName("Attack State.Attack2");

			    if (!(attack1Active || attack2Active)) {
				    m_playerRb.velocity = new Vector2(m_moveX * playerSpeed, m_playerRb.velocity.y);
			    } else {
				    m_playerRb.velocity = Vector2.zero;
			    }

		    }

			Vector2 tempScale;
		    // flip sprite based on direction facing
		    if (m_moveX < 0.0f) {
				tempScale = new Vector2 (-1, 1);
			    m_playerSpriteRenderer.flipX = true;
			    swordTransform.localScale = tempScale;
                ledgeTrigger.transform.localScale = tempScale;
				maincollider.transform.localScale = tempScale;
            } else if (m_moveX > 0.0f) {
				tempScale = new Vector2 (1, 1);
			    swordTransform.localScale = tempScale;
                ledgeTrigger.transform.localScale = tempScale;
				maincollider.transform.localScale = tempScale;
                m_playerSpriteRenderer.flipX = false;
		    }
        }

	}

	void Jump () {
		m_input.m_jumpPressed = false;
		JoyInputController.m_jump = false;

		if (m_input.isOnGround) {
			transform.parent = null;
			m_playerRb.velocity = Vector2.zero;
			m_playerRb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
		}

		SetGroundStatus (false);
	}

	void CheckIfFalling () {
		if (!m_input.isOnGround) {
			if (transform.position.y < prevPosition.y) {
				m_input.isFalling = true;
			}
		} else {
			m_input.isFalling = false;
		}
	}

	void CheckIfAirborne () {
		if (!m_input.isOnGround) {
			if (transform.position.y > prevPosition.y) {
				m_input.isInFlight = true;
			}
		} else {
			m_input.isInFlight = false;
		}
	}

    void ResetIfDead()
    {
        if (this.transform.position.y < -7)
        {
			PlayerPrefs.SetInt("MaxCoins", gameManagerScript.maxCoin += gameManagerScript.coin);
			PlayerPrefs.DeleteKey("Coins");
			gameManagerScript.coin = 0;
			gameManagerScript.DeadMenu.SetActive(true);
			Time.timeScale = 1;
		}
    }
	public void buyLivesRemainx1()
	{
		if (gameManagerScript.maxCoin >= 70)
		{
			gameManagerScript.livesRemain += 1;
			gameManagerScript.maxCoin -= 70;
			Time.timeScale = 1;
			gameManagerScript.DeadMenu.SetActive(false);
			gameManagerScript.goNote.SetActive(false);
		}
		else
		{
			gameManagerScript.goNote.SetActive(true);
			gameManagerScript.Note.text = "You don't have enough money";
		}
	}

	public void buyLivesRemainx2()
	{
		if (gameManagerScript.maxCoin >= 150)
		{
			gameManagerScript.livesRemain += 2;
			gameManagerScript.maxCoin -= 150;
			Time.timeScale = 1;
			gameManagerScript.DeadMenu.SetActive(false);
			gameManagerScript.goNote.SetActive(false);
		}
		else
		{
			gameManagerScript.goNote.SetActive(true);
			gameManagerScript.Note.text = "You don't have enough money";
		}
	}


	void PlayerRaycast()
	{
		RaycastHit2D downRayLeft = Physics2D.Raycast (this.transform.position + new Vector3(-0.35f, 0), Vector2.down, downRaySize);
		RaycastHit2D downRayRight = Physics2D.Raycast (this.transform.position + new Vector3(0.35f, 0), Vector2.down, downRaySize);
		RaycastHit2D downRay = Physics2D.Raycast (this.transform.position, Vector2.down, downRaySize);

		if (downRayRight.collider != null || downRayLeft.collider != null || downRay.collider != null) {
			bool leftCollider = downRayLeft.collider != null && downRayLeft.collider.tag == "Ground&Obstacles";
			bool rightCollider = downRayRight.collider !=null && downRayRight.collider.tag == "Ground&Obstacles";
			bool centerCollider = downRay.collider !=null && downRay.collider.tag == "Ground&Obstacles";

			if (leftCollider || rightCollider || centerCollider) {
				SetGroundStatus (true);
			}
		}
		else {
			SetGroundStatus (false);
		}
	}

	void SetGroundStatus (bool m_status) {
		m_input.isOnGround = m_status;
	}

	void DamagePlayer () {
		currentHealth -= 15f;
		float healthRatio = currentHealth / MAX_HEALTH;
        m_input.isHurt = true;

		gameManagerScript.SetPlayerHealth(healthRatio);

		if (currentHealth <= 0) {
			currentHealth = MAX_HEALTH;
		}
	}

	private void OnTriggerEnter2D (Collider2D other) {
		if (other.gameObject.tag == "EnemyWeaponTrigger" || other.gameObject.tag == "FireBall") {
			DamagePlayer ();
		}


        if (other.CompareTag("Coin"))
        {
			gameManagerScript.coin += 1;
            Destroy(other.gameObject);
        }

        if (other.CompareTag("HP"))
        {
			currentHealth += 30f;
			float healthRatio = currentHealth / MAX_HEALTH;
			m_input.isHurt = true;

			gameManagerScript.SetPlayerHealth(healthRatio);
			Destroy(other.gameObject);
        }
		if (other.CompareTag("Shoes"))
		{
			Destroy(other.gameObject);
			playerSpeed = 20;
			StartCoroutine(timecount(5));
		}
	}
	IEnumerator timecount(int time)
	{
		yield return new WaitForSeconds(time);
		playerSpeed = 10;
	}

	public void SetOnGrabStay ()
    {
        if (!m_input.isOnGround && m_input.jumpGrabCornerPressed)
        {
            if (!m_animator.GetCurrentAnimatorStateInfo(0).IsName("CornerClimb"))
            {
                m_input.grabCorner = true;
                m_playerRb.gravityScale = 0;
                m_playerRb.velocity = Vector3.zero;
            }
        }
    }

    // Animation Event: On the first keyframe of CornerClimb
    public void ClimbWall()
    {
        m_playerRb.gravityScale = 4;
        m_playerRb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }
}

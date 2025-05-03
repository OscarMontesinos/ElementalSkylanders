using CodeMonkey.Utils;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;
using Random = UnityEngine.Random;

public class PjBase : MonoBehaviour, TakeDamage
{
    public bool walk;
    [HideInInspector]
    public Rigidbody2D rb;
    public UIManager UIManager;
    [HideInInspector]
    public GameManager manager;
    [HideInInspector]
    public PlayerController controller;
    [HideInInspector]
    public Animator animator;
    public GameObject sprite;
    public string chName;
    public int team;
    [HideInInspector]
    public bool lockPointer;
    [HideInInspector]
    public bool lookAtPointer;
    public GameObject pointer;
    public GameObject cursor;
    public HitData.Element element;
    public GameObject spinObjects;
    public Slider hpBar;
    public Slider stunnBar;
    public Slider shieldBar;
    public TextMeshProUGUI hpBarText;

    public Sprite hab1Image;
    public Sprite hab2Image;
    public float maxRange;
    public float hab1Cd;
    public float hab2Cd;
    [HideInInspector]
    public float currentBasicCd;
    [HideInInspector]
    public float comboReset;
    [HideInInspector]
    public int basicComboCount;
    [HideInInspector]
    public float currentHab1Cd;
    [HideInInspector]
    public float currentHab2Cd;
    [HideInInspector]
    public bool casting;
    [HideInInspector]
    public bool dashing;
    [HideInInspector]
    public float stunTime;
    [HideInInspector]
    public bool ignoreSoftCastDebuff;
    public Stats stats;
    public Upgrades upgrades;
    public float damageTextOffset;
    [HideInInspector]
    public float dmgDealed;
    public bool invisible;
    public bool invulnerable;
    public GameObject visuals;
    float healCount;
    float dmgCount;
    public enum AttackType
    {
        Physical, Magical, None
    }

    [ContextMenu("CreateReferences")]
    public void CreateReferences()
    {
        walk = true;
        UIManager = transform.GetChild(1).gameObject.GetComponent<UIManager>();
        sprite = transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
        pointer = transform.GetChild(0).GetChild(0).gameObject;
        cursor = transform.GetChild(3).gameObject;
        hpBar = transform.GetChild(0).GetChild(1).GetChild(0).gameObject.GetComponent<Slider>();
        stunnBar = transform.GetChild(0).GetChild(1).GetChild(1).gameObject.GetComponent<Slider>();
        shieldBar = transform.GetChild(0).GetChild(1).GetChild(2).gameObject.GetComponent<Slider>();
        hpBarText = transform.GetChild(0).GetChild(1).GetChild(3).gameObject.GetComponent<TextMeshProUGUI>();
        visuals = transform.GetChild(0).gameObject;
    }

    public virtual void Awake()
    {
        controller = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        stats.mHp = (int)(Mathf.Lerp(stats.mHp * 0.35f, stats.mHp * 6, stats.lvl * (0.01f / 3)) * 3);
        stats.strength = (int)Mathf.Lerp(stats.strength * 0.35f, stats.strength * 6, stats.lvl * (0.01f / 3));
        stats.sinergy = (int)Mathf.Lerp(stats.sinergy * 0.35f, stats.sinergy * 6, stats.lvl * (0.01f / 3));
        stats.resist = (int)Mathf.Lerp(stats.resist * 0.35f, stats.resist * 6, stats.lvl * (0.01f / 3));

        if (team != 0)
        {
            hpBar.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = Color.red;
        }
    }
    public virtual void Start()
    {
        stats.hp = stats.mHp;

        GameManager.Instance.pjList.Add(this);
        if (hpBar != null)
        {
            shieldBar = hpBar.transform.parent.GetChild(2).GetComponent<Slider>();
        }
    }
    public virtual void Update()
    {
        if(comboReset > 0)
        {
            comboReset -= Time.deltaTime;
            if(comboReset <= 0)
            {
                basicComboCount = 0;
            }
        }

        if (walk)
        {
            animator.SetFloat("FrontVelocity", rb.velocity.magnitude);
        }

        if(stats.hp < stats.mHp)
        {
            stats.hp += stats.healthRegen * Time.deltaTime;
            if(stats.hp > stats.mHp)
            {
                stats.hp = stats.mHp;
            }
        }
        if (hpBar != null)
        {
            hpBar.maxValue = stats.mHp;
            hpBar.value = stats.hp;

            shieldBar.value = stats.shield;
            shieldBar.maxValue = stats.mHp;

            hpBarText.text = stats.hp.ToString("F0");
        }

        if (stunTime > 0)
        {
            stunTime -= Time.deltaTime;
            if (stunnBar != null)
            {
                if (stunnBar.maxValue < stunTime)
                {
                    stunnBar.maxValue = stunTime;
                }

                stunnBar.value = stunTime;
            }
        }
        else
        {
            if (stunnBar != null)
            {
                stunnBar.maxValue = 0.3f;
                stunnBar.value = 0;
            }
        }

        if (currentBasicCd > 0)
        {
            currentBasicCd -= Time.deltaTime;
        }


        RechargeHab1();

        RechargeHab2();

        if (spinObjects != null)
        {
            spinObjects.transform.rotation = pointer.transform.rotation;
        }

    }


    public virtual void RechargeHab1()
    {
        if (currentHab1Cd > 0)
        {
            currentHab1Cd -= Time.deltaTime;
        }
    }
    public virtual void RechargeHab2()
    {
        if (currentHab2Cd > 0)
        {
            currentHab2Cd -= Time.deltaTime;
        }
    }

    public virtual IEnumerator MainAttack()
    {
        yield return null;
    }

    public virtual IEnumerator Hab1()
    {
        yield return null;
    }

    public virtual IEnumerator Hab2()
    {
        yield return null;
        /*if (currentHab2Cd <= 0 && !casting && !dashing)
        {
            currentHab2Cd = CDR(currentMove2.cd);
            lockPointer = false;
            if (currentMove2.cast)
            {
                casting = true;
            }
            lookAtPointer = true;
            Vector2 dir = cursor.transform.position - pointer.transform.position;
            pointer.transform.up = dir;
            yield return null;
            if (currentMove2.lockPointer)
            {
                lockPointer = true;
            }
            StartCoroutine(PlayAnimation(currentMove2.anim));
            currentMove2.Trigger();
        }*/
    }

    public virtual void UsedBasicDash()
    { 
    
    }
    public virtual void EndedBasicDash()
    { 
    
    }
    public virtual void UsedBasicDashGlobal()
    { 
    
    }
    public virtual void EndedBasicDashGlobal()
    { 
    
    }

    public IEnumerator PlayAnimation(string name)
    {
        animator.Play("Idle");
        yield return null;
        animator.Play(name);
    }

    public virtual void AnimationCallStopAnim()
    {
        casting = false;
        lockPointer = false;
        lookAtPointer = false;
    }

    public bool IsCasting()
    {
        if(!casting)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public bool IsStunned()
    {
        if (stunTime > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool IsDashing()
    {
        if (dashing)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void RegisterDamage(float dmg)
    {
        if (controller != null)
        {
            dmgDealed += dmg;
            UIManager.UpdateDamageText();
        }
    }

    public virtual void DamageDealed(PjBase user, PjBase target, float amount, HitData.Element element, HitData.AttackType attackType, HitData.HabType habType)
    {
        List<HitInteract> hitList = new List<HitInteract>( target.gameObject.GetComponents<HitInteract>());
        foreach (HitInteract hit in hitList)
        {
            hit.Interact(user,target,amount,element,attackType,habType);
        }
        
    }

    public static float[][] typesChart =
   { //                       WAT GR    CR     TH   WI  ICE   NAT  FIR   NEUT
       /*Wat*/ new float[]{ 0f, 0f, 0.3f, -0.3f, 0f, -0.3f, 0f, 0.3f, 0f},
       /*Gro*/ new float[]{ 0f, 0f, -0.3f, 0f, 0.3f, 0.3f, -0.3f, 0f, 0f},
       /*Cry*/ new float[]{ -0.3f, 0.3f, 0f, 0.3f, 0f, 0f, -0.3f, 0f, 0f},
       /*Thu*/ new float[]{ 0.3f, 0f, -0.3f, 0f, 0.3f, 0f, 0f, -0.3f, 0f},
       /*Win*/ new float[]{ 0f, -0.3f, 0f, -0.3f, 0f, 0f, 0.3f, 0.3f, 0f},
       /*Ice*/ new float[]{ 0.3f, -0.3f, 0f, 0f, 0f, 0f, 0.3f, -0.3f , 0f},
       /*Nat*/ new float[]{ 0f, 0.3f, 0.3f, 0f, -0.3f, -0.3f, 0f, 0f, 0f},
       /*Fir*/ new float[]{ -0.3f, 0f, 0f, 0.3f, -0.3f, 0.3f, 0f, 0f, 0f },
       /*Neutral*/ new float[]{ 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f}
    };

    public float CalculateDmg(float baseDmg)
    {
        return CalculateDmg(baseDmg,0);
    }
    public float CalculateDmg(float baseDmg, float critChanceMod)
    {
        float critRoll = Random.Range(0f, 100f);
        if(critRoll < critChanceMod + stats.critChance)
        {
            baseDmg *= stats.critDmgMult;
        }

        return baseDmg;
    }

    void TakeDamage.TakeDamage(PjBase user,float value, HitData.Element element, AttackType type)
    {
        TakeDmg(user, value, element, type);
    }

    public virtual void TakeDmg(PjBase user, float value, HitData.Element element, AttackType type)
    {
        user.RegisterDamage(value - stats.shield);
        float calculo = 0;
        DamageText dText = null;
        if (value + dmgCount > 1)
        {
            switch (element)
            {
                case HitData.Element.ice:
                    dText = Instantiate(GameManager.Instance.damageText, transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(damageTextOffset - 0.5f, damageTextOffset + 0.5f), 0), transform.rotation).GetComponent<DamageText>();
                    dText.textColor = GameManager.Instance.iceColor;
                    break;
                case HitData.Element.fire:
                    dText = Instantiate(GameManager.Instance.damageText, transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(damageTextOffset - 0.5f, damageTextOffset + 0.5f), 0), transform.rotation).GetComponent<DamageText>();
                    dText.textColor = GameManager.Instance.fireColor;
                    break;
                case HitData.Element.water:
                    dText = Instantiate(GameManager.Instance.damageText, transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(damageTextOffset - 0.5f, damageTextOffset + 0.5f), 0), transform.rotation).GetComponent<DamageText>();
                    dText.textColor = GameManager.Instance.waterColor;
                    break;
                case HitData.Element.desert:
                    dText = Instantiate(GameManager.Instance.damageText, transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(damageTextOffset - 0.5f, damageTextOffset + 0.5f), 0), transform.rotation).GetComponent<DamageText>();
                    dText.textColor = GameManager.Instance.desertColor;
                    break;
                case HitData.Element.wind:
                    dText = Instantiate(GameManager.Instance.damageText, transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(damageTextOffset - 0.5f, damageTextOffset + 0.5f), 0), transform.rotation).GetComponent<DamageText>();
                    dText.textColor = GameManager.Instance.windColor;
                    break;
                case HitData.Element.nature:
                    dText = Instantiate(GameManager.Instance.damageText, transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(damageTextOffset - 0.5f, damageTextOffset + 0.5f), 0), transform.rotation).GetComponent<DamageText>();
                    dText.textColor = GameManager.Instance.natureColor;
                    break;
                case HitData.Element.lightning:
                    dText = Instantiate(GameManager.Instance.damageText, transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(damageTextOffset - 0.5f, damageTextOffset + 0.5f), 0), transform.rotation).GetComponent<DamageText>();
                    dText.textColor = GameManager.Instance.lightningColor;
                    break;
                case HitData.Element.crystal:
                    dText = Instantiate(GameManager.Instance.damageText, transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(damageTextOffset - 0.5f, damageTextOffset + 0.5f), 0), transform.rotation).GetComponent<DamageText>();
                    dText.textColor = GameManager.Instance.crystalColor;
                    break;
            }
        }

        float effectivenessMultiplier = 1;
        effectivenessMultiplier += typesChart[(int)element][(int)this.element];

        

        value *= effectivenessMultiplier;

        calculo = stats.resist;
        

        if (calculo < 0)
        {
            calculo = 0;
        }
        value *= 1- (calculo / (calculo + 40 + 1 *user.stats.lvl));
        float originalValue = value;
        if (controller != null)
        {
            while (stats.shield > 0 && value > 0)
            {
                Shield chosenShield = null;
                foreach (Shield shield in GetComponents<Shield>())
                {
                    if (chosenShield == null || shield.duration < chosenShield.duration && shield.shieldAmount > 0)
                    {
                        chosenShield = shield;
                    }
                }
                value = chosenShield.ChangeShieldAmount(-value);

            }

            /*if(value != originalValue)
            {
                originalValue -= value;
                DamageText sText = Instantiate(GameManager.Instance.damageText, transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(damageTextOffset - 0.5f, damageTextOffset + 0.5f), 0), transform.rotation).GetComponent<DamageText>();
                sText.textColor = Color.white;
                sText.damageText.text = originalValue.ToString("F0");
            }*/
        }

        if(effectivenessMultiplier < 0.4f)
        {
            effectivenessMultiplier = 0.4f;
        }

        if (dText != null)
        {
            switch (effectivenessMultiplier)
            {
                case 0.4f:
                    dText.damageText.text = (value + dmgCount).ToString("F0") + "VV";
                    break;
                case 0.7f:
                    dText.damageText.text = (value + dmgCount).ToString("F0") + "V";
                    break;
                case 1:
                    dText.damageText.text = (value + dmgCount).ToString("F0");
                    break;
                case 1.3f:
                    dText.damageText.text = (value + dmgCount).ToString("F0") + "!";
                    break;
                case 1.6f:
                    dText.damageText.text = (value + dmgCount).ToString("F0") + "!!";
                    break;
            }
            dText.damageText.fontSize += 0.25f;
            dmgCount = 0;
        }
        else
        {
            dmgCount += value;
        }

        stats.hp -= value;
        user.RegisterDamage(value);
        if (stats.hp <= 0)
        {
            GetComponent<TakeDamage>().Die(user);
        }
        if (hpBar != null)
        {
            hpBar.maxValue = stats.mHp;
            hpBar.value = stats.hp;
        }
    }

    public virtual void Heal(PjBase user, float value, HitData.Element element)
    {
        if (stats.hp > 0)
        {
            stats.hp += value;
            if (stats.hp > stats.mHp)
            {
                value -= (stats.hp - stats.mHp);
                stats.hp = stats.mHp;
            }

            user.RegisterDamage(value);

            if (value + healCount > 1)
            {
                value += healCount;
                healCount = 0;
                DamageText dText = null;
                switch (element)
                {
                    case HitData.Element.ice:
                        dText = Instantiate(GameManager.Instance.damageText, transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(damageTextOffset - 0.5f, damageTextOffset + 0.5f), 0), transform.rotation).GetComponent<DamageText>();
                        dText.textColor = GameManager.Instance.iceColor;
                        break;
                    case HitData.Element.fire:
                        dText = Instantiate(GameManager.Instance.damageText, transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(damageTextOffset - 0.5f, damageTextOffset + 0.5f), 0), transform.rotation).GetComponent<DamageText>();
                        dText.textColor = GameManager.Instance.fireColor;
                        break;
                    case HitData.Element.water:
                        dText = Instantiate(GameManager.Instance.damageText, transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(damageTextOffset - 0.5f, damageTextOffset + 0.5f), 0), transform.rotation).GetComponent<DamageText>();
                        dText.textColor = GameManager.Instance.waterColor;
                        break;
                    case HitData.Element.desert:
                        dText = Instantiate(GameManager.Instance.damageText, transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(damageTextOffset - 0.5f, damageTextOffset + 0.5f), 0), transform.rotation).GetComponent<DamageText>();
                        dText.textColor = GameManager.Instance.desertColor;
                        break;
                    case HitData.Element.wind:
                        dText = Instantiate(GameManager.Instance.damageText, transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(damageTextOffset - 0.5f, damageTextOffset + 0.5f), 0), transform.rotation).GetComponent<DamageText>();
                        dText.textColor = GameManager.Instance.windColor;
                        break;
                    case HitData.Element.nature:
                        dText = Instantiate(GameManager.Instance.damageText, transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(damageTextOffset - 0.5f, damageTextOffset + 0.5f), 0), transform.rotation).GetComponent<DamageText>();
                        dText.textColor = GameManager.Instance.natureColor;
                        break;
                    case HitData.Element.lightning:
                        dText = Instantiate(GameManager.Instance.damageText, transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(damageTextOffset - 0.5f, damageTextOffset + 0.5f), 0), transform.rotation).GetComponent<DamageText>();
                        dText.textColor = GameManager.Instance.lightningColor;
                        break;
                    case HitData.Element.crystal:
                        dText = Instantiate(GameManager.Instance.damageText, transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(damageTextOffset - 0.5f, damageTextOffset + 0.5f), 0), transform.rotation).GetComponent<DamageText>();
                        dText.textColor = GameManager.Instance.crystalColor;
                        break;
                }

                dText.damageText.text = "+" + value.ToString("F0");
            }
            else
            {
                healCount += value;
            }
        }

    }

    public virtual void Stunn(PjBase target, float value)
    {
        target.GetComponent<TakeDamage>().Stunn(value);
    }

    public virtual void OnGlobalStunn(PjBase target, float value)
    {

    }

    public virtual void OnGlobalDamageTaken()
    {

    }
    public virtual void OnDamageTaken()
    {

    }

    public virtual void Moving(float magnitude)
    {

    }

    public virtual void GlobalMoving(float magnitude, PjBase user)
    {

    }
    void TakeDamage.Stunn(float stunTime)
    {
        if (this.stunTime < stunTime)
        {
            if (controller)
            {
                stunTime /= 3;
            }
            this.stunTime = stunTime;
        }
    }
    void TakeDamage.Die(PjBase killer)
    {
        killer.OnKill(this);

        GameManager.Instance.pjList.Remove(this);

        Destroy(gameObject);
    }

    public virtual float CalculateSinergy(float calculo)
    {
        float value = stats.sinergy;
        value *= calculo / 100;
        //valor.text = value.ToString();
        return value;

    }

    public virtual float CalculateStrength(float calculo)
    {
        float value = stats.strength;
        value *= calculo / 100;
        //valor.text = value.ToString();
        return value;

    }

    public float CDR(float value)
    {
        value -= ((value * ((stats.cdr / (100 + stats.cdr)))));
        return value;
    }
    public float CalculateAtSpd(float value)
    {
        value = 1 / value;
        return value;
    }

    public virtual IEnumerator Dash(Vector2 direction, float speed, float range)
    {
        if (this != null)
        {
            StartCoroutine(Dash(direction, speed, range, false,true));
        }
        yield return null;
    }
    public virtual IEnumerator Dash(Vector2 direction, float speed, float range, bool ignoreWalls, bool ignoreAir)
    {
        if (this != null)
        {
            StartCoroutine(Dash(direction, speed, range, false, true, true));
        }
        yield return null;
    }
    public virtual IEnumerator Dash(Vector2 direction, float speed, float range, bool ignoreWalls, bool ignoreAir, bool cancelAnimation)
    {
        if (stunTime > 0)
        {
            stunTime = 0;
            yield return null;
        }
        if (dashing)
        {
            dashing = false;
            yield return null;
        }

        StartCoroutine(PlayAnimation("Dash"));

        if (GetComponent<NavMeshAgent>())
        {
            GetComponent<NavMeshAgent>().enabled = false;
        }


        GetComponent<Collider2D>().isTrigger = true;

        dashing = true;
        Vector2 destinyPoint = new Vector2(0, 0);
        if (ignoreAir && !ignoreWalls)
        {
            destinyPoint = Physics2D.Raycast(transform.position, direction, range, GameManager.Instance.wallLayer).point;
        }
        else if (!ignoreAir && ignoreWalls)
        {
            destinyPoint = Physics2D.Raycast(transform.position, direction, range, GameManager.Instance.airLayer).point;
        }
        else if (!ignoreAir && !ignoreWalls)
        {
            destinyPoint = Physics2D.Raycast(transform.position, direction, range, GameManager.Instance.airLayer + GameManager.Instance.wallLayer).point;
        }

        yield return null;
        if (destinyPoint == new Vector2(0, 0) )
        {
            destinyPoint = new Vector2(transform.position.x, transform.position.y) + direction.normalized * range;
        }

        NavMeshHit hit;
        NavMesh.SamplePosition(new Vector3(destinyPoint.x, destinyPoint.y, transform.position.z), out hit, 100, 1);
        destinyPoint = hit.position;

        Vector2 distance = destinyPoint - new Vector2(transform.position.x, transform.position.y);
        yield return null;
        while (distance.magnitude > 1 && dashing && stunTime <= 0)
        {
            if (distance.magnitude > 0.7)
            {
                rb.velocity = distance.normalized * speed;
            }
            else
            {
                rb.velocity = distance * speed;
            }
            distance = destinyPoint - new Vector2(transform.position.x, transform.position.y);
            yield return null;
        }
        dashing = false;
        rb.velocity = new Vector2(0, 0);

        GetComponent<Collider2D>().isTrigger = false;
        if (GetComponent<NavMeshAgent>())
        {
            GetComponent<NavMeshAgent>().enabled = true;
        }


        if (cancelAnimation)
        {
            StartCoroutine(PlayAnimation("Idle"));
            AnimationCallStopAnim();
        }
    }
    public virtual IEnumerator Dash(Transform direction, float speed, float range, bool ignoreWalls, bool ignoreAir, bool cancelAnimation)
    {
        if (stunTime > 0)
        {
            stunTime = 0;
            yield return null;
        }

        StartCoroutine(PlayAnimation("Dash"));

        if (GetComponent<NavMeshAgent>())
        {
            GetComponent<NavMeshAgent>().enabled = false;
        }


        GetComponent<Collider2D>().isTrigger = true;

        dashing = true;
       
        NavMeshHit hit;

        Vector2 distance = (Vector2)direction.position - new Vector2(transform.position.x, transform.position.y);
        yield return null;
        while (distance.magnitude > 0.4 && dashing && stunTime <= 0 && direction)
        {
            if (distance.magnitude > 0.7)
            {
                rb.velocity = distance.normalized * speed;
            }
            else
            {
                rb.velocity = distance * speed;
            }
            distance = (Vector2)direction.position - new Vector2(transform.position.x, transform.position.y);
            yield return null;
        }
        dashing = false;
        rb.velocity = new Vector2(0, 0);

        GetComponent<Collider2D>().isTrigger = false;
        if (GetComponent<NavMeshAgent>())
        {
            GetComponent<NavMeshAgent>().enabled = true;
        }


        if (cancelAnimation)
        {
            StartCoroutine(PlayAnimation("Idle"));
            AnimationCallStopAnim();
        }
    }

    public void AnimationCursorLock(int value)
    {
        if (controller != null)
        {
            if (value == 1)
            {
                controller.LockPointer(true);
            }
            else
            {
                controller.LockPointer(false);
            }
        }
        else
        {
            if (value == 1)
            {
                lockPointer = true;
            }
            else
            {
                lockPointer = false;
            }
        }
    }

    public virtual void OnKill(PjBase target)
    {

    }

    public virtual void Interact(PjBase user, PjBase target, float amount, HitData.Element element, HitData.AttackType attackType, HitData.HabType habType)
    {

    }

}

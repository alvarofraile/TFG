using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    //[SerializeField] private Transform bulletProjectilePrefab;
    //[SerializeField] private Transform shootPointTransform;
    [SerializeField] private Transform rifleTransform;

    private void Awake()
    {
        if (TryGetComponent<MoveAction>(out MoveAction moveAction))
        {
            moveAction.OnStartMoving += moveAction_OnStartMoving;
            moveAction.OnStopMoving += moveAction_OnStopMoving;
        }

        if (TryGetComponent<ShootAction>(out ShootAction shootAction))
        {
            shootAction.OnShoot += shootAction_OnShoot;
        }

        if (TryGetComponent<HealAction>(out HealAction healAction))
        {
            healAction.OnHeal += healAction_OnHeal;
        }

        if (TryGetComponent<MeleeAction>(out MeleeAction meleeAction))
        {
            meleeAction.OnMeleeActionStarted += meleeAction_OnMeleeActionStarted;
        }
    }

    private void meleeAction_OnMeleeActionStarted(object sender, EventArgs e)
    {
        animator.SetTrigger("melee");
    }

    private void healAction_OnHeal(object sender, EventArgs e)
    {
        animator.SetTrigger("heal");
    }

    private void moveAction_OnStartMoving(object sender, EventArgs e)
    {
        animator.SetBool("isWalking", true);
    }

    private void moveAction_OnStopMoving(object sender, EventArgs e)
    {
        animator.SetBool("isWalking", false);
    }

    private void shootAction_OnShoot(object sender, ShootAction.OnShootEventArgs e)
    {
        animator.SetTrigger("shoot");

        //TODO -> instanciar bala

        //Transform bulletProjectileTransform =
            //Instantiate(bulletProjectilePrefab, shootPointTransform.position, Quaternion.identity);

        //BulletProjectile bulletProjectile = bulletProjectileTransform.GetComponent<BulletProjectile>();

        //Vector3 targetUnitShootAtPosition = e.targetUnit.GetWorldPosition();

        //targetUnitShootAtPosition.y = shootPointTransform.position.y;

        //bulletProjectile.Setup(targetUnitShootAtPosition);
    }
}

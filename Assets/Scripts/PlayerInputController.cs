using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    public InputControls inputActions;
    public InputControls.PlayerActions playerActions;
    public Player player;
    public PhotonView photonView;
    private void Awake()
    {
        photonView =GetComponent<PhotonView>();
        
        inputActions = new();
        playerActions = inputActions.Player;
        player = GetComponent<Player>();
    }
    private void OnEnable()
    {
        playerActions.Enable();
        playerActions.Movement.started += Move;
        playerActions.Movement.canceled += Move;
        playerActions.Jump.started += _ => Jump();
        playerActions.Attack1.started +=_=> Fire();
        playerActions.Action.started += _ => { player.isActionPressed = true; };
        playerActions.Action.canceled += _ => { player.isActionPressed = false; };
        playerActions.Dash.started += _ => Dash();
        playerActions.MeleeAttack.started += _ =>  MeleeAttack();
        playerActions.Throw.started += _ =>  Throw();
        playerActions.AimDelta.started += _ => MousePos(_);
    }

    private void MousePos(InputAction.CallbackContext context)
    {
        player.MousePos(playerActions.Aim.ReadValue<Vector2>());
    }

    private void Throw()
    {
        player.Throw();
    }

    private void OnDisable()
    {
        playerActions.Disable();
        playerActions.Movement.started -= Move;
        playerActions.Movement.canceled -= Move;
        playerActions.Jump.started -= _ => Jump();
        playerActions.Attack1.started -=_=> Fire();
        playerActions.Action.started -= _ => { player.isActionPressed = true; };
        playerActions.Action.canceled -= _ => { player.isActionPressed = false; };
        playerActions.Dash.started -= _=> Dash();
        playerActions.MeleeAttack.started -= _ => MeleeAttack();
        playerActions.Throw.started -= _ => Throw();
        playerActions.AimDelta.started -= _ => MousePos(_);
    }


    private void Fire()
    {
        if (!photonView.IsMine)
            return;

        player.Fire();
    }

    private void Jump()
    {
        if (!photonView.IsMine)
            return;
        player.Jump();
    }

    private void Move(InputAction.CallbackContext context)
    {
        if (!photonView.IsMine)
            return;
        player.Move(context);
    }

    private void Dash()
    {
        if(!photonView.IsMine)
		    return;
        player.Dash();
    }

    private void MeleeAttack()
    {
        if (!photonView.IsMine)
            return;
        player.MeleeAttack();
    }


}

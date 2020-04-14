using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBaseState : State
{

    public LocomotionController Player => player = player ?? (LocomotionController)owner; // om player inte är null casta owner till att vara en PlayerController och returnera till player, Player blir till player.
    private LocomotionController player;

}

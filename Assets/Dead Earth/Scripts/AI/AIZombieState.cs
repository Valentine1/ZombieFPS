using UnityEngine;
using System.Collections;

public abstract class AIZombieState : AIState {

    protected int playerLayerMask = -1;
    protected int bodyPartsLayerNumber = -1;
    protected AIZombiStateMachine ZombieStateMachine;

    public override void SetStateMachine(AIStateMachine machine)
    {
        if (machine.GetType() == typeof(AIZombiStateMachine))
        {
            base.SetStateMachine(machine);
            ZombieStateMachine = (AIZombiStateMachine)machine;
        }
    }

    void Awake()
    {
        playerLayerMask = LayerMask.GetMask("Player", "AI Body Parts") + 1; //+1 for Default layer
        bodyPartsLayerNumber = LayerMask.NameToLayer("AI Body Parts");
    }

    public override void OnSensorEvent(AITriggerEventType eventType, Collider c)
    {
        base.OnSensorEvent(eventType, c);
        if (this.ZombieStateMachine)
        {
            return;
        }
        if (eventType != AITriggerEventType.Exit)
        {
            if (c.CompareTag("Player"))
            {
                float distance = Vector3.Distance(this.ZombieStateMachine.SensorPosition, c.transform.position);
                //if current target is not Player or  is PLayer and new target is Player but is closer
                if (this.ZombieStateMachine.VisualTarget.Type != AITargetType.Player || (this.ZombieStateMachine.VisualTarget.Type == AITargetType.Player
                    && distance < this.ZombieStateMachine.VisualTarget.Distance))
                {
                    RaycastHit hitInfo;
                    if (ColliderIsTangible(c, out hitInfo))
                    {
                        // Yep...it's closest and in FOV so we store it as our visual target
                        this.ZombieStateMachine.VisualTarget.Set(AITargetType.Player, c, c.transform.position, distance);
                    }
                }
            }
        }
    }

    protected virtual bool ColliderIsTangible(Collider c, out RaycastHit hitInfo)
    {
        hitInfo = new RaycastHit();
        Vector3 directionToTarget = c.transform.position - this.StateMachine.SensorPosition;
        float angle = Vector3.Angle(directionToTarget, this.transform.forward);
        if (angle > ZombieStateMachine.FOV * 0.5f)
        {
            return false;
        }
        RaycastHit[] hits = Physics.RaycastAll(this.StateMachine.SensorPosition, directionToTarget,
                                               this.StateMachine.SensorRadius * this.ZombieStateMachine.SightDistance, this.playerLayerMask);

        float closestHitDistance = float.MaxValue;
        Collider closestCollider = null;

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            if (hit.distance < closestHitDistance)
            {
                //if raycast does not hit zombie's own body parts then...
                if (!(hit.transform.gameObject.layer == this.bodyPartsLayerNumber &&
                    this.StateMachine == GameSceneManager.Instance.GetAIStateMachine(hit.rigidbody.GetInstanceID())))
                {
                    closestHitDistance = hit.distance;
                    closestCollider = hit.collider;
                    hitInfo = hit;
                }
            }
        }

        if (closestCollider && closestCollider == c.gameObject)
        {
            return true;
        }
        return false;

    }
}

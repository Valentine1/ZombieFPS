using UnityEngine;
using System.Collections;

public abstract class AIZombieState : AIState {

    protected int playerLayerMask = -1;
    protected int visualLayerMask = -1;
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
        visualLayerMask = LayerMask.GetMask("Player", "Visual Aggravotor") + 1;
        bodyPartsLayerNumber = LayerMask.NameToLayer("AI Body Parts");
    }

    public override void OnSensorEvent(AITriggerEventType eventType, Collider c)
    {
        base.OnSensorEvent(eventType, c);
        if (!this.ZombieStateMachine)
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
                    if (ColliderIsTangible(c, out hitInfo, this.playerLayerMask))
                    {
                        // Yep...it's closest and in FOV so we store it as our visual target
                        this.ZombieStateMachine.VisualTarget.Set(AITargetType.Player, c, c.transform.position, distance);
                    }
                }
            }
            else if (c.CompareTag("Flash Light") && this.ZombieStateMachine.VisualTarget.Type != AITargetType.Player)
            {
                //flash light is presented by a long Box Collider
                BoxCollider flashLightTrigger = (BoxCollider)c;
                float distanceToLight = Vector3.Distance(this.ZombieStateMachine.SensorPosition, flashLightTrigger.transform.position);
                float zSize = flashLightTrigger.size.z * flashLightTrigger.transform.lossyScale.z;
                float aggrFactor = distanceToLight / zSize;
                if (aggrFactor <= ((this.ZombieStateMachine.SightDistance + this.ZombieStateMachine.Intelligence) / 2f))
                {
                    this.ZombieStateMachine.VisualTarget.Set(AITargetType.Light, c, c.transform.position, distanceToLight);
                }
            }
            else if (c.CompareTag("AI Sound Emitter") && this.ZombieStateMachine.VisualTarget.Type != AITargetType.Player)
            {
                //sound is presented by a spherical collider
                SphereCollider soundTrigger = (SphereCollider)c;
                if (soundTrigger == null)
                {
                    return;
                }
                Vector3 soundPos;
                float soundRadius;
                AIState.SphereColliderToWorldSpace(soundTrigger, out soundPos, out soundRadius);
                //float distanceToSound = Vector3.Distance(this.ZombieStateMachine.SensorPosition, soundPos);
                float distanceToSound = (soundPos - this.ZombieStateMachine.SensorPosition).magnitude;
                float aggrFactor = distanceToSound / soundRadius;
                if (aggrFactor <= this.ZombieStateMachine.HearingDistance && distanceToSound < this.ZombieStateMachine.AudioTarget.Distance)
                {
                    this.ZombieStateMachine.AudioTarget.Set(AITargetType.Audio, c, soundPos, distanceToSound);
                }
            }
            else if (c.CompareTag("AI Food") && this.ZombieStateMachine.VisualTarget.Type != AITargetType.Player &&
                this.ZombieStateMachine.VisualTarget.Type != AITargetType.Light && 
                this.ZombieStateMachine.AudioTarget.Type== AITargetType.None && this.ZombieStateMachine.Satisfaction<=0.9f)
            {
                float distanceToFood = Vector3.Distance(this.ZombieStateMachine.SensorPosition, c.transform.position);
                // Is this nearer then anything we have previous stored
                if (this.ZombieStateMachine.VisualTarget.Type == AITargetType.None ||  distanceToFood < this.ZombieStateMachine.VisualTarget.Distance)
                {
                    //check if target is not hidden behind other colliders and is within FOV
                    RaycastHit hitInfo;
                    if (this.ColliderIsTangible(c, out hitInfo, this.visualLayerMask))
                    {
                        this.ZombieStateMachine.VisualTarget.Set(AITargetType.Food, c, c.transform.position, distanceToFood);
                    }
                }
            }
        }
    }

    protected virtual bool ColliderIsTangible(Collider c, out RaycastHit hitInfo, int mask)
    {
        hitInfo = new RaycastHit();
        Vector3 directionToTarget = c.transform.position - this.StateMachine.SensorPosition;
        float angle =Mathf.Abs(90 - Vector3.Angle(directionToTarget, this.transform.right));
        if (angle > ZombieStateMachine.FOV * 0.5f)
        {
            return false;
        }
        Vector3 soundPos;
        float soundRadius;
        AIState.SphereColliderToWorldSpace((SphereCollider)c, out soundPos, out soundRadius);
        RaycastHit[] hits = Physics.RaycastAll(this.StateMachine.SensorPosition, directionToTarget,
                                               this.StateMachine.SensorRadius * this.ZombieStateMachine.SightDistance,mask);

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

        if (closestCollider && closestCollider == c)
        {
            return true;
        }
        return false;

    }
}

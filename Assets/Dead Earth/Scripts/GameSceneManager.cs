using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameSceneManager : MonoBehaviour {

    private static GameSceneManager _instance = null;
    public static GameSceneManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameSceneManager>();
            }
            return _instance;
        }
    }

    private Dictionary<int, AIStateMachine> StateMachines = new Dictionary<int, AIStateMachine>();

    public void RegisterAIStateMachine(int key, AIStateMachine machine)
    {

        if (!StateMachines.ContainsKey(key))
        {
            StateMachines[key] = machine;
        }
    }

    public AIStateMachine GetAIStateMachine(int key)
    {
        if (StateMachines.ContainsKey(key))
        {
            return StateMachines[key];
        }
        return null;
    }
}

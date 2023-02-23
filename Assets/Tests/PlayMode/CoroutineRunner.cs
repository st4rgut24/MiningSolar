using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineRunner : MonoBehaviour
{
    public void testScheduleActionsCoroutine(Scheduler scheduler)
    {
        StartCoroutine(scheduler.ScheduleActions());
    }

    public void testRewardGeneratorCoroutine(RewardGenerator rewardGenerator)
    {
        StartCoroutine(rewardGenerator.ChooseRandomWinner());
    }
}

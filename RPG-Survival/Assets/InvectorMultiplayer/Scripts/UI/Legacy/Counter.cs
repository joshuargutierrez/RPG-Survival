using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBGames.UI
{
    [AddComponentMenu("CB GAMES/UI/Legacy/Counter")]
    public class Counter : MonoBehaviour
    {
        public enum CounterType { CountDown, CountUp }
        [Tooltip("The text object that will display the number as it counts up/down.")]
        [SerializeField] private Text counter = null;
        [Tooltip("Do you want this to count up or down to the target amount?")]
        [SerializeField] private CounterType counterType = CounterType.CountDown;

        private bool counting = false;
        float currentNumber = 0.0f;
        float targetNumber = 0.0f;

        /// <summary>
        /// Start counting down from the specified input amount.
        /// </summary>
        /// <param name="amount">float type, the amount of time to start counting down from.</param>
        public void StartCounting(float amount)
        {
            targetNumber = amount;
            switch (counterType)
            {
                case CounterType.CountDown:
                    counter.text = amount.ToString();
                    currentNumber = amount;
                    break;
                case CounterType.CountUp:
                    counter.text = "0";
                    currentNumber = 0;
                    break;
            }
            counting = true;
        }

        /// <summary>
        /// Is responsible for counting down and setting the `counter` value.
        /// </summary>
        private void Update()
        {
            if (counting == true)
            {
                switch (counterType)
                {
                    case CounterType.CountDown:
                        currentNumber -= Time.deltaTime;
                        counter.text = (currentNumber > 0) ? currentNumber.ToString("f0") : "0";
                        if (currentNumber <= 0)
                        {
                            counting = false;
                        }
                        break;
                    case CounterType.CountUp:
                        currentNumber += Time.deltaTime;
                        counter.text = (currentNumber < targetNumber) ? currentNumber.ToString("f0") : targetNumber.ToString(); ;
                        if (currentNumber >= targetNumber)
                        {
                            counting = false;
                        }
                        break;
                }
            }
        }
    }
}
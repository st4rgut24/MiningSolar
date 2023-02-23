using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts
{
    /**
     * A player consists of his assets
     */
    public class Player : IPlayer
    {
        protected override void Start()
        {
            base.Start();
        }

        /// <summary>
        /// Upon receiving a contract the player must be made aware of the contract
        /// </summary>
        /// <param name="contract">The contract player must accept or decline</param>
        /// <returns>true if the player accepts the contract</returns>
        public override bool negotiateContract(Contract contract)
        {
            return true;
        }

        /// <summary>
        /// A player gets a new plot
        /// </summary>
        public void addPlot(Plot plot)
        {
            plots.Add(plot);
        }
    }
}

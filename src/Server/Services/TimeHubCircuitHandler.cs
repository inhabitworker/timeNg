﻿using Microsoft.AspNetCore.Components.Server.Circuits;

namespace Server.Services
{
    public class TimeHubCircuitHandler : CircuitHandler
    {
        private HashSet<Circuit> circuits = new();

        public override Task OnConnectionUpAsync(Circuit circuit,
            CancellationToken cancellationToken)
        {
            circuits.Add(circuit);

            return Task.CompletedTask;
        }

        public override Task OnConnectionDownAsync(Circuit circuit,
            CancellationToken cancellationToken)
        {
            circuits.Remove(circuit);

            return Task.CompletedTask;
        }

        public int ConnectedCircuits => circuits.Count;
    }
}

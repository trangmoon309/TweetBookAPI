using System;
using System.Collections.Generic;
using System.Text;

namespace Tweetbook.Contracts.HealthChecks
{
    public class HealthCheckResponse
    {
        public string Status { get; set; }
        public IEnumerable<HealthCheckss> Checks { get; set; }

        public TimeSpan Duration { get; set; }
    }
}

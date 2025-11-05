using Microsoft.Extensions.Hosting;
using Myth.Instrumentations.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myth.Instrumentations.ValueProviders {

    public abstract class CustomMetric : ICustomMetric {
        protected readonly IMeterFactory _meterFactory;
        protected readonly Meter _meter;

        protected CustomMetric(string name, IMeterFactory meterFactory) {
            _meterFactory = meterFactory;
            _meter = _meterFactory.Create(name);
        }
    }
}

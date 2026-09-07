using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ClyvoVet.API.Infrastructure.Observability
{
    public class ApiMetrics
    {
        public const string MeterName = "ClyvoVet.API";

        private readonly Meter _meter = new(MeterName, "1.0.0");
        private readonly Counter<long> _requestCounter;
        private readonly Counter<long> _errorCounter;
        private readonly Histogram<double> _responseTime;
        private long _totalRequests;
        private long _totalErrors;
        private long _totalDurationMicroseconds;

        public ApiMetrics()
        {
            _requestCounter = _meter.CreateCounter<long>("clyvovet.api.requests", "requests");
            _errorCounter = _meter.CreateCounter<long>("clyvovet.api.errors", "errors");
            _responseTime = _meter.CreateHistogram<double>("clyvovet.api.response_time", "ms");

            _meter.CreateObservableGauge(
                "clyvovet.api.error_rate",
                () => GetSnapshot().ErrorRatePercent,
                "%");

            _meter.CreateObservableGauge(
                "clyvovet.api.average_response_time",
                () => GetSnapshot().AverageResponseTimeMs,
                "ms");
        }

        public void Record(string method, string route, int statusCode, double elapsedMilliseconds)
        {
            var tags = new TagList
            {
                { "http.request.method", method },
                { "http.route", route },
                { "http.response.status_code", statusCode }
            };

            _requestCounter.Add(1, tags);
            _responseTime.Record(elapsedMilliseconds, tags);
            Interlocked.Increment(ref _totalRequests);
            Interlocked.Add(ref _totalDurationMicroseconds, (long)(elapsedMilliseconds * 1000));

            if (statusCode >= 400)
            {
                _errorCounter.Add(1, tags);
                Interlocked.Increment(ref _totalErrors);
            }
        }

        public ApiMetricsSnapshot GetSnapshot()
        {
            var requests = Interlocked.Read(ref _totalRequests);
            var errors = Interlocked.Read(ref _totalErrors);
            var durationMicroseconds = Interlocked.Read(ref _totalDurationMicroseconds);

            var averageResponseTimeMs = requests == 0 ? 0 : durationMicroseconds / 1000d / requests;
            var errorRatePercent = requests == 0 ? 0 : errors * 100d / requests;

            return new ApiMetricsSnapshot(
                requests,
                errors,
                Math.Round(errorRatePercent, 2),
                Math.Round(averageResponseTimeMs, 2));
        }
    }

    public record ApiMetricsSnapshot(
        long TotalRequests,
        long TotalErrors,
        double ErrorRatePercent,
        double AverageResponseTimeMs);
}

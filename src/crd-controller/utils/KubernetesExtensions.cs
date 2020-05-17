using System;
using System.Reactive.Linq;
using System.Threading;
using k8s;

namespace CustomResourceDescriptorController.utils
{
    public static class KubernetesExtensions
    {
        public static IObservable<(WatchEventType, TCRD)> ObserveClusterCustomObject<TCRD>(
            this IKubernetes kubernetes,
            string group,
            string version,
            string plural,
            CancellationToken cancellationToken
            ) where TCRD : class
        {
            return Observable.FromAsync(async () =>
            {
                var subject = new System.Reactive.Subjects.Subject<(WatchEventType, TCRD)>();
                var path = $"apis/{group}/{version}/watch/{plural}";
                await kubernetes.WatchObjectAsync<TCRD>(path,
                    timeoutSeconds: (int) TimeSpan.FromMinutes(60).TotalSeconds,
                    onEvent: (@type, @event) => subject.OnNext((@type, @event)),
                    onError: e => subject.OnError(e),
                    onClosed: () => subject.OnCompleted(), cancellationToken: cancellationToken);
                return subject;
            })
                .SelectMany(x => x)
                .Select(t => (t.Item1, t.Item2 as TCRD))
                .Where(t => t.Item2 != null);
        }
    }
}

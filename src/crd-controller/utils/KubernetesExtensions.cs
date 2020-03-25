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
            CancellationToken cancelationToken
            ) where TCRD : class
        {
            return Observable.FromAsync(async () =>
            {
                var subject = new System.Reactive.Subjects.Subject<(WatchEventType, TCRD)>();
                var path = $"apis/{group}/{version}/{plural}";
                var watcher = await kubernetes.WatchObjectAsync<TCRD>(path,
                    onEvent: (@type, @event) => subject.OnNext((@type, @event)),
                    onError: e => subject.OnError(e),
                    onClosed: () => subject.OnCompleted());
                return subject;
            })
                .SelectMany(x => x)
                .Select(t => (t.Item1, t.Item2 as TCRD))
                .Where(t => t.Item2 != null);
        }
    }
}

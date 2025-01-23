#pragma warning disable IDE0130 // Namespace does not match folder structure

#if !NETCOREAPP

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}

namespace System.Collections.Generic
{
    internal static class KeyValuePairExtensions
    {
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, out TKey key, out TValue value)
        {
            key = pair.Key;
            value = pair.Value;
        }
    }

    internal static class DictionaryExtensions
    {
        public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
            {
                return false;
            }
            dictionary.Add(key, value);
            return true;
        }
    }
}

namespace System.Threading.Tasks
{
    internal static class AsyncEnumerableExtensions
    {
        public static IEnumerable<T> ToBlockingEnumerable<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
        {
            IAsyncEnumerator<T> enumerator = source.GetAsyncEnumerator(cancellationToken);
            // A ManualResetEventSlim variant that lets us reuse the same
            // awaiter callback allocation across the entire enumeration.
            ManualResetEventWithAwaiterSupport? mres = null;

            try
            {
                while (true)
                {
#pragma warning disable CA2012 // Use ValueTasks correctly
                    ValueTask<bool> moveNextTask = enumerator.MoveNextAsync();
#pragma warning restore CA2012 // Use ValueTasks correctly

                    if (!moveNextTask.IsCompleted)
                    {
#pragma warning disable CA2000 // Dispose objects before losing scope
                        (mres ??= new ManualResetEventWithAwaiterSupport()).Wait(moveNextTask.ConfigureAwait(false).GetAwaiter());
#pragma warning restore CA2000 // Dispose objects before losing scope
                        Debug.Assert(moveNextTask.IsCompleted);
                    }

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
                    if (!moveNextTask.Result)
                    {
                        yield break;
                    }
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits

                    yield return enumerator.Current;
                }
            }
            finally
            {
                ValueTask disposeTask = enumerator.DisposeAsync();

                if (!disposeTask.IsCompleted)
                {
#pragma warning disable CA2000 // Dispose objects before losing scope
                    (mres ?? new ManualResetEventWithAwaiterSupport()).Wait(disposeTask.ConfigureAwait(false).GetAwaiter());
#pragma warning restore CA2000 // Dispose objects before losing scope
                    Debug.Assert(disposeTask.IsCompleted);
                }

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
                disposeTask.GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
            }
        }

        private sealed class ManualResetEventWithAwaiterSupport : ManualResetEventSlim
        {
            private readonly Action _onCompleted;

            public ManualResetEventWithAwaiterSupport()
            {
                _onCompleted = Set;
            }

            public void Wait<TAwaiter>(TAwaiter awaiter) where TAwaiter : ICriticalNotifyCompletion
            {
                awaiter.UnsafeOnCompleted(_onCompleted);
                Wait();
                Reset();
            }
        }
    }
}

#endif

namespace System.IO
{
}

#pragma warning restore IDE0130 // Namespace does not match folder structure

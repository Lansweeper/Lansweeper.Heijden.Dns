namespace Lansweeper.Heijden.Dns;

internal static class TimeoutHelper
{
    /// <summary>
    /// Executes the asynchronous action with the specified timeout.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="timeoutInMilliseconds">The timeout in milliseconds.</param>
    /// <param name="cancellationToken">The cancellation token to use.</param>
    /// <returns>Resulting Task result</returns>
    /// <exception cref="OperationCanceledException"></exception>
    /// <exception cref="TimeoutException"></exception>
    public static async ValueTask ExecuteAsyncWithTimeout(Func<CancellationToken, ValueTask> action, int timeoutInMilliseconds, CancellationToken cancellationToken)
    {
        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var linkedCancellationToken = timeoutCts.Token; // This token will be cancelled both when timeout occurs or original token is cancelled
            timeoutCts.CancelAfter(timeoutInMilliseconds);
            await action(linkedCancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested) // Not cancelled with original token --> must be caused by timeout
        {
            throw new TimeoutException("Operation timed out", ex);
        }
    }

    /// <summary>
    /// Executes the asynchronous action with the specified timeout.
    /// </summary>
    /// <typeparam name="T">Type of the return value in the ValueTask</typeparam>
    /// <param name="action">The action to execute.</param>
    /// <param name="timeoutInMilliseconds">The timeout in milliseconds.</param>
    /// <param name="cancellationToken">The cancellation token to use.</param>
    /// <returns>Resulting Task result</returns>
    /// <exception cref="OperationCanceledException"></exception>
    /// <exception cref="TimeoutException"></exception>
    public static async ValueTask<T> ExecuteAsyncWithTimeout<T>(Func<CancellationToken, ValueTask<T>> action, int timeoutInMilliseconds, CancellationToken cancellationToken)
    {
        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var linkedCancellationToken = timeoutCts.Token; // This token will be cancelled both when timeout occurs or original token is cancelled
            timeoutCts.CancelAfter(timeoutInMilliseconds);
            return await action(linkedCancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested) // Not cancelled with original token --> must be caused by timeout
        {
            throw new TimeoutException("Operation timed out", ex);
        }
    }
}
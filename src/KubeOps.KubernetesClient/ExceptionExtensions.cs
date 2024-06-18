namespace KubeOps.KubernetesClient;

/// <summary>
/// Method extensions for the <see cref="Exception"/> class.
/// </summary>
public static class ExceptionExtensions
{
    /// <summary>
    /// Walk through all collected Exceptions (base exception and all inner exceptions) LINQ style.
    /// </summary>
    /// <param name="self"><see cref="Exception"/>.</param>
    /// <returns>IEnumerable for all inner exceptions.</returns>
    /// <exception cref="ArgumentNullException">self is null.</exception>
    public static IEnumerable<Exception> All(this Exception self)
    {
        if (self == null)
        {
            throw new ArgumentNullException(nameof(self));
        }

        return self.TakeAll();
    }

    private static IEnumerable<Exception> TakeAll(this Exception self)
    {
        var cause = self;
        do
        {
            yield return cause;
            cause = ReferenceEquals(cause, cause.InnerException) ? null : cause.InnerException;
        }
        while (cause != null && !ReferenceEquals(cause, self));
    }
}

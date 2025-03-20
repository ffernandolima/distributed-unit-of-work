using System;
using System.Transactions;

namespace DistributedUnitOfWork.Factories;

/// <summary>
/// Factory class for creating TransactionScope instances with specified options.
/// </summary>
public static class TransactionScopeFactory
{
    /// <summary>
    /// Creates a new TransactionScope with the specified options.
    /// </summary>
    /// <param name="transactionScopeOption">The transaction scope option.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <param name="timeout">The timeout for the transaction scope.</param>
    /// <param name="transactionScopeAsyncFlowOption">The async flow option for the transaction scope.</param>
    /// <returns>A new TransactionScope instance.</returns>
    public static TransactionScope CreateTransactionScope(
        TransactionScopeOption transactionScopeOption = TransactionScopeOption.Required,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        TimeSpan? timeout = null,
        TransactionScopeAsyncFlowOption? transactionScopeAsyncFlowOption = null)
    {
        TransactionScope transactionScope;

        var transactionOptions = new TransactionOptions
        {
            IsolationLevel = isolationLevel
        };

        if (timeout.HasValue)
        {
            transactionOptions.Timeout = timeout.Value;
        }
        else
        {
            transactionOptions.Timeout = TransactionManager.DefaultTimeout;
        }

        if (transactionScopeAsyncFlowOption.HasValue)
        {
            transactionScope = new TransactionScope(
                transactionScopeOption,
                transactionOptions,
                transactionScopeAsyncFlowOption.Value);
        }
        else
        {
            transactionScope = new TransactionScope(
                transactionScopeOption,
                transactionOptions);
        }

        return transactionScope;
    }
}

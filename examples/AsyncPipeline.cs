using DrifterApps.Seeds.FluentResult;

namespace FluentResult.Examples;

// Demonstrates async pipeline patterns: chaining Task<Result<T>>,
// mixing sync and async steps, and error propagation without await mid-chain.
public sealed class OrderProcessor
{
    private readonly IOrderRepository _orders;
    private readonly IInventoryService _inventory;
    private readonly IPaymentGateway _payment;
    private readonly INotificationService _notifications;
    private readonly IAuditLog _audit;

    public OrderProcessor(
        IOrderRepository orders,
        IInventoryService inventory,
        IPaymentGateway payment,
        INotificationService notifications,
        IAuditLog audit)
    {
        _orders = orders;
        _inventory = inventory;
        _payment = payment;
        _notifications = notifications;
        _audit = audit;
    }

    // Full async pipeline: every step is awaited in sequence via chaining.
    // Any failure short-circuits the rest of the chain.
    public Task<Result<OrderConfirmation>> ProcessOrderAsync(
        Guid orderId, CancellationToken ct) =>
        _orders.FindByIdAsync(orderId, ct)
               .OnFailure(err => _audit.LogAsync("Order lookup failed", err, ct))
               .OnSuccess(order => _inventory.ReserveAsync(order, ct))
               .OnSuccess(order => _payment.ChargeAsync(order, ct))
               .OnSuccess(confirmation => _notifications.SendConfirmationAsync(confirmation, ct)
                   .OnSuccess(_ => confirmation))
               .OnFailure(err => _audit.LogAsync("Order processing failed", err, ct));

    // LINQ query syntax — preferred when steps have named intermediate values.
    public Task<Result<Invoice>> GenerateInvoiceAsync(Guid orderId, CancellationToken ct) =>
        from order    in _orders.FindByIdAsync(orderId, ct)
        from customer in _orders.FindCustomerAsync(order.CustomerId, ct)
        from invoice  in CreateInvoice(order, customer)
        from _        in _orders.SaveInvoiceAsync(invoice, ct)
        select invoice;

    // Mixing sync and async: implicit Task<Result<T>> conversion handles the transition.
    public Task<Result<OrderSummary>> SummarizeOrderAsync(Guid orderId, CancellationToken ct) =>
        _orders.FindByIdAsync(orderId, ct)
               .OnSuccess(order => BuildSummary(order)); // sync step — implicit conversion applied

    private static Result<Invoice> CreateInvoice(Order order, Customer customer) =>
        order.Lines.Count == 0
            ? InvoiceErrors.EmptyOrder
            : new Invoice(Guid.NewGuid(), order, customer);

    private static Result<OrderSummary> BuildSummary(Order order) =>
        new OrderSummary(order.Id, order.Lines.Count, order.Total);

    private static class InvoiceErrors
    {
        internal static readonly ResultError EmptyOrder =
            new("Invoice.EmptyOrder", "Cannot generate invoice for an order with no lines.");
    }
}

// Stub types for the example — replace with your actual domain.
public sealed record Order(Guid Id, Guid CustomerId, List<OrderLine> Lines, decimal Total);
public sealed record OrderLine(string ProductCode, int Quantity, decimal UnitPrice);
public sealed record Customer(Guid Id, string Name, string Email);
public sealed record OrderConfirmation(Guid OrderId, string ConfirmationNumber);
public sealed record Invoice(Guid Id, Order Order, Customer Customer);

public interface IOrderRepository
{
    Task<Result<Order>> FindByIdAsync(Guid id, CancellationToken ct);
    Task<Result<Customer>> FindCustomerAsync(Guid customerId, CancellationToken ct);
    Task<Result<Nothing>> SaveInvoiceAsync(Invoice invoice, CancellationToken ct);
}

public interface IInventoryService
{
    Task<Result<Order>> ReserveAsync(Order order, CancellationToken ct);
}

public interface IPaymentGateway
{
    Task<Result<OrderConfirmation>> ChargeAsync(Order order, CancellationToken ct);
}

public interface INotificationService
{
    Task<Result<Nothing>> SendConfirmationAsync(OrderConfirmation confirmation, CancellationToken ct);
}

public interface IAuditLog
{
    Task LogAsync(string message, ResultError error, CancellationToken ct);
}

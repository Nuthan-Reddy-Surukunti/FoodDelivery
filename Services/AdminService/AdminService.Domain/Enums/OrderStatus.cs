namespace AdminService.Domain.Enums;

public enum OrderStatus
{
    Pending = 1,

    Confirmed = 2,

    Preparing = 3,

    Ready = 4,

    OutForDelivery = 5,

    Delivered = 6,

    Cancelled = 7
}

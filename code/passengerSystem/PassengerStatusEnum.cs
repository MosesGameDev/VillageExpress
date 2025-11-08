public class PassengerStatusEnum
{
    public enum PassengerStatus 
    { 
        NONE = 1,
        WAITING_FOR_INTERACTION = 2,
        BOARDING_VEHICLE = 3,
        IN_TRANSIT = 4,
        REFUSED_BOARDING = 5,
        EXITING_VEHICLE = 6,
        EXITED_VEHICLE = 7
    };

    public PassengerStatusEnum Status;
}
